using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AcmeDriver.Handlers;

namespace AcmeDriver {
    public class AcmeClient : IDisposable {

        private string _nonce;
        private HttpClient _client;
        private AcmeDirectory _directory;

        public const string STAGING_URL = "https://acme-staging.api.letsencrypt.org";

        public AcmeClient(AcmeDirectory directory) {
            _directory = directory;
            var handler = new AcmeExceptionHandler {
                InnerHandler = new HttpClientHandler {
                }
            };
            _client = new HttpClient(handler);
        }

        public static async Task<AcmeClient> CreateAcmeClient(string baseUrl) {
            using (var client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/directory");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<AcmeDirectory>(responseContent);
                var acmeClient = new AcmeClient(res);
                acmeClient.CheckNonce(response);
                return acmeClient;
            }
        }

        public async Task<AcmeDirectory> GetDirectoryAsync() {
            return await SendGetAsync<AcmeDirectory>("directory");
        }

        public async Task<AcmeRegistration> NewRegistrationAsync(string[] contacts, RSA rsa = null) {
            if (rsa == null) {
                rsa = RSA.Create();
                rsa.KeySize = 2048;
            }
            var reg = new AcmeClientRegistration {
                Key = rsa
            };
            Registration = reg;
            var data = await SendPostAsync<object, AcmeRegistration>(_directory.NewRegUrl, new {
                resource = "new-reg",
                contact = contacts
            });
            reg.Id = data.Id;
            return data;
        }

        public async Task<AcmeRegistration> GetRegistrationAsync() {
            var data = await SendPostAsync<object, AcmeRegistration>($"/acme/reg/{Registration.Id}", new {
                resource = "reg"
            });
            return data;
        }

        public async Task UpdateRegistrationAsync() {
            await Task.FromResult(0);
        }


        public async Task<AcmeRegistration> AcceptAgreementAsync(string agreementUrl) {
            var data = await SendPostAsync<object, AcmeRegistration>($"/acme/reg/{Registration.Id}", new {
                resource = "reg",
                agreement = agreementUrl
            });
            return data;
        }

        public async Task<AcmeAuthorization> NewAuthorizationAsync(string domainName) {
            return await SendPostAsync<object, AcmeAuthorization>(_directory.NewAuthzUrl, new {
                resource = "new-authz",
                identifier = new {
                    type = "dns",
                    value = domainName
                }
            });
        }

        public async Task<Uri> NewOrderAsync(AcmeOrder order) {
            Uri location = null;
            await SendPostAsync<object>(_directory.NewCertUrl, new {
                resource = "new-cert",
                csr = Base64Url.Encode(order.Csr.GetPemCsrData()),
                notBefore = order.NotBefore.ToRfc3339String(),
                notAfter = order.NotAfter.ToRfc3339String(),
            }, (headers, ord) => {
                location = headers.Location;
            });
            return location;
        }

        public async Task<byte[]> DownloadCertificateAsync(Uri uri) {
            using (var client = new HttpClient()) {
                return await client.GetByteArrayAsync(uri);
            }
        }

        public async Task<AcmeAuthorization> GetAuthorizationAsync(Uri location) {
            var data = await SendGetAsync<AcmeAuthorization>(location);
            data.Location = location;
            return data;
        }

        public async Task<AcmeChallenge> CompleteChallengeAsync(AcmeChallenge challenge) {
            var data = await SendPostAsync<object, AcmeChallenge>(challenge.Uri, new {
                resource = "challenge",
                type = challenge.Type,
                keyAuthorization = challenge.GetKeyAuthorization(Registration)
            }, null);
            return data;
        }


        public AcmeClientRegistration Registration { get; set; }

        private string ComputeSignature(byte[] data) {
            if (Registration == null) {
                throw new Exception("registration is not set");
            }
            var signature = Registration.Key.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Base64Url.Encode(signature);
        }

        private string Sign(byte[] payload) {
            if (Registration == null) {
                throw new Exception("registration is not set");
            }
            var protectedHeader = new {
                nonce = _nonce
            };
            var protectedHeaderJson = JsonConvert.SerializeObject(protectedHeader);
            var protectedHeaderData = Encoding.UTF8.GetBytes(protectedHeaderJson);
            var protectedHeaderEncoded = Base64Url.Encode(protectedHeaderData);

            var payloadEncoded = Base64Url.Encode(payload);

            var tbs = protectedHeaderEncoded + "." + payloadEncoded;

            var json = new {
                payload = payloadEncoded,
                @protected = protectedHeaderEncoded,
                header = new {
                    typ = "JWT",
                    alg = "RS256",
                    jwk = Registration.GetJwk()
                },
                signature = ComputeSignature(Encoding.UTF8.GetBytes(tbs))
            };
            return JsonConvert.SerializeObject(json);
        }

        private async Task<TResult> SendPostAsync<TSource, TResult>(string url, TSource model) where TResult : AcmeResource {
            return await SendPostAsync<TSource, TResult>(url, model, (headers, authz) => {
                authz.Location = headers.Location;
            });
        }

        private async Task<TResult> SendPostAsync<TSource, TResult>(string url, TSource model, Action<HttpResponseHeaders, TResult> headersHandler) where TResult : class {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(data);

            var response = await _client.PostAsync(url, new StringContent(signedContent, Encoding.UTF8, "application/json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<string> SendPostAsync<TSource>(string url, TSource model, Action<HttpResponseHeaders, string> headersHandler = null) {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(data);

            var response = await _client.PostAsync(url, new StringContent(signedContent, Encoding.UTF8, "application/json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private Task<TResult> SendGetAsync<TResult>(string url, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            return SendGetAsync(new Uri(url, UriKind.Relative), headersHandler);
        }

        private async Task<TResult> SendGetAsync<TResult>(Uri url, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.SendAsync(request);
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<TResult> ProcessRequestAsync<TResult>(HttpResponseMessage response, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            CheckNonce(response);
            var responseContent = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<TResult>(responseContent);
            headersHandler?.Invoke(response.Headers, res);
            return res;
        }

        private async Task<string> ProcessRequestAsync(HttpResponseMessage response, Action<HttpResponseHeaders, string> headersHandler = null) {
            CheckNonce(response);
            var res = await response.Content.ReadAsStringAsync();
            headersHandler?.Invoke(response.Headers, res);
            return res;
        }

        private void CheckNonce(HttpResponseMessage response) {
            if (response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> replayNonce)) {
                _nonce = replayNonce.FirstOrDefault();
            }
        }

        public void Dispose() {
            _client.Dispose();
        }

    }
}
