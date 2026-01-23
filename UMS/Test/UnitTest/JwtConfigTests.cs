using Microsoft.VisualStudio.TestTools.UnitTesting;
using UMSHost;

namespace UnitTest
{
    [TestClass]
    public class JwtConfigTests
    {
        [TestMethod]
        public void JwtConfig_ReadsFromConfiguration()
        {
            // Assumes App.config has the correct values
            Assert.AreEqual("TestAudience", JwtConfig.Audience);
            Assert.AreEqual("TestIssuer", JwtConfig.Issuer);
            Assert.AreEqual("TestKey", JwtConfig.Key);
        }

        [TestMethod]
        public void ToString_ReturnsCorrectFormat()
        {
            string expected = "Audience=TestAudience; Issuer=TestIssuer; Key=TestKey;";
            Assert.AreEqual(expected, JwtConfig.ToString());
        }
    }
}
