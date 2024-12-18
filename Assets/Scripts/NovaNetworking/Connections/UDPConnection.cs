using System;
using System.Net;
using System.Net.Sockets;

using UnityEngine;


namespace NovaNetworking {
    public class UDPConnection : Connection {
        public UdpClient udpClient;
        public IPEndPoint endPoint;


        public override void ConnectToServer(string ip, int port) {
            // Invokes the OnConnected event
            base.ConnectToServer(ip, port);

            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            udpClient = new UdpClient();

            udpClient.Connect(endPoint);
            udpClient.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet()) {
                Send(packet);
            }
        }


        public override void ConnectToClient(object client) {
            // Invokes the OnConnected event
            base.ConnectToClient(client);

            endPoint = client as IPEndPoint;
            
            if (endPoint == null) {
                Debug.LogError("Invalid client type for IPEndPoint");
                return;
            }
        }


        private void ReceiveCallback(IAsyncResult result) {
            try {
                byte[] data = udpClient.EndReceive(result, ref endPoint);
                udpClient.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4) {
                    Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch {
                Disconnect();
            }
        }


        public void HandleData(byte[] data) {
            using (Packet packet = new Packet(data)) {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            DataReceived(data);
        }


        public override void Send(Packet packet) {
            try {
                if (udpClient != null) {
                    udpClient.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }


        public override void Disconnect() {
            // Invokes the OnDisconnect event
            base.Disconnect();

            if (udpClient != null) udpClient.Close();
            udpClient = null;
            endPoint = null;
        }
    }
}
