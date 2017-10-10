using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using AcmeDriver.Handlers;

namespace AcmeDriver {
    public class AcmeClient {

        private readonly Uri _baseUrl;
        private string _nonce;

        public const string STAGING_URL = "https://acme-staging.api.letsencrypt.org";

        public AcmeClient(string baseUrl) {
            _baseUrl = new Uri(baseUrl);
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
            var data = await SendPostAsync<object, AcmeRegistration>("/acme/new-reg", new {
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
            var data = await SendPostAsync<object, AcmeAuthorization>("/acme/new-authz", new {
                resource = "new-authz",
                identifier = new {
                    type = "dns",
                    value = domainName
                }
            }, (headers, authz) => {
                authz.Location = headers.Location;
            });
            return data;
        }

        public async Task<AcmeOrder> NewOrderAsync(AcmeOrder order) {
            var data = await SendPostAsync<object, AcmeOrder>("/acme/new-cert", new {
                resource = "new-cert",
                csr = Base64Url.Encode(order.Csr.GetPemCsrData()),
                notBefore = order.NotBefore.ToRfc3339String(),
                notAfter = order.NotAfter.ToRfc3339String(),
            }, (headers, ord) => {
                ord.Location = headers.Location;
            });
            return data;
        }

        public async Task<AcmeAuthorization> GetAuthorizationAsync(Uri location) {
            var data = await SendGetAsync<AcmeAuthorization>(location);
            data.Location = location;
            return data;
        }

        public async Task<AcmeChallenge> SubmitChallenge(AcmeChallenge challenge) {
            var data = await SendPostAsync<object, AcmeChallenge>(challenge.Uri, challenge.GetResponse());
            return data;
        }

        public async Task<AcmeChallenge> CompleteChallenge(AcmeChallenge challenge) {
            var data = await SendPostAsync<object, AcmeChallenge>(challenge.Uri, challenge.GetResponse());
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

        private async Task<TResult> SendPostAsync<TSource, TResult>(string url, TSource model, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(data);

            using (var client = CreateHttpClient()) {
                var response = await client.PostAsync(url, new StringContent(signedContent, Encoding.UTF8, "application/json"));
                return await ProcessRequest(response, headersHandler);
            }
        }

        private Task<TResult> SendGetAsync<TResult>(string url, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            return SendGetAsync(new Uri(url, UriKind.Relative), headersHandler);
        }

        private async Task<TResult> SendGetAsync<TResult>(Uri url, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            using (var client = CreateHttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.SendAsync(request);
                return await ProcessRequest(response, headersHandler);
            }
        }

        private async Task<TResult> ProcessRequest<TResult>(HttpResponseMessage response, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            CheckNonce(response);
            var responseContent = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<TResult>(responseContent);
            headersHandler?.Invoke(response.Headers, res);
            return res;
        }

        private void CheckNonce(HttpResponseMessage response) {
            if (response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> replayNonce)) {
                _nonce = replayNonce.FirstOrDefault();
            }
        }

        private HttpClient CreateHttpClient() {
            var handler = new AcmeExceptionHandler {
                InnerHandler = new HttpClientHandler {

                }
            };
            return new HttpClient(handler) {
                BaseAddress = _baseUrl
            };
        }

    }
}
