using System;

namespace NovaNetworking {
    public class Transport {
        public event Action<byte[]> OnDataReceived;
        public event Action OnDisconnected;

        public virtual void ConnectToServer(string ip, int port) {}
        public virtual void ConnectToClient(object socket) {}
        public virtual void Send(Message packet) {}
        
        public void DataReceived(byte[] data) {
            OnDataReceived?.Invoke(data);
        }

        public virtual void Disconnect() {
            OnDisconnected?.Invoke();
        }
    }
}
