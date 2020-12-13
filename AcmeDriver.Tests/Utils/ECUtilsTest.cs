using System.Security.Cryptography;
using AcmeDriver.Utils;
using NUnit.Framework;

namespace AcmeDriver.Tests {
    [TestFixture]
    public class ECUtilsTest {

        [Test]
        public void GetCurveTest() {
            var nistP256 = ECUtils.GetCurve("P-256");
            Assert.AreEqual(ECCurve.NamedCurves.nistP256.Oid.ToString(), nistP256.Oid.ToString());
        }
    }
}