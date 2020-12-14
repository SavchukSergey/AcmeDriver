using System.Threading.Tasks;
using NUnit.Framework;

namespace AcmeDriver.Tests {
    [TestFixture]
    public class AcmeClientTest {

        [Test]
        public async Task GetDirectoryAsyncTest() {
            using var client = new AcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL);
            var directory = await client.GetDirectoryAsync();
            Assert.IsNotNull(directory.NewNonceUrl);
            Assert.IsNotNull(directory.NewAccountUrl);
            Assert.IsNotNull(directory.NewOrderUrl);
            Assert.IsNotNull(directory.NewAuthzUrl);
            Assert.IsNotNull(directory.RevokeCertUrl);
            Assert.IsNotNull(directory.KeyChangeUrl);
        }
    }
}