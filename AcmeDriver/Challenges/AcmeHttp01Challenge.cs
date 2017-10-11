using System;

namespace AcmeDriver {

    public class AcmeHttp01Challenge : AcmeChallenge2 {

        public string FileName { get; set; }

        public string FileDirectory => "/.well-known/acme-challenge/";

        public Uri FileUri => new Uri($"http://{Domain}{FileDirectory}{FileName}");

        public string FileContent { get; set; }

    }

}