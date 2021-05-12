using System.Threading.Tasks;
using AcmeDriver.JWK;
using NUnit.Framework;

namespace AcmeDriver.Tests {
	[TestFixture]
	public class AcmeClientTest {

		[Test]
		public async Task GetDirectoryAsyncTest() {
			using var client = await AcmeClient.CreateAcmeClientAsync(AcmeClient.LETS_ENCRYPT_STAGING_URL);
			var directory = await client.GetDirectoryAsync();
			Assert.IsNotNull(directory.NewNonceUrl);
			Assert.IsNotNull(directory.NewAccountUrl);
			Assert.IsNotNull(directory.NewOrderUrl);
			Assert.IsNotNull(directory.RevokeCertUrl);
			Assert.IsNotNull(directory.KeyChangeUrl);
		}

		[Test]
		public async Task NewRegistrationAsyncTest() {
			using var client = new AcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL);
			var key = EccPrivateJwk.Create();
			var registration = await client.RegisterAsync(new[] { "mailto:noreply@test.com" }, key);
			Assert.IsNotNull(registration);
		}

		[Test]
		public async Task GetRegistrationAsyncTest() {
			using var client = new AcmeClient(AcmeClient.LETS_ENCRYPT_STAGING_URL);
			var key = EccPrivateJwk.Create();
			var registration1 = await client.RegisterAsync(new[] { "mailto:noreply@test.com" }, key);
			Assert.IsNotNull(registration1);

			var registration2 = await (await client.AuthenticateAsync(key)).Registrations.GetRegistrationAsync();
			Assert.AreEqual(registration1.Registration.Location, registration2.Location);
		}

	}
}