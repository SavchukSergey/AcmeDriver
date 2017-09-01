using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AcmeDriver.Handlers {
    public class AcmeExceptionHandler : DelegatingHandler {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                throw new AcmeException(content);
            }
            return response;
        }

    }
}
