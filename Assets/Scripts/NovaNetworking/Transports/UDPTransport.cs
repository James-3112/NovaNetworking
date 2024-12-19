using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NovaNetworking {
    public class UDPTransport : Transport {
        public UdpClient udpListener;
        public UdpClient udpClient;
        public IPEndPoint endPoint;


        #region Server
        public override void StartReceiving(int port) {
            udpListener = new UdpClient(port);
            udpListener.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result) {
            if (!udpListener.Client.IsBound) {
                return;
            }

            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4) return;

                Message message = new Message(data);
                int clientId = message.ReadInt();
            
                if (clientId == 0) return;

                if (endPoint == null) {
                    endPoint = clientEndPoint;
                    ClientConnected();
                    return;
                }

                if (endPoint.ToString() == clientEndPoint.ToString()) {
                    HandleData(data);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error receiving UDP connection: {ex}");
            }
        }
        #endregion


        #region Client
        public override void Connect(string ip, int port) {
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            udpClient = new UdpClient();

            udpClient.Connect(endPoint);
            udpClient.BeginReceive(DataReceiveCallback, null);

            Message message = new Message();
            Send(message);

            base.Connect();
        }


        private void DataReceiveCallback(IAsyncResult result) {
            try {
                byte[] data = udpClient.EndReceive(result, ref endPoint);
                udpClient.BeginReceive(DataReceiveCallback, null);

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
        #endregion


        private void HandleData(byte[] data) {
            Message message = new Message(data);
            int messageLength = message.ReadInt();
            data = message.ReadBytes(messageLength);

            DataReceived(data);
        }


        public override void Send(Message message) {
            try {
                message.WriteLength();

                if (udpClient != null) {
                    udpClient.BeginSend(message.ToArray(), message.Length(), null, null);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }


        public override void Disconnect() {
            base.Disconnect();

            if (udpClient != null) udpClient.Close();
            udpClient = null;
            endPoint = null;
        }
    }
}
