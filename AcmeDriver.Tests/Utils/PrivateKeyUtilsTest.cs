using System.Security.Cryptography;
using AcmeDriver.Utils;
using NUnit.Framework;

namespace AcmeDriver.Tests {
    [TestFixture]
    public class PrivateKeyUtilsTest {

        [Test]
        public void ToAsymmetricAlgorithmRSATest() {
            var pem = @"
-----BEGIN RSA PRIVATE KEY-----
MIICXwIBAAKBgQC9uL5prEwR0P/aw51c7qh/pX2OIImCtj4MH3dLyVO7UGUa1aWF
z0SKVMlQTUbSTyBqHzV0uve641G9YVnPf57tuM+Wwjj7rGX2ma4jkP0vCTE41/vg
iTAZAj3RF09yfL3lcqAYdlJfOPOk/cziyNmnKQgEw/nQ2bmSldx8gRuHDQIDAQAB
AoGBAJRbUwrvYMzN0kUPko6JUdn/Xx808lL0j7CLKo5o8MEZLPa++qCYY9CIXKQe
ybLLjV1u6i5NxkquV8zvw3svIOcLVONK10izIrtRycqA4a2LzMK8/vGeoE4BKvtY
ZoQfSsmmQYOmUhVvIaXh3KG58i0R/szFu98/CSKBs8OGegVBAkEA2KJ0TW0fhP20
IIY+YGjwDu5GmeJ7biEgDu1LIhWREKNPAndD9ifrTpLVOhX402snUN0ZiSScxM5g
U29VS2E5/wJBAOAyVitK192oxFQ0dkfGxDmrp8iZ12kiuxTCHT06snotFenUYTWl
o3cCy25Xi51BOgyKCS3XfxOtkP8ZIDSKhvMCQQC79zkydRyEKB3CmrhErsicK+LW
Ysz2IYfPa9VlHZFg/lHvscwmKTziUETywV3FE5RpdW5SsJHVsbuiqVc4sIPBAkEA
uKP8H492FNGnT4odV6O1e6GJf0ZkB+xYkxIDLqgm0URMZdVJcftGkv80N13WV96e
RjFwudZejABICC/TPDJSTQJBAJjiunuhNmb6H2mDw5fZ7vVM1Tz9yyhU0qLerIpI
2snUPAxmOh07FZghRzc5SYR3FLRx2fJ5pazzV/G1vWkQul0=
-----END RSA PRIVATE KEY-----
";
            var key = PrivateKeyUtils.ToAsymmetricAlgorithm(pem);
            Assert.IsInstanceOf<RSACryptoServiceProvider>(key);
            var rsa = (RSACryptoServiceProvider)key;
            Assert.AreEqual(1024, rsa.KeySize);
        }

        [Test]
        public void ToAsymmetricAlgorithmECDsaTest() {
            var pem = @"
-----BEGIN EC PRIVATE KEY-----
MHcCAQEEIHYl4h5mITzWZWVy2vhN8ZZafrGStvrv7cJ+mcPr9KjvoAoGCCqGSM49
AwEHoUQDQgAEpf95SPWY5MFGrfB5F3600BjPrJWdatLCGaiHktoP/46a2shiqKk0
SxqwKTdk1O1p7ErfwS8aj2dV29ou8kw2dg==
-----END EC PRIVATE KEY-----
";
            var key = PrivateKeyUtils.ToAsymmetricAlgorithm(pem);
            Assert.IsInstanceOf<ECDsa>(key);
            var ecdsa = (ECDsa)key;
            Assert.AreEqual(ecdsa.KeySize, 256);
        }

    }
}