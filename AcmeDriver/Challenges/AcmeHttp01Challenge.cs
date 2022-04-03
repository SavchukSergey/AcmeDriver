using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeHttp01Challenge : AcmeChallenge {

		public string FileName => Data.Token;

		public string FileDirectory => "/.well-known/acme-challenge/";

		public Uri FileUri => new Uri($"http://{Domain}{FileDirectory}{FileName}");

		public string FileContent { get; }

		public string CurlCmd => $"curl {FileUri}";

		public AcmeHttp01Challenge(AcmeChallengeData data, AcmeAuthorization authorization, AcmeClientRegistration registration) : base(data, authorization, registration) {
			FileContent = data.GetKeyAuthorization(registration);
		}

		public override async Task<bool> PrevalidateAsync() {
			try {
				using (var client = new HttpClient()) {
					var responseContent = await client.GetStringAsync(FileUri).ConfigureAwait(false);
					return responseContent == FileContent;
				}
			} catch {
				return false;
			}
		}

	}

}