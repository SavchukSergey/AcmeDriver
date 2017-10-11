using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcmeDriver.Handlers {
    public class AcmeExceptionHandler : DelegatingHandler {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<AcmeExceptionInfo>(content);
                throw new AcmeException(res);
            }
            return response;
        }

    }

}
