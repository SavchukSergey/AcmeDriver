using System;
using AcmeDriver.JWK;
using AcmeDriver.Utils;
using NUnit.Framework;

namespace AcmeDriver.Tests {
    [TestFixture]
    public class AcmeJsonTest {

        [Test]
        public void ReadAcmeChallengeDataTest() {
            var content = @"
{
    ""type"": ""1"",
    ""status"": ""abc"",
    ""url"": ""http://google.com"",
    ""token"": ""token"",
    ""error"": 
    {
        ""type"": ""error.type""
    }
}";
            var data = AcmeJson.Deserialize<AcmeChallengeData>(content);
            Assert.AreEqual("1", data.Type);
            Assert.AreEqual("abc", data.Status);
            Assert.AreEqual(new Uri("http://google.com"), data.Url);
            Assert.AreEqual("token", data.Token);
            Assert.AreEqual("error.type", data.Error?.Type);
        }

        [Test]
        public void ReadAcmeExceptionInfoTest() {
            var content = @"
{
    ""type"": ""1"",
    ""status"": 512,
    ""detail"": ""http://google.com""
}";
            var data = AcmeJson.Deserialize<AcmeExceptionInfo>(content);
            Assert.AreEqual("1", data.Type);
            Assert.AreEqual(512, data.Status);
            Assert.AreEqual("http://google.com", data.Detail);
        }

        [Test]
        public void ReadWriteEccPublicJwkTest() {
            var ecc = new EccPublicJwk {
                Curve = "test",
                X = "---x---",
                Y = "---y---"
            };
            var json = AcmeJson.Serialize((PublicJsonWebKey)ecc);
            var result = AcmeJson.Deserialize<EccPublicJwk>(json);
            Assert.AreEqual(ecc.Curve, result.Curve);
            Assert.AreEqual(ecc.X, result.X, "X");
            Assert.AreEqual(ecc.Y, result.Y, "Y");
        }

    }
}