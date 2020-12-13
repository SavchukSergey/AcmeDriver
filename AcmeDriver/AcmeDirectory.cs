using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeDirectory {

        public string DirectoryUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-04</para>
        ///</summary>
        [JsonPropertyName("newNonce")]
        public string NewNonceUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonPropertyName("newAccount")]
        public string NewAccountUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonPropertyName("keyChange")]
        public string KeyChangeUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonPropertyName("newOrder")]
        public string NewOrderUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-05. Use <see cref="M:NewOrderUrl" /></para>
        ///</summary>
        [JsonPropertyName("new-app")]
        public string NewAppUrl { get; set; }


        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-03. Use <see cref="M:NewAppUrl" /></para>
        ///</summary>
        [JsonPropertyName("new-cert")]
        public string NewCertUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///</summary>
        [JsonPropertyName("revokeCert")]
        public string RevokeCertUrl { get; set; }


        ///<summary>
        ///<para>Unavailable in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonPropertyName("newAuthz")]
        public string NewAuthzUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-02</para>
        ///</summary>
        [JsonPropertyName("recover-reg")]
        public string RecoverRegUrl { get; set; }

        public static AcmeDirectory FromBaseUrl(string baseUrl) {
            return new AcmeDirectory {
                DirectoryUrl = $"{baseUrl}/directory",
                NewNonceUrl = $"{baseUrl}/acme/new-nonce",
                NewAccountUrl = $"{baseUrl}/acme/new-acct",
                NewOrderUrl = $"{baseUrl}/acme/new-order",
                NewAuthzUrl = $"{baseUrl}/acme/new-authz",
                NewCertUrl = $"{baseUrl}/acme/new-cert"
            };
        }

    }
}
