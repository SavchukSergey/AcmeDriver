using NUnit.Framework;

namespace AcmeDriver.Tests.Challenges {
	[TestFixture, TestOf(typeof(AcmeDns01Challenge))]
	public class AcmeDns01ChallengeTests {

		[Test]
		public void CtorTest() {
			var authz = AcmeChallengeTests.Authorization;
			var challenge = authz.GetDns01Challenge();
            Assert.AreEqual("rIesIMIQh84-qSuLb2E-xyjjVACGgAP4ZtAIelEfjLE", challenge.DnsRecordContent);
		}

	}
}