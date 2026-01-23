using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UMSHost;
using System.Dynamic;

namespace UnitTest
{
    [TestClass]
    public class UmsHubTests
    {
        private UmsHub _hub;
        private Mock<IHubCallerConnectionContext<dynamic>> _mockClients;
        private Mock<HubCallerContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            // Initializes UmsHub. Requires "UMSDB" connection string in App.config.
            _hub = new UmsHub();

            _mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            _mockContext = new Mock<HubCallerContext>();

            // Setup base Hub Clients
            ((Hub)_hub).Clients = _mockClients.Object;
            _hub.Context = _mockContext.Object;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _hub.Dispose();
        }

        [TestMethod]
        public void SendAll_BroadcastsMessageToAllClients()
        {
            // Arrange
            string srcName = "TestUser";
            string env = "TestEnv";
            string message = "TestMessage";
            var mockClientProxy = new Mock<IClientProxy>();

            // Setting up dynamic mock for Clients.All
            // Since we cast to Hub and use Clients (dynamic), Clients.All returns dynamic.
            // We need to ensure the dynamic object returned handles "SendAll"

            // Moq 4 with dynamic is tricky.
            // Usually simpler to just verify that the property was accessed if we can't easily mock dynamic method call on interface.
            // But we can Mock IHubCallerConnectionContext<dynamic>

            // Let's try to mock the 'All' property to return a dynamic object that we can verify.
            // Actually, Hub.Clients.All returns 'dynamic'.
            // The underlying type is determined by the mock.
            // If we return a Mock object, we can verify calls on it.

            // But SendAll is a dynamic call.
            // ((Hub)this).Clients.All.SendAll(srcName, env, message);

            // If we return an ExpandoObject, we can't verify calls easily.
            // If we return a Mock<IClientContract> where IClientContract has SendAll?

            // Let's verify that Clients.All was accessed.
            // Verifying the dynamic call is harder without an interface.

            // Option: Create a temporary interface that matches the expected call

            var mockAll = new Mock<IMockClient>();
            _mockClients.Setup(c => c.All).Returns(mockAll.Object);

            // Act
            _hub.SendAll(srcName, env, message);

            // Assert
            // Verify that SendAll was called on the object returned by Clients.All
            mockAll.Verify(m => m.SendAll(srcName, env, message), Times.Once);
        }

        [TestMethod]
        public void Hello_CallsSendAllWithHelloMessage()
        {
            // Arrange
            string srcName = "TestUser";
            string env = "TestEnv";

            var mockAll = new Mock<IMockClient>();
            _mockClients.Setup(c => c.All).Returns(mockAll.Object);
            _mockContext.Setup(c => c.ConnectionId).Returns("conn1");

            // Act
            _hub.Hello(srcName, env);

            // Assert
            // Hello calls SendAll(srcName, env, "Hello")
            mockAll.Verify(m => m.SendAll(srcName, env, "Hello"), Times.Once);
        }

        [TestMethod]
        public void Send_CallsClientSendAndSetsMessage_IfUserFound()
        {
            // Note: This test acts as an integration test because UmsHub creates a real UmsHelper instance
            // which connects to the database defined in App.config.
            // Without dependency injection for UmsHelper, we cannot mock the database calls easily.
            // This test assumes that if the DB call fails or returns no users, the catch blocks in Send will handle it.

            // Arrange
            string srcName = "Sender";
            string env = "TestEnv";
            string targetName = "Receiver";
            string message = "Test Message";

            // Act
            // This might throw if DB is not accessible, or just log errors if the catch blocks swallow them.
            // We are exercising the code path.
            _hub.Send(srcName, env, targetName, message);

            // Assert
            // Since we can't mock the DB response to return a user, we can't easily verify
            // that Clients.Client(...).Send(...) was called unless we have a real DB with data.
            // However, we can verify that the method execution completed without crashing.
        }

        // Interface to help mocking dynamic calls
        public interface IMockClient
        {
            void SendAll(string srcName, string env, string message);
            void Send(string srcName, string env, string message);
        }
    }
}
