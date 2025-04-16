using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// El Messenger es una clase que permite a los objetos intercambiar mensajes.
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Registra un destinatario para un tipo de mensaje <typeparamref name="TTypeMessage"/>. El parámetro de acción
        /// se ejecutará cuando corresponda se envía el mensaje.see el parámetro recibido Para detalles sobre cómo los
        /// mensajes derivados de <typeparamref name="TTypeMessage"/> (o, si TMessage es una interfaz, Mensajes que
        /// implementan <typeparamref name="TTypeMessage"/>) también se pueden recibir. <para> Registro de un
        /// destinatario no crea una referencia difícil a ella, entonces, si se elimina este destinatario, no se causa
        /// una pérdida de memoria.</para>
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje para que el destinatario se registre.</typeparam>
        /// <param name="recipient">El destinatario que recibirá los mensajes.</param>
        /// <param name="receiveDerivedMessagesToo">
        /// Si es cierto, los tipos de mensajes derivan de <typeparamref name="TTypeMessage"/> también será transmitido
        /// al destinatario. Por ejemplo, si un SendOrderMessage y un ExecuteOrderMessage se derivan de OrderMessage, se
        /// registra para el OrderMessage y la configuración de receiveDerivedMessagesToo al verdadero enviará
        /// SendOrderMessage y ExecuteOrderMessage al destinatario que se registró. <para> Además, si <typeparamref
        /// name="TTypeMessage"/> es una interfaz, los tipos de mensajes que implementan <typeparamref
        /// name="TTypeMessage"/> también se transmitirán al destinatario. Por ejemplo, si se envía un SendOrderMessage
        /// y un ExecuteOrderMessage implementando IOrderMessage, el registro de IOrderMessage y la configuración de
        /// receiveDerivedMessagesToo al verdadero enviarán SendOrderMessage y ExecuteOrderMessage al destinatario que
        /// se registró.</para>
        /// </param>
        /// <param name="action">
        /// La acción que se ejecutará cuando un mensaje de tipo. <typeparamref name="TTypeMessage"/> es enviado.
        /// </param>
        public void Register<TMessage>(
            object recipient,
            Action<TMessage> action,
            bool receiveDerivedMessages = false,
            object token = null);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje llegará a todos los
        /// destinatarios que se registraron para este tipo de mensaje usando uno de los métodos de registro.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <param name="message">El mensaje para enviar a destinatarios registrados.</param>
        void Send<TTypeMessage>(TTypeMessage message);

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje solo llegará a los
        /// destinatarios que se registraron para este tipo de mensaje usando uno de los métodos de
        /// registro, y que son del tipo de destino.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <typeparam name="TTarget">
        /// El tipo de destinatarios que recibirán el mensaje. El mensaje no se enviará a los
        /// destinatarios de otro tipo.
        /// </typeparam>
        /// <param name="message">El mensaje para enviar a destinatarios registrados.</param>
        void Send<TTypeMessage, TTarget>(TTypeMessage message);

        /// <summary>
        /// Anular el registro de un mensajero completamente. Después de ejecutar este método, el
        /// destinatario ya no recibirá ningún mensaje.
        /// </summary>
        /// <param name="recipient">El destinatario el cual se anulara el registro.</param>
        void Unregister(object recipient);

        /// <summary>
        /// Anular el registro de un destinatario de mensaje solo para un tipo determinado de
        /// mensajes. Después de ejecutar este método, el destinatario ya no recibirá mensajes de
        /// tipo <typeparamref name="TTypeMessage"/>, pero seguirá recibiendo otros tipos de
        /// mensajes (si se registró para ellos anteriormente).
        /// </summary>
        /// <typeparam name="TTypeMessage">
        /// El tipo de mensajes de los que el destinatario desea darse de baja.
        /// </typeparam>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        void Unregister<TTypeMessage>(object recipient);

        /// <summary>
        /// Cancela el registro de un destinatario de mensaje para un tipo determinado de mensajes y
        /// para una acción determinada. Otros tipos de mensajes seguirán transmitiendose al
        /// destinatario (si se registró para ellos anteriormente). Otras acciones que se hayan
        /// registrado para el tipo de mensaje <typeparamref name="TTypeMessage"/> y para el
        /// destinatario dado (si está disponible) también permanecerán disponibles.
        /// </summary>
        /// <typeparam name="TTypeMessage">
        /// El tipo de mensajes de los que el destinatario desea darse de baja.
        /// </typeparam>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        /// <param name="action">
        /// La acción que debe no registrarse para el destinatario y para el tipo de mensaje
        /// <typeparamref name="TTypeMessage"/>.
        /// </param>
        void Unregister<TTypeMessage>(object recipient, Action<TTypeMessage> action);
    }
}
