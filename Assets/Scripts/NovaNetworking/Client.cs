using System;
using UnityEngine;


namespace NovaNetworking {
    public class Client {
        public string ip = "127.0.0.1";
        public int port = 7777;
        public int id = 0;

        // Events
        public event Action<int> OnConnected;
        public event Action<int> OnDisconnected;
        public event Action<int, byte[]> OnDataReceived;

        // Connections
        private static int dataBufferSize = 4096;

        public Connection tcpConnection = new TCPConnection(dataBufferSize);
        public Connection udpConnection = new UDPConnection();


        public Client() {
            AddEvents();
        }
        
        public Client(int id) {
            AddEvents();
            this.id = id;
        }


        private void AddEvents() {
            tcpConnection.OnDisconnected += TCPDisconnect;
            tcpConnection.OnDataReceived += DataReceived;

            udpConnection.OnDisconnected += UDPDisconnect;
            udpConnection.OnDataReceived += DataReceived;
        }


        public void ConnectToServer(string ip, int port) {
            this.ip = ip;
            this.port = port;

            tcpConnection.ConnectToServer(ip, port);
            OnConnected?.Invoke(id);
        }


        public void ConnectToClient(object client) {
            tcpConnection.ConnectToClient(client);
            OnConnected?.Invoke(id);
        }


        public void Disconnect() {
            tcpConnection.Disconnect();
            udpConnection.Disconnect();

            OnDisconnected?.Invoke(id);
        }

        private void TCPDisconnect() {
            
        }

        private void UDPDisconnect() {
            
        }



        public void SendTCPData(Packet packet) {
            packet.WriteLength();
            tcpConnection.Send(packet);
        }


        public void SendUDPData(Packet packet) {
            packet.WriteLength();
            packet.InsertInt(id);
            udpConnection.Send(packet);
        }


        private void DataReceived(byte[] data) {
            OnDataReceived?.Invoke(id, data);
        }
    }
}
