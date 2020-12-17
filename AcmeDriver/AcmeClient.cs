using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AcmeDriver.JWK;
using AcmeDriver.Utils;

namespace AcmeDriver {
	public class AcmeClient : IDisposable {

        private readonly AcmeClientContext _context;
        public AcmeDirectory Directory { get; }

        public static readonly Uri LETS_ENCRYPT_STAGING_URL = new Uri("https://acme-staging-v02.api.letsencrypt.org");
        public static readonly Uri LETS_ENCRYPT_PRODUCTION_URL = new Uri("https://acme-v02.api.letsencrypt.org");

        public AcmeClient(Uri baseUrl) : this(AcmeDirectory.FromBaseUrl(baseUrl)) {
        }

        public AcmeClient(AcmeDirectory directory) {
            Directory = directory;
            _context = new AcmeClientContext(directory);
        }

        public async Task<AcmeAuthenticatedClient> AuthenticateAsync(PrivateJsonWebKey key) {
            var registration = await GetRegistrationAsync(key);
            return new AcmeAuthenticatedClient(new AcmeAuthenticatedClientContext(_context, registration));
        }

        public async Task<AcmeAuthenticatedClient> RegisterAsync(string[] contacts, PrivateJsonWebKey? key = null) {
            key = key ?? GenerateKey();
            var reg = new AcmeClientRegistration(key, new Uri("https://acmedriver.com"));
            var data = await _context.SendPostAsync<object, AcmeRegistration>(Directory.NewAccountUrl, new {
                contact = contacts,
                termsOfServiceAgreed = true
            }, reg).ConfigureAwait(false);
            reg = new AcmeClientRegistration(key, data.Location);
            return new AcmeAuthenticatedClient(new AcmeAuthenticatedClientContext(_context, reg));
        }

        public static async Task<AcmeClient> CreateAcmeClientAsync(Uri baseUrl) {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/directory");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.SendAsync(request).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var res = AcmeJson.Deserialize<AcmeDirectory>(responseContent);
            var acmeClient = new AcmeClient(res);
            return acmeClient;
        }

        public void Dispose() {
            _context.Dispose();
        }

        private PrivateJsonWebKey GenerateKey() {
            return RsaPrivateJwk.Create();
        }

        private async Task<AcmeClientRegistration> GetRegistrationAsync(PrivateJsonWebKey key) {
            var reg = new AcmeClientRegistration(key, new Uri("https://acmedriver.com"));
            var data = await _context.SendPostAsync<object, AcmeRegistration>(Directory.NewAccountUrl, new {
                onlyReturnExisting = true
            }, reg).ConfigureAwait(false);
            return new AcmeClientRegistration(key, data.Location);
        }

    }
}
