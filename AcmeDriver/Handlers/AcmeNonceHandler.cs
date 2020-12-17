using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeDriver {
    public class AcmeNonceHandler : DelegatingHandler {

        private readonly AcmeClientContext _context;

        public AcmeNonceHandler(AcmeClientContext context) {
            _context = context;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            _context.Nonce = null;
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.Headers.TryGetValues("Replay-Nonce", out var replayNonce)) {
                _context.Nonce = replayNonce.FirstOrDefault();
            }

            return response;
        }

    }
}
