using NUnit.Framework;

namespace AcmeDriver.Tests.Challenges {
	[TestFixture, TestOf(typeof(AcmeHttp01Challenge))]
	public class AcmeHttp01ChallengeTests {

		[Test]
		public void CtorTest() {
			var authz = AcmeChallengeTests.Authorization;
			var challenge = authz.GetHttp01Challenge();
            Assert.AreEqual("fd20c6e3ca71455dbd76a88e19a0a4e3.NxiMpdzdZ7vjq_CNO4Bkz_Y9BAfugq5F33FbFwHn-fM", challenge.FileContent);
		}

	}
}