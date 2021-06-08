using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AcmeDriver.Handlers;
using AcmeDriver.Utils;

namespace AcmeDriver {
	public class AcmeClientContext : IDisposable {

		private readonly HttpClient _client;

		public string? Nonce { get; set; }

		public AcmeDirectory Directory { get; }
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public AcmeClientContext(AcmeDirectory directory) {
			Directory = directory;
			_client = new HttpClient(new AcmeExceptionHandler {
				InnerHandler = new AcmeNonceHandler(this) {
					InnerHandler = new HttpClientHandler {
					}
				}
			});
		}

		public async Task<TResult> SendGetAsync<TResult>(Uri uri, Action<HttpResponseHeaders, TResult>? headersHandler = null) where TResult : class {
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await SendAsync(request).ConfigureAwait(false);
			return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
		}

		public async Task SendHeadAsync(Uri uri) {
			var request = new HttpRequestMessage(HttpMethod.Head, uri);
			var response = await SendAsync(request).ConfigureAwait(false);
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
			if (uri == null) {
				throw new ArgumentNullException(nameof(uri));
			}
			if (registration == null) {
				throw new ArgumentNullException(nameof(registration));
			}
			var dataContent = AcmeJson.Serialize(model);
			var data = Encoding.UTF8.GetBytes(dataContent);
			return await UseNonceAsync(async nonce => {
				var signedContent = registration.SignKid(uri, nonce, data);

				var response = await PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
				return await ProcessRequestAsync(response, headersHandler).ConfigureAwait(false);
			});
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

			var data = Array.Empty<byte>();
			return await UseNonceAsync(async nonce => {
				var signedContent = registration.SignKid(uri, nonce, data);

				var response = await PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
				return response;
			});
		}

		public async Task<HttpResponseMessage> SendPostResponseAsync<TSource>(Uri uri, TSource model, AcmeClientRegistration registration) {
			if (uri == null) {
				throw new ArgumentNullException(nameof(uri));
			}
			if (registration == null) {
				throw new ArgumentNullException(nameof(registration));
			}
			var dataContent = AcmeJson.Serialize(model);
			var data = Encoding.UTF8.GetBytes(dataContent);
			return await UseNonceAsync(async nonce => {
				var signedContent = registration.Sign(uri, nonce, data);

				return await PostAsync(uri, GetStringContent(signedContent)).ConfigureAwait(false);
			});
		}

		public async Task InvalidateNonceAsync() {
			await _semaphore.WaitAsync();
			Nonce = null;
			_semaphore.Release();
		}

		public void Dispose() {
			_client.Dispose();
			_semaphore.Dispose();
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

		private async Task<TResult> UseNonceAsync<TResult>(Func<string, Task<TResult>> action) {
			await _semaphore.WaitAsync();
			try {
				if (string.IsNullOrWhiteSpace(Nonce)) {
					if (Directory.NewNonceUrl != null) {
						await SendHeadAsync(Directory.NewNonceUrl).ConfigureAwait(false);
					}
				}
				return await action(Nonce!);
			} finally {
				_semaphore.Release();
			}
		}

		private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) {
			return _client.SendAsync(request);
		}

		private Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content) {
			return _client.PostAsync(uri, content);
		}

	}
}