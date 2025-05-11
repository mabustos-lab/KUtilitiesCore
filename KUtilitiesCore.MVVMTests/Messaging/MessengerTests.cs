using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.MVVM.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Messaging.Tests
{
    [TestClass()]
    public class MessengerTests
    {     

         [TestMethod()]
        public void RegisterAndSend_ShouldDeliverMessageToRecipient()
        {
            // Arrange
            var messenger = Messenger.Default;
            var recipient = new TestRecipient();
            messenger.Register<string>(recipient, recipient.HandleMessage);

            // Act
            messenger.Send("TestMessage");

            // Assert
            Assert.IsTrue("TestMessage".Equals(recipient.ReceivedMessage));
        }

         [TestMethod()]
        public void Unregister_ShouldStopDeliveringMessages()
        {
            // Arrange
            var messenger = Messenger.Default;
            var recipient = new TestRecipient();
            messenger.Register<string>(recipient, recipient.HandleMessage);

            // Act
            messenger.Unregister(recipient);
            messenger.Send("TestMessage");

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(recipient.ReceivedMessage));
        }

         [TestMethod()]
        public void Send_WithToken_ShouldDeliverMessageToCorrectRecipient()
        {
            // Arrange
            var messenger = Messenger.Default;
            var recipient1 = new TestRecipient();
            var recipient2 = new TestRecipient();
            messenger.Register<string>(recipient1, recipient1.HandleMessage, token: "Token1");
            messenger.Register<string>(recipient2, recipient2.HandleMessage, token: "Token2");

            // Act
            messenger.Send("MessageForToken1", "Token1");

            // Assert
            Assert.IsTrue("MessageForToken1".Equals(recipient1.ReceivedMessage));
            Assert.IsTrue(string.IsNullOrEmpty(recipient2.ReceivedMessage));
        }

        private class TestRecipient
        {
            public string? ReceivedMessage { get; private set; }

            public void HandleMessage(string message)
            {
                ReceivedMessage = message;
            }
        }
    }
}