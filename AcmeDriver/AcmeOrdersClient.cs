using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public class AcmeOrdersClient {

		private readonly AcmeAuthenticatedClientContext _context;

		public AcmeOrdersClient(AcmeAuthenticatedClientContext context) {
			_context = context;
		}

		public Task<AcmeOrder> GetOrderAsync(Uri location) {
			return _context.SendPostAsGetAsync<AcmeOrder>(location);
		}

		public async Task<AcmeOrder> NewOrderAsync(AcmeOrder order) {
			return await _context.SendPostKidAsync<object, AcmeOrder>(_context.Directory.NewOrderUrl, new {
				identifiers = order.Identifiers,
			}, (headers, ord) => {
				ord.Location = headers.Location ?? order.Location;
			}).ConfigureAwait(false);
		}

		public async Task<AcmeOrder> FinalizeOrderAsync(AcmeOrder order, string csr) {
			return await _context.SendPostKidAsync<object, AcmeOrder>(order.Finalize, new {
				csr = Base64Url.Encode(csr.GetPemCsrData())
			}, (headers, ord) => {
				ord.Location = headers.Location ?? order.Location;
			}).ConfigureAwait(false);
		}

		public Task<string> DownloadCertificateAsync(AcmeOrder order) {
			return _context.SendPostAsGetStringAsync(order.Certificate);
		}

	}
}