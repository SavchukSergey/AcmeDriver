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
            Assert.AreEqual("http://google.com", data.Uri);
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

    }
}