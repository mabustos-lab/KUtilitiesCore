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
        
        [TestCleanup]
        public void CleanupMessenger()
        {
            // Restablece la instancia Default para asegurar que cada prueba
            // obtenga una nueva instancia si es necesario, o para limpiar su estado.
            // Esto es importante si Messenger.Default captura un SynchronizationContext
            // que podría variar o ser problemático entre pruebas.
            Messenger.Reset();
        }

        [TestMethod()]
        public void RegisterAndSend_ShouldDeliverMessageToRecipient()
        {
            // Arrange
            // Al llamar a Messenger.Reset() en Cleanup, la primera vez que se acceda a Default
            // en una prueba, se creará una nueva instancia de Messenger.
            var messenger = Messenger.Default;
            var recipient = new TestRecipient();
            const string testMessage = "TestMessage";

            messenger.Register<string>(recipient, recipient.HandleMessage);

            // Act
            messenger.Send(testMessage);

            // Espera a que el mensaje sea procesado por el destinatario
            // Ajusta el tiempo de espera según sea necesario, pero debería ser corto.
            bool messageWasReceived = recipient.WaitForMessage(TimeSpan.FromSeconds(2)); // Por ejemplo, 2 segundos

            // Assert
            Assert.IsTrue(messageWasReceived, "El mensaje no fue recibido por el destinatario dentro del tiempo de espera.");
            Assert.AreEqual(testMessage, recipient.ReceivedMessage, "El mensaje recibido no es el esperado.");
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
            recipient.WaitForMessage(TimeSpan.FromSeconds(2)); // Por ejemplo, 2 segundos

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
            messenger.Register<string>(recipient1, "Token1", recipient1.HandleMessage);
            messenger.Register<string>(recipient2, "Token2", recipient2.HandleMessage);

            // Act
            messenger.Send("MessageForToken1", "Token1");
            bool messageWasReceived = recipient1.WaitForMessage(TimeSpan.FromSeconds(2)); // Por ejemplo, 2 segundos

            // Assert
            Assert.IsTrue(messageWasReceived, "El mensaje no fue recibido por el destinatario dentro del tiempo de espera.");
            Assert.IsTrue("MessageForToken1".Equals(recipient1.ReceivedMessage));
            Assert.IsTrue(string.IsNullOrEmpty(recipient2.ReceivedMessage));
        }

        private class TestRecipient
        {
            public string? ReceivedMessage { get; private set; }
            private readonly ManualResetEventSlim _messageReceivedEvent = new ManualResetEventSlim(false);
            public void HandleMessage(string message)
            {
                ReceivedMessage = message;
                _messageReceivedEvent.Set(); // Señala que el mensaje fue recibido y procesado
            }
            // Método para que la prueba espere
            public bool WaitForMessage(TimeSpan timeout)
            {
                return _messageReceivedEvent.Wait(timeout);
            }
            // Opcional: Método para resetear para múltiples envíos en una prueba o entre pruebas
            public void ResetForNextMessage()
            {
                ReceivedMessage = null;
                _messageReceivedEvent.Reset();
            }
        }
    }
}