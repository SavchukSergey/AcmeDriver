using Newtonsoft.Json;

namespace AcmeDriver {
    public class AcmeDirectory {

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-04</para>
        ///</summary>
        [JsonProperty("new-nonce")]
        public string NewNonceUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonProperty("new-account")]
        public string NewAccountUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonProperty("key-change")]
        public string KeyChangeUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-05</para>
        ///</summary>
        [JsonProperty("new-order")]
        public string NewOrderUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-05. Use <see cref="M:NewOrderUrl" /></para>
        ///</summary>
        [JsonProperty("new-app")]
        public string NewAppUrl { get; set; }


        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-03. Use <see cref="M:NewAppUrl" /></para>
        ///</summary>
        [JsonProperty("new-cert")]
        public string NewCertUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///</summary>
        [JsonProperty("revoke-cert")]
        public string RevokeCertUrl { get; set; }


        ///<summary>
        ///<para>Unavailable in https://tools.ietf.org/html/draft-ietf-acme-acme-03</para>
        ///</summary>
        [JsonProperty("new-authz")]
        public string NewAuthUrl { get; set; }

        ///<summary>
        /// <para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        /// <para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-05. Use <see cref="M:NewAccountUrl" /></para>
        ///</summary>
        [JsonProperty("new-reg")]
        public string NewRegUrl { get; set; }

        ///<summary>
        ///<para>Introduced in https://tools.ietf.org/html/draft-ietf-acme-acme-01</para>
        ///<para>Removed in https://tools.ietf.org/html/draft-ietf-acme-acme-02</para>
        ///</summary>
        [JsonProperty("recover-reg")]
        public string RecoverRegUrl { get; set; }

    }
}
