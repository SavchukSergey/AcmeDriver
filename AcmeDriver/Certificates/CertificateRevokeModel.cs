using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AcmeDriver.Certificates;

/// <summary>
/// https://tools.ietf.org/html/draft-ietf-acme-acme-18#section-7.6
/// </summary>
public class CertificateRevokeModel {
	[JsonPropertyName("certificate")]
	[Required]
	public string Certificate { get; set; }

	[JsonPropertyName("reason")]
	public CertificateRevokeReason Reason { get; set; } = CertificateRevokeReason.Unspecified;
}
