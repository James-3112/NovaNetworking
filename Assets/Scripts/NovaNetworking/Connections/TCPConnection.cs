using System;
using System.Net.Sockets;

using UnityEngine;


namespace NovaNetworking {
    public class TCPConnection : Connection {
        public TcpClient tcpClient;
        private NetworkStream stream;

        private int dataBufferSize;
        private Packet receivedData;
        private byte[] receiveBuffer;


        public TCPConnection(int dataBufferSize) {
            this.dataBufferSize = dataBufferSize;
        }


        public override void ConnectToServer(string ip, int port) {
            // Invokes the OnConnected event
            base.ConnectToServer(ip, port);

            tcpClient = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            receivedData = new Packet();

            tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient);
        }


        private void ConnectCallback(IAsyncResult result) {
            tcpClient.EndConnect(result);
            if (!tcpClient.Connected) return;

            stream = tcpClient.GetStream();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }


        public override void ConnectToClient(object client) {
            // Invokes the OnConnected event
            base.ConnectToClient(client);

            tcpClient = client as TcpClient;
            
            if (tcpClient == null) {
                Debug.LogError("Invalid client type for TcpClient");
                return;
            }

            tcpClient.ReceiveBufferSize = dataBufferSize;
            tcpClient.SendBufferSize = dataBufferSize;

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream = tcpClient.GetStream();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }


        private void ReceiveCallback(IAsyncResult result) {
            if (stream == null) return;

            try {
                int byteLength = stream.EndRead(result);

                if (byteLength <= 0) {
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex) {
                Debug.Log($"Error receiving TCP data: {ex}");
                Disconnect();
            }
        }


        private bool HandleData(byte[] data) {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4) {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                DataReceived(receivedData.ReadBytes(packetLength));

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0) return true;
                }
            }

            if (packetLength <= 1) return true;
            return false;
        }


        public override void Send(Packet packet) {
            try {
                if (tcpClient != null) {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error sending data to server via TCP: {ex}");
            }
        }


        public override void Disconnect() {
            // Invokes the OnDisconnect event
            base.Disconnect();

            if (tcpClient != null) tcpClient.Close();
            tcpClient = null;
            stream = null;

            receivedData = null;
            receiveBuffer = null;
        }
    }
}
