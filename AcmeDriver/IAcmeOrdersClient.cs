using System;
using System.Threading.Tasks;
using AcmeDriver.Certificates;

namespace AcmeDriver {
	public interface IAcmeOrdersClient {

		Task<AcmeOrder> GetOrderAsync(Uri location);

		Task<AcmeOrder> NewOrderAsync(AcmeOrder order);

		Task<AcmeOrder> FinalizeOrderAsync(AcmeOrder order, string csr);

		Task<string> DownloadCertificateAsync(AcmeOrder order);

		Task RevokeCertificateAsync(AcmeOrder order, CertificateRevokeReason reason);

	}
}