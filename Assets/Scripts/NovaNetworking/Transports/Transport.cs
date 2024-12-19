using System;

namespace NovaNetworking {
    public class Transport {
        public event Action OnConnected;
        public event Action OnClientConnected;
        public event Action OnDisconnected;
        public event Action<byte[]> OnDataReceived;


        // Server
        public virtual void StartReceiving(int port) {}
        public virtual void StopReceiving() {}

        public void ClientConnected() {
            OnClientConnected?.Invoke();
        }


        // Client
        public virtual void Connect(string ip, int port) {
            OnConnected?.Invoke();
        }

        public virtual void Send(Message packet) {}
        

        // General
        public void DataReceived(byte[] data) {
            OnDataReceived?.Invoke(data);
        }

        public virtual void Disconnect() {
            OnDisconnected?.Invoke();
        }
    }
}
