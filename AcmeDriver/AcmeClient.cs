using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeClient {

        private readonly string _baseUrl;
        private string _nonce;

        public AcmeClient(string baseUrl) {
            _baseUrl = baseUrl;
        }

        public async Task<AcmeDirectory> GetDirectoryAsync() {
            return await SendGetAsync<AcmeDirectory>("directory");
        }

        public async Task<AcmeRegistration> NewRegistrationAsync(string[] contacts, RSA rsa = null) {
            if (rsa == null) {
                rsa = new RSACryptoServiceProvider(2048);
            }
            var reg = new AcmeClientRegistration {
                Key = rsa
            };
            Registration = reg;
            var data = await SendPostAsync<object, AcmeRegistration>("/acme/new-reg", new {
                resource = "new-reg",
                contact = contacts
            });
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
                authz.Location = headers["Location"];
            });
            return data;
        }


        public async Task<AcmeAuthorization> GetAuthorizationAsync(string location) {
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
            var signature = Registration.Key.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Base64Url.Encode(signature);
        }

        private string Sign(byte[] payload) {
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

        private string FormatUrl(string relUrl) {
            if (relUrl.StartsWith("https://")) return relUrl;
            return (_baseUrl.TrimEnd('/') + '/' + relUrl.TrimStart('/')).TrimEnd('/');
        }

        private async Task<TResult> SendPostAsync<TSource, TResult>(string url, TSource model, Action<WebHeaderCollection, TResult> headersHandler = null) where TResult : class {
            var dataContent = JsonConvert.SerializeObject(model);
            var data = Encoding.UTF8.GetBytes(dataContent);
            var signedContent = Sign(data);
            var signedData = Encoding.UTF8.GetBytes(signedContent);

            var webRequest = WebRequest.Create(FormatUrl(url));
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = signedData.Length;
            var requestStream = await webRequest.GetRequestStreamAsync();
            await requestStream.WriteAsync(signedData, 0, signedData.Length);

            return await ProcessRequest(webRequest, headersHandler);
        }

        private async Task<TResult> SendGetAsync<TResult>(string url, Action<WebHeaderCollection, TResult> headersHandler = null) where TResult : class {
            var webRequest = (HttpWebRequest)WebRequest.Create(FormatUrl(url));
            webRequest.Method = "GET";
            webRequest.Accept = "application/json";
            return await ProcessRequest(webRequest, headersHandler);
        }

        private async Task<TResult> ProcessRequest<TResult>(WebRequest webRequest, Action<WebHeaderCollection, TResult> headersHandler = null) where TResult : class {
            try {
                var response = await webRequest.GetResponseAsync();
                CheckNonce(response);
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return null;
                var responseReader = new StreamReader(responseStream);
                var responseContent = await responseReader.ReadToEndAsync();
                var res = JsonConvert.DeserializeObject<TResult>(responseContent);
                headersHandler?.Invoke(response.Headers, res);
                return res;
            } catch (WebException exc) {
                var excResponse = exc.Response;
                var responseStream = excResponse.GetResponseStream();
                if (responseStream == null) throw;
                var responseReader = new StreamReader(responseStream);
                var responseContent = await responseReader.ReadToEndAsync();
                throw new Exception(responseContent);
            }
        }

        private void CheckNonce(WebResponse response) {
            var replayNonce = response.Headers["Replay-Nonce"];
            if (!string.IsNullOrWhiteSpace(replayNonce)) {
                _nonce = replayNonce;
            }
        }
        /*


        Get-Identifier
        Update-Identifier 

        Get-IssuerCertificate
        New-Certificate
        Get-Certificate 
        Submit-Certificate 
        Update-Certificate

        Get-ChallengeHandlerProfile
        Set-ChallengeHandlerProfile

        Set-Proxy
        Set-ServerDirectory 

        Get-Vault 
        Get-VaultProfile
        Set-Vault 
        Initialize-Vault
        Set-VaultProfile

         */
    }
}
