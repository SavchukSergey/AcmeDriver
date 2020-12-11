using System;
using System.Collections.Generic;

namespace AcmeDriver.CLI {
    public class CommandLineOptions {

        public string Action { get; set; }

        public IList<string> Contacts { get; } = new List<string>();

        public IList<string> Domains { get; } = new List<string>();

        public string AccountFile { get; set; }

        public string OrderFile { get; set; }

		public string CsrFile { get; set; }
		
		public string ChallengePath { get; set; }

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
                        case "--challenge":
                            enumerator.MoveNext();
                            res.ChallengePath = enumerator.Current;
                            break;
						case "--contact":
							enumerator.MoveNext();
							res.Contacts.Add(enumerator.Current);
							break;
						case "--csr":
							enumerator.MoveNext();
							res.CsrFile = enumerator.Current;
							break;
						case "--domain":
                            enumerator.MoveNext();
                            res.Domains.Add(enumerator.Current);
                            break;
                        case "--order":
                            enumerator.MoveNext();
                            res.OrderFile = enumerator.Current;
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
