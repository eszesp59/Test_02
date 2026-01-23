using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UMSHost;

namespace UnitTest
{
    [TestClass]
    public class QueryStringOAuthBearerProviderTests
    {
        [TestMethod]
        public async Task RequestToken_TokenInQueryString_SetsToken()
        {
            // Arrange
            var provider = new QueryStringOAuthBearerProvider("access_token");
            var mockRequest = new Mock<IOwinRequest>();
            var query = new FormCollection(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "access_token", new[] { "query_token" } }
            });
            mockRequest.Setup(r => r.Query).Returns(query);
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary(new System.Collections.Generic.Dictionary<string, string[]>()));

            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var context = new OAuthRequestTokenContext(mockContext.Object, "Token");

            // Act
            await provider.RequestToken(context);

            // Assert
            Assert.AreEqual("query_token", context.Token);
        }

        [TestMethod]
        public async Task RequestToken_TokenInHeaders_SetsToken()
        {
            // Arrange
            var provider = new QueryStringOAuthBearerProvider("Authorization");
            var mockRequest = new Mock<IOwinRequest>();
            mockRequest.Setup(r => r.Query).Returns(new FormCollection(new System.Collections.Generic.Dictionary<string, string[]>()));

            var headers = new HeaderDictionary(new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "Authorization", new[] { "header_token" } }
            });
            mockRequest.Setup(r => r.Headers).Returns(headers);

            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var context = new OAuthRequestTokenContext(mockContext.Object, "Token");

            // Act
            await provider.RequestToken(context);

            // Assert
            Assert.AreEqual("header_token", context.Token);
        }

        [TestMethod]
        public async Task RequestToken_NoToken_DoesNotSetToken()
        {
            // Arrange
            var provider = new QueryStringOAuthBearerProvider("access_token");
            var mockRequest = new Mock<IOwinRequest>();
            mockRequest.Setup(r => r.Query).Returns(new FormCollection(new System.Collections.Generic.Dictionary<string, string[]>()));
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary(new System.Collections.Generic.Dictionary<string, string[]>()));

            var mockContext = new Mock<IOwinContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var context = new OAuthRequestTokenContext(mockContext.Object, "Token");

            // Act
            await provider.RequestToken(context);

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(context.Token));
        }
    }
}
