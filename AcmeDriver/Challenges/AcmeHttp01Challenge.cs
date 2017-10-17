using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcmeDriver {

    public class AcmeHttp01Challenge : AcmeChallenge {

        public string FileName { get; set; }

        public string FileDirectory => "/.well-known/acme-challenge/";

        public Uri FileUri => new Uri($"http://{Domain}{FileDirectory}{FileName}");

        public string FileContent { get; set; }

        public string CurlCmd => $"curl {FileUri}";

        public override async Task<bool> PrevalidateAsync() {
            try {
                using (var client = new HttpClient()) {
                    var responseContent = await client.GetStringAsync(FileUri);
                   return responseContent == FileContent;
                }
            } catch {
                return false;
            }
        }

    }

}