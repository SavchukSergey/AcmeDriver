using System;
using System.Text.Json.Serialization;

namespace AcmeDriver {
    public class AcmeDirectory {

        public Uri DirectoryUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-04</para>
        ///</summary>
        [JsonPropertyName("newNonce")]
        public Uri NewNonceUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonPropertyName("newAccount")]
        public Uri NewAccountUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonPropertyName("newOrder")]
        public Uri NewOrderUrl { get; set; }

        ///<summary>
        ///<para>Unavailable in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonPropertyName("newAuthz")]
        public Uri NewAuthzUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///</summary>
        [JsonPropertyName("revokeCert")]
        public Uri RevokeCertUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonPropertyName("keyChange")]
        public Uri KeyChangeUrl { get; set; }

        [JsonPropertyName("meta")]
        public AcmeDirectoryMeta Meta { get; set; }

        public static AcmeDirectory FromBaseUrl(Uri baseUrl) {
            return new AcmeDirectory {
                DirectoryUrl = new Uri(baseUrl, "directory"),
                NewNonceUrl = new Uri(baseUrl, "acme/new-nonce"),
                NewAccountUrl = new Uri(baseUrl, "acme/new-acct"),
                NewOrderUrl = new Uri(baseUrl, "acme/new-order"),
                NewAuthzUrl = new Uri(baseUrl, "acme/new-authz")
            };
        }

    }
}
