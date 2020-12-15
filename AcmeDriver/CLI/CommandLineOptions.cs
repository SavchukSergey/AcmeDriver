using System;
using System.Collections.Generic;

namespace AcmeDriver.CLI {
    public record CommandLineOptions {

        public string Action { get; set; }

        public string Api { get; set; }

        public Uri ApiUrl {
            get {
                return Api switch {
                    "lets-encrypt" => AcmeClient.LETS_ENCRYPT_PRODUCTION_URL,
                    "lets-encrypt:staging" => AcmeClient.LETS_ENCRYPT_STAGING_URL,
                    _ => new Uri(Api)
                };
            }
        }

        public IList<string> Contacts { get; } = new List<string>();

        public IList<string> Domains { get; } = new List<string>();

        public string? AccountFile { get; set; }

        public string? PrivateKeyFile { get; set; }

        public string? KeyAlgorithm { get; set; }
        
        public string? Subject { get; set; }
        
        public string? OrderFile { get; set; }

		public string? CsrFile { get; set; }
		
        public string? CrtFile { get; set; }

		public string? ChallengePath { get; set; }

		public static CommandLineOptions Parse(IEnumerable<string> args) {
            var enumerator = args.GetEnumerator();
            var res = new CommandLineOptions();
            while (enumerator.MoveNext()) {
                var current = enumerator.Current;
                if (current.StartsWith("--")) {
                    switch (current) {
                        case "--account":
                            enumerator.MoveNext();
                            res.AccountFile = enumerator.Current;
                            break;
                        case "--api":
                            enumerator.MoveNext();
                            res.Api = enumerator.Current;
                            break;
                        case "--challenge":
                            enumerator.MoveNext();
                            res.ChallengePath = enumerator.Current;
                            break;
                        case "--contact":
                            enumerator.MoveNext();
                            res.Contacts.Add(enumerator.Current);
                            break;
                        case "--crt":
                            enumerator.MoveNext();
                            res.CrtFile = enumerator.Current;
                            break;
                        case "--csr":
                            enumerator.MoveNext();
                            res.CsrFile = enumerator.Current;
                            break;
                        case "--domain":
                            enumerator.MoveNext();
                            res.Domains.Add(enumerator.Current);
                            break;
                        case "--key-algorithm":
                            enumerator.MoveNext();
                            res.KeyAlgorithm = enumerator.Current;
                            break;
                        case "--order":
                            enumerator.MoveNext();
                            res.OrderFile = enumerator.Current;
                            break;
                        case "--private-key":
                            enumerator.MoveNext();
                            res.PrivateKeyFile = enumerator.Current;
                            break;
                        case "--subject":
                            enumerator.MoveNext();
                            res.Subject = enumerator.Current;
                            break;
                        default:
                            Console.Error.WriteLineAsync($"Invalid option {current}");
                            break;
                    }
                } else {
                    res.Action = current;
                }
            }
            return res;
        }

    }
}
