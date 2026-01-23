using System;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UMSHost;

namespace UnitTest
{
    [TestClass]
    public class JWTAuthorizeAttributeTests
    {
        private class TestableJWTAuthorizeAttribute : JWTAuthorizeAttribute
        {
            public bool PublicUserAuthorized(IPrincipal user)
            {
                return base.UserAuthorized(user);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UserAuthorized_NullUser_ThrowsArgumentNullException()
        {
            var attribute = new TestableJWTAuthorizeAttribute();
            attribute.PublicUserAuthorized(null);
        }

        [TestMethod]
        public void UserAuthorized_AuthenticatedUser_ReturnsTrue()
        {
            var attribute = new TestableJWTAuthorizeAttribute();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(true);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);

            bool result = attribute.PublicUserAuthorized(mockUser.Object);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UserAuthorized_UnauthenticatedUser_ReturnsFalse()
        {
            var attribute = new TestableJWTAuthorizeAttribute();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);
            var mockUser = new Mock<IPrincipal>();
            mockUser.Setup(u => u.Identity).Returns(mockIdentity.Object);

            bool result = attribute.PublicUserAuthorized(mockUser.Object);

            Assert.IsFalse(result);
        }
    }
}
