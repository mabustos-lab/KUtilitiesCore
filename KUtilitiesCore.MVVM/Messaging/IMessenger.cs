using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    public interface IMessenger
    {
        /// <summary>
        /// Registra un destinatario para un tipo de mensaje <typeparamref name="TTypeMessage"/>. El parámetro de acción
        /// se ejecutará cuando se envíe un mensaje correspondiente. Consulte el parámetro receiveDerivedMessagesToo
        /// para obtener detalles sobre cómo se pueden recibir también los mensajes derivados de <typeparamref
        /// name="TTypeMessage"/> (o, si <typeparamref name="TTypeMessage"/> es una interfaz, mensajes que implementan
        /// <typeparamref name="TTypeMessage"/>). <para> El registro de un destinatario no crea una referencia dura a
        /// él, por lo que si se elimina este destinatario, no se produce ninguna pérdida de memoria.</para>
        /// </summary>
        /// <typeparam name="TTypeMessage">Tipo de mensaje para el que se registra el destinatario.</typeparam>
        /// <param name="recipient">El destinatario que recibirá los mensajes.</param>
        /// <param name="token">
        /// Un token para un canal de mensajería. Si un destinatario se registra con un token y un remitente envía un
        /// mensaje con el mismo token, este mensaje se entregará al destinatario. Otros destinatarios que no usaron un
        /// token al registrarse (o que usaron un token diferente) no recibirán el mensaje. Del mismo modo, los mensajes
        /// enviados sin ningún token, o con un token diferente, no se entregarán a ese destinatario.
        /// </param>
        /// <param name="receiveDerivedMessagesToo">
        /// Si es true, los tipos de mensajes derivados de <typeparamref name="TTypeMessage"/> también se transmitirán
        /// al destinatario. Por ejemplo, si SendOrderMessage y ExecuteOrderMessage derivan de OrderMessage, registrarse
        /// en OrderMessage y establecer receiveDerivedMessagesToo en true enviará SendOrderMessage y
        /// ExecuteOrderMessage al destinatario que se registró. <para> Además, si <typeparamref name="TTypeMessage"/>
        /// es una interfaz, los tipos de mensajes que implementan <typeparamref name="TTypeMessage"/> también se
        /// transmitirán al destinatario. Por ejemplo, si SendOrderMessage y ExecuteOrderMessage implementan
        /// IOrderMessage, registrarse en IOrderMessage y establecer receiveDerivedMessagesToo en true enviará
        /// SendOrderMessage y ExecuteOrderMessage al destinatario que se registró.</para>
        /// </param>
        /// <param name="action">
        /// La acción que se ejecutará cuando se envíe un mensaje de tipo TMessage.
        /// </param>
        void Register<TTypeMessage>(object recipient, Action<TTypeMessage> action, bool receiveDerivedMessagesToo = false, object token = null);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje llegará a todos los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <param name="message">El mensaje que se enviará a los destinatarios registrados.</param>
        void Send<TTypeMessage>(TTypeMessage message);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje solo llegará a los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register y que son de targetType.
        /// </summary>
        /// <typeparam name="TTypeMessage">The type of message that will be sent.</typeparam>
        /// <typeparam name="TTarget">
        /// El tipo de destinatarios que recibirán el mensaje. El mensaje no se enviará a destinatarios de otro tipo.
        /// </typeparam>
        /// <param name="message">El mensaje que se enviará a los destinatarios registrados.</param>
        void Send<TTypeMessage, TTarget>(TTypeMessage message);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje solo llegará a los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register y que son de targetType.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <param name="message">El mensaje que se debe enviar a los destinatarios registrados.</param>
        /// <param name="token">
        /// Un token para un canal de mensajería. Si un destinatario se registra con un token y un remitente envía un
        /// mensaje con el mismo token, este mensaje se entregará al destinatario. Otros destinatarios que no usaron un
        /// token al registrarse (o que usaron un token diferente) no recibirán el mensaje. Del mismo modo, los mensajes
        /// enviados sin ningún token, o con un token diferente, no se entregarán a ese destinatario.
        /// </param>
        void Send<TTypeMessage>(TTypeMessage message, object token);

        /// <summary>
        /// Cancela completamente el registro de un destinatario del mensajer. Después de ejecutar este método, el
        /// destinatario ya no recibirá ningún mensaje.
        /// </summary>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        void Unregister(object recipient);

        /// <summary>
        /// Anular el registro de un destinatario de mensaje solo para un tipo determinado de mensajes. Después de
        /// ejecutar este método, el destinatario ya no recibirá mensajes de tipo <typeparamref name="TTypeMessage"/>,
        /// pero seguirá recibiendo otros tipos de mensajes (si se registró para ellos anteriormente).
        /// </summary>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        /// <typeparam name="TTypeMessage">
        /// El tipo de mensajes de los que el destinatario desea darse de baja.
        /// </typeparam>
        void Unregister<TTypeMessage>(object recipient);

        /// <summary>
        /// Unregisters a message recipient for a given type of messages and for a given action. Other message types
        /// will still be transmitted to the recipient (if it registered for them previously). Other actions that have
        /// been registered for the message type TMessage and for the given recipient (if available) will also remain
        /// available.
        /// </summary>
        /// <typeparam name="TTypeMessage">The type of messages that the recipient wants to unregister from.</typeparam>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="action">The action that must be unregistered for the recipient and for the message type TMessage.</param>
        void Unregister<TTypeMessage>(object recipient, Action<TTypeMessage> action);

        /// <summary>
        /// Unregisters a message recipient for a given type of messages only and for a given token.  After this method
        /// is executed, the recipient will not receive messages of type TMessage anymore with the given token, but will
        /// still receive other message types or messages with other tokens (if it registered for them previously).
        /// </summary>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="token">The token for which the recipient must be unregistered.</param>
        /// <typeparam name="TTypeMessage">The type of messages that the recipient wants to unregister from.</typeparam>
        void Unregister<TTypeMessage>(object recipient, object token);

        /// <summary>
        /// Unregisters a message recipient for a given type of messages, for a given action and a given token. Other
        /// message types will still be transmitted to the recipient (if it registered for them previously). Other
        /// actions that have been registered for the message type TMessage, for the given recipient and other tokens
        /// (if available) will also remain available.
        /// </summary>
        /// <typeparam name="TMessage">
        /// The type of messages that the recipient wants to unregister from.
        /// </typeparam>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="token">The token for which the recipient must be unregistered.</param>
        /// <param name="action">The action that must be unregistered for the recipient and for the message type TMessage.</param>
        void Unregister<TTypeMessage>(object recipient, object token, Action<TTypeMessage> action);
    }
}