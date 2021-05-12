using System;
using System.Threading.Tasks;

namespace AcmeDriver {
	public interface IAcmeOrdersClient {

		Task<AcmeOrder> GetOrderAsync(Uri location);

		Task<AcmeOrder> NewOrderAsync(AcmeOrder order);

		Task<AcmeOrder> FinalizeOrderAsync(AcmeOrder order, string csr);

		Task<string> DownloadCertificateAsync(AcmeOrder order);

	}
}