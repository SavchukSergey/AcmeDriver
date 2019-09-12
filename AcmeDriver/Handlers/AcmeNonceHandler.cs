using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeDriver {
    public class AcmeNonceHandler : DelegatingHandler {

        private readonly AcmeClient _client;

        public AcmeNonceHandler(AcmeClient client) {
            _client = client;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.Headers.TryGetValues("Replay-Nonce", out IEnumerable<string> replayNonce)) {
                _client.Nonce = replayNonce.FirstOrDefault();
            }

            return response;
        }

    }
}
