using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#if NET8_0_OR_GREATER
using System.Threading.Channels;
#else
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endif

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Define el contrato para un servicio de mensajería que permite la comunicación desacoplada entre componentes.
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Registra un destinatario para un tipo de mensaje específico.
        /// La acción proporcionada se ejecutará cuando se envíe un mensaje del tipo <typeparamref name="TMessage"/>.
        /// El registro utiliza una referencia débil, por lo que el destinatario puede ser recolectado por el GC sin causar fugas de memoria.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje para el cual registrar al destinatario.</typeparam>
        /// <param name="recipient">El objeto destinatario que recibirá el mensaje.</param>
        /// <param name="action">La acción a ejecutar cuando se reciba el mensaje.</param>
        void Register<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// Registra un destinatario para un tipo de mensaje específico, con la opción de recibir mensajes de tipos derivados.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje para el cual registrar al destinatario.</typeparam>
        /// <param name="recipient">El objeto destinatario que recibirá el mensaje.</param>
        /// <param name="receiveDerivedMessagesToo">Si es <c>true</c>, el destinatario también recibirá mensajes de tipos que heredan de <typeparamref name="TMessage"/> o implementan <typeparamref name="TMessage"/> si es una interfaz.</param>
        /// <param name="action">La acción a ejecutar cuando se reciba el mensaje.</param>
        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action);

        /// <summary>
        /// Registra un destinatario para un tipo de mensaje específico, utilizando un token para identificar un canal de comunicación.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje para el cual registrar al destinatario.</typeparam>
        /// <param name="recipient">El objeto destinatario que recibirá el mensaje.</param>
        /// <param name="token">Un token para identificar un canal de mensajería específico. Solo los mensajes enviados con el mismo token serán recibidos.</param>
        /// <param name="action">La acción a ejecutar cuando se reciba el mensaje.</param>
        void Register<TMessage>(object recipient, object token, Action<TMessage> action);

        /// <summary>
        /// Registra un destinatario para un tipo de mensaje específico, con la opción de recibir mensajes de tipos derivados y utilizando un token.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje para el cual registrar al destinatario.</typeparam>
        /// <param name="recipient">El objeto destinatario que recibirá el mensaje.</param>
        /// <param name="token">Un token para identificar un canal de mensajería específico.</param>
        /// <param name="receiveDerivedMessagesToo">Si es <c>true</c>, el destinatario también recibirá mensajes de tipos derivados.</param>
        /// <param name="action">La acción a ejecutar cuando se reciba el mensaje.</param>
        void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action);

        /// <summary>
        /// Envía un mensaje a todos los destinatarios registrados para el tipo <typeparamref name="TMessage"/>.
        /// </summary>
        /// <typeparam name="TMessage">El tipo del mensaje a enviar.</typeparam>
        /// <param name="message">La instancia del mensaje a enviar.</param>
        void Send<TMessage>(TMessage message);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados para el tipo <typeparamref name="TMessage"/> que también son del tipo <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TMessage">El tipo del mensaje a enviar.</typeparam>
        /// <typeparam name="TTarget">El tipo de destino de los destinatarios que deben recibir el mensaje.</typeparam>
        /// <param name="message">La instancia del mensaje a enviar.</param>
        void Send<TMessage, TTarget>(TMessage message);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados para el tipo <typeparamref name="TMessage"/>, utilizando un token.
        /// </summary>
        /// <typeparam name="TMessage">El tipo del mensaje a enviar.</typeparam>
        /// <param name="message">La instancia del mensaje a enviar.</param>
        /// <param name="token">El token que identifica el canal de mensajería.</param>
        void Send<TMessage>(TMessage message, object token);

        /// <summary>
        /// Anula el registro de un destinatario para todos los tipos de mensajes.
        /// </summary>
        /// <param name="recipient">El destinatario a anular.</param>
        void Unregister(object recipient);

        /// <summary>
        /// Anula el registro de un destinatario para un tipo de mensaje específico.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje del cual anular el registro.</typeparam>
        /// <param name="recipient">El destinatario a anular.</param>
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// Anula el registro de un destinatario para un tipo de mensaje específico y un token.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje del cual anular el registro.</typeparam>
        /// <param name="recipient">El destinatario a anular.</param>
        /// <param name="token">El token del canal del cual anular el registro.</param>
        void Unregister<TMessage>(object recipient, object token);

        /// <summary>
        /// Anula el registro de una acción específica para un destinatario y un tipo de mensaje.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje.</typeparam>
        /// <param name="recipient">El destinatario.</param>
        /// <param name="action">La acción específica a anular.</param>
        void Unregister<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// Anula el registro de una acción específica para un destinatario, un tipo de mensaje y un token.
        /// </summary>
        /// <typeparam name="TMessage">El tipo de mensaje.</typeparam>
        /// <param name="recipient">El destinatario.</param>
        /// <param name="token">El token del canal.</param>
        /// <param name="action">La acción específica a anular.</param>
        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);
    }
}
