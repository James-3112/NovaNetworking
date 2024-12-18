using System;


namespace NovaNetworking {
    public class Connection {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<byte[]> OnDataReceived;

        public virtual void ConnectToServer(string ip, int port) {
            OnConnected?.Invoke();
        }

        public virtual void ConnectToClient(object client) {
            OnConnected?.Invoke();
        }

        public virtual void Disconnect() {
            OnDisconnected?.Invoke();
        }

        public virtual void Send(Packet packet) {}
        
        public void DataReceived(byte[] data) {
            OnDataReceived?.Invoke(data);
        }
    }
}
