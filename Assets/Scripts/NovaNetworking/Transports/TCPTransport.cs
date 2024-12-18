using System;
using System.Net.Sockets;
using UnityEngine;

namespace NovaNetworking {
    public class TCPTransport : Transport {
        public TcpClient tcpClient;
        private NetworkStream stream;

        private int dataBufferSize = 4096;
        private Message receivedData;
        private byte[] receiveBuffer;


        public override void ConnectToServer(string ip, int port) {
            tcpClient = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            receivedData = new Message();

            tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient);
        }


        private void ConnectCallback(IAsyncResult result) {
            tcpClient.EndConnect(result);
            if (!tcpClient.Connected) return;

            stream = tcpClient.GetStream();
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }


        public override void ConnectToClient(object client) {
            tcpClient = client as TcpClient;
            
            if (tcpClient == null) {
                Debug.LogError("Invalid client type for TcpClient");
                return;
            }

            tcpClient.ReceiveBufferSize = dataBufferSize;
            tcpClient.SendBufferSize = dataBufferSize;

            receivedData = new Message();
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
            int messageLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4) {
                messageLength = receivedData.ReadInt();
                if (messageLength <= 0) return true;
            }

            while (messageLength > 0 && messageLength <= receivedData.UnreadLength()) {
                DataReceived(receivedData.ReadBytes(messageLength));

                messageLength = 0;
                if (receivedData.UnreadLength() >= 4) {
                    messageLength = receivedData.ReadInt();

                    if (messageLength <= 0) return true;
                }
            }

            if (messageLength <= 1) return true;
            return false;
        }


        public override void Send(Message message) {
            try {
                if (tcpClient != null) {
                    stream.BeginWrite(message.ToArray(), 0, message.Length(), null, null);
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error sending data to server via TCP: {ex}");
            }
        }


        public override void Disconnect() {
            base.Disconnect();

            if (tcpClient != null) tcpClient.Close();
            tcpClient = null;
            stream = null;

            receivedData = null;
            receiveBuffer = null;
        }
    }
}
