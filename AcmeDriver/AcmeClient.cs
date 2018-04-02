using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AcmeDriver.Handlers;
using AcmeDriver.JWK;

namespace AcmeDriver {
    public class AcmeClient : IDisposable {

        private readonly HttpClient _client;
        private readonly AcmeDirectory _directory;

        public AcmeClientRegistration Registration { get; set; }

        public string Nonce { get; set; }

        public const string LETS_ENCRYPT_STAGING_URL = "https://acme-staging-v02.api.letsencrypt.org";
        public const string LETS_ENCRYPT_PRODUCTION_URL = "https://acme-v02.api.letsencrypt.org";

        public AcmeClient(string baseUrl) : this(AcmeDirectory.FromBaseUrl(baseUrl)) {
        }

        public AcmeClient(AcmeDirectory directory) {
            _directory = directory;
            _client = new HttpClient(new AcmeExceptionHandler {
                InnerHandler = new AcmeNonceHandler(this) {
                    InnerHandler = new HttpClientHandler {
                    }
                }
            });
        }

        public static async Task<AcmeClient> CreateAcmeClient(string baseUrl) {
            using (var client = new HttpClient()) {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/directory");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/jose+json"));
                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<AcmeDirectory>(responseContent);
                var acmeClient = new AcmeClient(res);
                return acmeClient;
            }
        }

        public async Task<AcmeDirectory> GetDirectoryAsync() {
            return await GetDirectoryAsync(_directory.DirectoryUrl);
        }

        public async Task<AcmeDirectory> GetDirectoryAsync(string directoryUrl) {
            return await SendGetAsync<AcmeDirectory>(new Uri(directoryUrl));
        }

        #region Registrations

        private PrivateJsonWebKey GenerateKey() {
#if NETCOREAPP2_0
            return EccPrivateJwk.Create();
#else
            return RsaPrivateJwk.Create();
#endif
        }

        public async Task<AcmeRegistration> NewRegistrationAsync(string[] contacts, PrivateJsonWebKey key = null) {
            key = key ?? GenerateKey();
            await EnsureNonceAsync();
            var reg = new AcmeClientRegistration {
                Key = key,
            };
            Registration = reg;
            var data = await SendPostAsync<object, AcmeRegistration>(new Uri(_directory.NewAccountUrl), new {
                contact = contacts,
                termsOfServiceAgreed = true
            });
            reg.Id = data.Id;
            reg.Location = data.Location;
            return data;
        }

        public async Task<AcmeRegistration> GetRegistrationAsync(Uri registrationUri) {
            await EnsureNonceAsync();
            var data = await SendPostKidAsync<object, AcmeRegistration>(registrationUri, new { });
            return data;
        }

        public async Task UpdateRegistrationAsync(Uri registrationUri) {
            await Task.FromResult(0);
        }

        public async Task<AcmeRegistration> AcceptRegistrationAgreementAsync(string agreementUrl) {
            //$"/acme/reg/{Registration.Id}"
            var data = await SendPostKidAsync<object, AcmeRegistration>(Registration.Location, new {
                resource = "reg",
                agreement = agreementUrl
            });
            return data;
        }

        #endregion

        #region Authorizations

        public async Task<AcmeAuthorization> NewAuthorizationAsync(AcmeIdentifier identifier) {
            await EnsureNonceAsync();
            return await SendPostAsync<object, AcmeAuthorization>(new Uri(_directory.NewAuthzUrl), new {
                resource = "new-authz",
                identifier = new {
                    type = identifier.Type,
                    value = identifier.Value
                }
            });
        }

        public async Task<AcmeAuthorization> NewAuthorizationAsync(string domainName) {
            return await NewAuthorizationAsync(new AcmeIdentifier {
                Type = "dns",
                Value = domainName
            });
        }

        public async Task<AcmeAuthorization> GetAuthorizationAsync(Uri location) {
            var data = await SendGetAsync<AcmeAuthorization>(location);
            data.Location = location;
            return data;
        }

        ///<summary>
        ///<para>Deactivates authorization.</para>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        public async Task DeactivateAuthorizationAsync(Uri authorizationUri) {
            await SendPostAsync(authorizationUri, new {
                status = AcmeAuthorizationStatus.Deactivated.ToString().ToLower()
            });
        }

        ///<summary>
        ///<para>Deletes authorization.</para>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-02</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-03. Use <see cref="M:DeactivateAuthorizationAsync" /></para>
        ///</summary>
        public async Task DeleteAuthorizationAsync(Uri authorizationUri) {
            await SendPostAsync(authorizationUri, new {
                resource = "authz",
                delete = true
            });
        }

        #endregion

        #region Orders

        public async Task<AcmeOrder> GetOrderAsync(Uri location) {
            return await SendGetAsync<AcmeOrder>(location);
        }

        public async Task<AcmeOrder> NewOrderAsync(AcmeOrder order) {
            await NewNonceAsync();
            return await SendPostKidAsync<object, AcmeOrder>(new Uri(_directory.NewOrderUrl), new {
                identifiers = order.Identifiers,
            }, (headers, ord) => {
                ord.Location = headers.Location;
            });
        }

        public async Task<AcmeOrder> FinalizeOrderAsync(AcmeOrder order, string csr) {
            await NewNonceAsync();
            return await SendPostKidAsync<object, AcmeOrder>(new Uri(order.Finalize), new {
                csr = Base64Url.Encode(csr.GetPemCsrData())
            }, (headers, ord) => {
                ord.Location = headers.Location;
            });
        }

        public async Task<byte[]> DownloadCertificateAsync(Uri uri) {
            using (var client = new HttpClient()) {
                return await client.GetByteArrayAsync(uri);
            }
        }

        public async Task<string> DownloadCertificateAsync(AcmeOrder order) {
            using (var client = new HttpClient()) {
                return await client.GetStringAsync(order.Certificate);
            }
        }

        #endregion

        #region Challenges

        public async Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallenge challenge) {
            return await CompleteChallengeAsync(challenge.Data);
        }

        public async Task<AcmeChallengeData> CompleteChallengeAsync(AcmeChallengeData challenge) {
            await NewNonceAsync();
            var data = await SendPostKidAsync<object, AcmeChallengeData>(new Uri(challenge.Uri), new {
                type = challenge.Type,
                keyAuthorization = challenge.GetKeyAuthorization(Registration)
            }, null);
            return data;
        }

        #endregion

        public async Task NewNonceAsync() {
            if (!string.IsNullOrWhiteSpace(_directory.NewNonceUrl)) {
                await SendHeadAsync(new Uri(_directory.NewNonceUrl));
            } else {
                await GetDirectoryAsync();
            }
        }

        private string ComputeSignature(byte[] data) {
            if (Registration == null) {
                throw new Exception("registration is not set");
            }
            var signature = Registration.Key.SignData(data);
            return Base64Url.Encode(signature);
        }

        private string Sign(Uri url, byte[] payload) {
            if (Registration == null) {
                throw new Exception("registration is not set");
            }
            var protectedHeader = new {
                nonce = Nonce,
                url = url.ToString(),
                alg = GetSignatureAlg(),
                jwk = Registration.Key.GetPublicJwk()
            };
            var protectedHeaderJson = JsonConvert.SerializeObject(protectedHeader);
            var protectedHeaderData = Encoding.UTF8.GetBytes(protectedHeaderJson);
            var protectedHeaderEncoded = Base64Url.Encode(protectedHeaderData);

            var payloadEncoded = Base64Url.Encode(payload);

            var tbs = protectedHeaderEncoded + "." + payloadEncoded;

            var json = new {
                payload = payloadEncoded,
                @protected = protectedHeaderEncoded,
                signature = ComputeSignature(Encoding.UTF8.GetBytes(tbs))
            };
            return JsonConvert.SerializeObject(json);
        }

        private string SignKid(Uri url, byte[] payload) {
            if (Registration == null) {
                throw new Exception("registration is not set");
            }
            var protectedHeader = new {
                nonce = Nonce,
                url = url.ToString(),
                alg = GetSignatureAlg(),
                kid = Registration.Location.ToString()
            };
            var protectedHeaderJson = JsonConvert.SerializeObject(protectedHeader);
            var protectedHeaderData = Encoding.UTF8.GetBytes(protectedHeaderJson);
            var protectedHeaderEncoded = Base64Url.Encode(protectedHeaderData);

            var payloadEncoded = Base64Url.Encode(payload);

            var tbs = protectedHeaderEncoded + "." + payloadEncoded;

            var json = new {
                payload = payloadEncoded,
                @protected = protectedHeaderEncoded,
                signature = ComputeSignature(Encoding.UTF8.GetBytes(tbs))
            };
            return JsonConvert.SerializeObject(json);
        }

        private string GetSignatureAlg() {
            var key = Registration?.Key?.Kty;
            switch(key) {
                case "RSA": return "RS256";
#if NETCOREAPP2_0
                case "EC": return "ES256";
#endif
                default: throw new NotSupportedException($"{key} key is not supported");
            }
        }

        private async Task<TResult> SendPostAsync<TSource, TResult>(Uri uri, TSource model) where TResult : AcmeResource {
            return await SendPostAsync<TSource, TResult>(uri, model, (headers, authz) => {
                authz.Location = headers.Location;
            });
        }

        private async Task<TResult> SendPostAsync<TSource, TResult>(Uri uri, TSource model, Action<HttpResponseHeaders, TResult> headersHandler) where TResult : class {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(uri, data);

            var response = await _client.PostAsync(uri, new StringContent(signedContent, Encoding.UTF8, "application/jose+json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<string> SendPostKidAsync<TSource>(Uri uri, TSource model, Action<HttpResponseHeaders, string> headersHandler = null) {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = SignKid(uri, data);

            var response = await _client.PostAsync(uri, new StringContent(signedContent, Encoding.UTF8, "application/jose+json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<TResult> SendPostKidAsync<TSource, TResult>(Uri uri, TSource model, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = SignKid(uri, data);

            var response = await _client.PostAsync(uri, new StringContent(signedContent, Encoding.UTF8, "application/jose+json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<string> SendPostAsync<TSource>(Uri uri, TSource model, Action<HttpResponseHeaders, string> headersHandler = null) {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(uri, data);

            var response = await _client.PostAsync(uri, new StringContent(signedContent, Encoding.UTF8, "application/jose+json"));
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task<TResult> SendGetAsync<TResult>(Uri uri, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.SendAsync(request);
            return await ProcessRequestAsync(response, headersHandler);
        }

        private async Task SendHeadAsync(Uri uri) {
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = await _client.SendAsync(request);
            await ProcessRequestAsync(response);
        }

        private async Task<TResult> ProcessRequestAsync<TResult>(HttpResponseMessage response, Action<HttpResponseHeaders, TResult> headersHandler = null) where TResult : class {
            var responseContent = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<TResult>(responseContent);
            headersHandler?.Invoke(response.Headers, res);
            return res;
        }

        private async Task<string> ProcessRequestAsync(HttpResponseMessage response, Action<HttpResponseHeaders, string> headersHandler = null) {
            var res = await response.Content.ReadAsStringAsync();
            headersHandler?.Invoke(response.Headers, res);
            return res;
        }

        private async Task EnsureNonceAsync() {
            if (!string.IsNullOrWhiteSpace(_directory.NewNonceUrl)) {
                await NewNonceAsync();
            } else {
                await GetDirectoryAsync();
            }
        }

        public void Dispose() {
            _client.Dispose();
        }

    }
}
