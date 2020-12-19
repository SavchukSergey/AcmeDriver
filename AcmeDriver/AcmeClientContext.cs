using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AcmeDriver.Handlers;
using AcmeDriver.Utils;

namespace AcmeDriver {
	public class AcmeClientContext : IDisposable {

		private readonly HttpClient _client;

		public string? Nonce { get; set; }

		public AcmeDirectory Directory { get; }

		public AcmeClientContext(AcmeDirectory directory) {
			Directory = directory;
			_client = new HttpClient(new AcmeExceptionHandler {
				InnerHandler = new AcmeNonceHandler(this) {
					InnerHandler = new HttpClientHandler {
					}
				}
			});
		}

        public async Task<string> EnsureNonceAsync() {
			if (Nonce != null) {
				return Nonce;
			}
            if (Directory.NewNonceUrl != null) {
                return await NewNonceAsync().ConfigureAwait(false);
            } else {
                return string.Empty;
            }
        }

        public async Task<TResult> SendGetAsync<TResult>(Uri uri, Action<HttpResponseHeaders, TResult>? headersHandler = null) where TResult : class {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.SendAsync(request).ConfigureAwait(false);
            return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
        }

        public async Task SendHeadAsync(Uri uri) {
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var response = await _client.SendAsync(request).ConfigureAwait(false);
            await ProcessRequestAsync(response).ConfigureAwait(false);
        }

		public Task<TResult> SendPostAsync<TSource, TResult>(Uri uri, TSource model, AcmeClientRegistration registration) where TResult : AcmeResource {
			return SendPostAsync<TSource, TResult>(uri, model, registration, (headers, authz) => {
				authz.Location = headers.Location ?? authz.Location;
			});
		}

		public Task SendPostVoidAsync<TSource>(Uri uri, TSource model, AcmeClientRegistration registration) {
			return SendPostResponseAsync(uri, model, registration);
		}

		public async Task<string> SendPostStringAsync<TSource>(Uri uri, TSource model, AcmeClientRegistration registration, Action<HttpResponseHeaders, string>? headersHandler = null) {
			var response = await SendPostResponseAsync(uri, model, registration).ConfigureAwait(false);
			return await ProcessRequestStringAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task<TResult> SendPostAsync<TSource, TResult>(Uri uri, TSource model, AcmeClientRegistration registration, Action<HttpResponseHeaders, TResult> headersHandler) where TResult : class {
			var response = await SendPostResponseAsync(uri, model, registration).ConfigureAwait(false);
			return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task<TResult> SendPostKidAsync<TSource, TResult>(Uri uri, TSource model, AcmeClientRegistration registration, Action<HttpResponseHeaders, TResult>? headersHandler = null) where TResult : class {
			var nonce = await EnsureNonceAsync();
			if (uri == null) {
				throw new ArgumentNullException(nameof(uri));
			}
			if (registration == null) {
				throw new ArgumentNullException(nameof(registration));
			}
			var dataContent = AcmeJson.Serialize(model);
			var data = Encoding.UTF8.GetBytes(dataContent);
			var signedContent = registration.SignKid(uri, nonce, data);

			var response = await _client.PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
			return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task<TResult> SendPostAsGetAsync<TResult>(Uri uri, AcmeClientRegistration registration, Action<HttpResponseHeaders, TResult>? headersHandler = null) where TResult : class {
			var response = await SendPostAsGetResponseAsync(uri, registration).ConfigureAwait(false);
			return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task<string> SendPostAsGetStringAsync(Uri uri, AcmeClientRegistration registration, Action<HttpResponseHeaders, string>? headersHandler = null) {
			var response = await SendPostAsGetResponseAsync(uri, registration).ConfigureAwait(false);
			return await ProcessRequestStringAsync(response, headersHandler).ConfigureAwait(false);
		}

		private async Task<byte[]> SendPostAsGetBytesAsync(Uri uri, AcmeClientRegistration registration, Action<HttpResponseHeaders, byte[]>? headersHandler = null) {
			var response = await SendPostAsGetResponseAsync(uri, registration).ConfigureAwait(false);
			return await ProcessRequestBytesAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task<HttpResponseMessage> SendPostAsGetResponseAsync(Uri uri, AcmeClientRegistration registration) {
			if (uri == null) {
				throw new ArgumentNullException(nameof(uri));
			}
			if (registration == null) {
				throw new ArgumentNullException(nameof(registration));
			}
			var nonce = await EnsureNonceAsync();

			var data = new byte[0];
			var signedContent = registration.SignKid(uri, nonce, data);

			var response = await _client.PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
			return response;
		}

		public async Task<HttpResponseMessage> SendPostResponseAsync<TSource>(Uri uri, TSource model, AcmeClientRegistration registration) {
			if (uri == null) {
				throw new ArgumentNullException(nameof(uri));
			}
			if (registration == null) {
				throw new ArgumentNullException(nameof(registration));
			}
			var nonce = await EnsureNonceAsync();
			var dataContent = AcmeJson.Serialize(model);
			var data = Encoding.UTF8.GetBytes(dataContent);
			var signedContent = registration.Sign(uri, nonce, data);

			return await _client.PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
		}

		public void Dispose() {
			_client.Dispose();
		}

		private StringContent GetStringContent(string val) {
			return new StringContent(val, null, "application/jose+json") {
				Headers = {
					ContentType = {
						CharSet = string.Empty //letsencrypt fails if charset is specified
                    }
				}
			};
		}

		private async Task<TResult> ProcessRequestAsync<TResult>(HttpResponseMessage response, Action<HttpResponseHeaders, TResult>? headersHandler = null) where TResult : class {
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var res = AcmeJson.Deserialize<TResult>(responseContent);
			headersHandler?.Invoke(response.Headers, res);
			return res;
		}

		private async Task<string> ProcessRequestStringAsync(HttpResponseMessage response, Action<HttpResponseHeaders, string>? headersHandler = null) {
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			headersHandler?.Invoke(response.Headers, responseContent);
			return responseContent;
		}

		private async Task<byte[]> ProcessRequestBytesAsync(HttpResponseMessage response, Action<HttpResponseHeaders, byte[]>? headersHandler = null) {
			var responseContent = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			headersHandler?.Invoke(response.Headers, responseContent);
			return responseContent;
		}

		private async Task<string> ProcessRequestAsync(HttpResponseMessage response, Action<HttpResponseHeaders, string>? headersHandler = null) {
			var res = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			headersHandler?.Invoke(response.Headers, res);
			return res;
		}

        private async Task<string> NewNonceAsync() {
            if (Directory.NewNonceUrl != null) {
                await SendHeadAsync(Directory.NewNonceUrl).ConfigureAwait(false);
                return Nonce;
            } else {
                return string.Empty;
            }
        }

	}
}