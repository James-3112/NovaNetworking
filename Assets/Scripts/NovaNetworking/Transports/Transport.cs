using System;

namespace NovaNetworking {
    public class Transport {
        public event Action OnConnected;
        public event Action OnClientConnected;
        public event Action OnDisconnected;
        public event Action<byte[]> OnDataReceived;

        public virtual void StartReceiving(int port) {}
        public virtual void Connect(string ip, int port) {}
        public virtual void Send(Message packet) {}
        
        public void DataReceived(byte[] data) {
            OnDataReceived?.Invoke(data);
        }

        public virtual void Connect() {
            OnConnected?.Invoke();
        }

        public void ClientConnected() {
            OnClientConnected?.Invoke();
        }

        public virtual void Disconnect() {
            OnDisconnected?.Invoke();
        }
    }
}
