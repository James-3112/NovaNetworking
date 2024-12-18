using System;


namespace NovaNetworking {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ServerReceiveAttribute : Attribute {
        public int packetId;

        public ServerReceiveAttribute(ClientToServerPackets packetType) {
            packetId = (int)packetType;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ClientReceiveAttribute : Attribute {
        public int packetId;

        public ClientReceiveAttribute(ServerToClientPackets packetType) {
            packetId = (int)packetType;
        }
    }
}
