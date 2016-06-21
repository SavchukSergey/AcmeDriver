using System;
using System.Threading.Tasks;

namespace AcmeDriver {
    class Program {
        static void Main(string[] args) {
            Task.Run(MainAsync).Wait();
        }

        private static async Task MainAsync() {
            try {
                var client = new AcmeClient("https://acme-staging.api.letsencrypt.org");
                var dir = await client.GetDirectoryAsync();
             
            } catch (Exception exc) {
                Console.WriteLine(exc.Message);
            }

            Console.ReadKey();
        }
    }
}
