using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AcmeDriver.Utils;

namespace AcmeDriver.Handlers {
    public class AcmeExceptionHandler : DelegatingHandler {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null && contentType.Contains("application") && contentType.Contains("json")) {
                    var res = AcmeJson.Deserialize<AcmeExceptionInfo>(content);
                    throw new AcmeException(res);
                } else {
                    throw new AcmeException(new AcmeExceptionInfo {
                        Detail = content,
                        Status = (int)response.StatusCode,
                        Type = "http"
                    });
                }
            }
            return response;
        }

    }

}
