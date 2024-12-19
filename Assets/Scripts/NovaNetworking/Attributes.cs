using System;


namespace NovaNetworking {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ServerReceiveAttribute : Attribute {
        public int messageId;

        public ServerReceiveAttribute(ClientToServerMessages messageType) {
            messageId = (int)messageType;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ClientReceiveAttribute : Attribute {
        public int messageId;

        public ClientReceiveAttribute(ServerToClientMessages messageType) {
            messageId = (int)messageType;
        }
    }
}
