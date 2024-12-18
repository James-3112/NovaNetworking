using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using UnityEngine;


namespace NovaNetworking {
    public class Server {
        public int maxClients;
        public int port;

        public Dictionary<int, Client> clients = new Dictionary<int, Client>();

        // Events
        public event Action<int> OnClientConnected;
        public event Action<int> OnClientDisconnected;
        public event Action<int, byte[]> OnDataReceived;


        private TcpListener tcpListener;
        private UdpClient udpListener;


        public void Start(int maxClients, int port) {
            this.maxClients = maxClients;
            this.port = port;

            Debug.Log("Starting server");

            for (int i = 1; i <= maxClients; i++) {
                Client client = new Client(i);
                client.OnConnected += ClientConnected;
                client.OnDisconnected += ClientDisconnected;
                client.OnDataReceived += DataReceived;

                clients.Add(i, client);
            }

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server started on port {port}");
        }


        public void Stop() {
            tcpListener.Stop();
            udpListener.Close();

            for (int i = 1; i <= maxClients; i++) {
                clients[i].Disconnect();
            }

            Debug.Log("Server has stopped");
        }


        private void TCPConnectCallback(IAsyncResult result) {
            if (!tcpListener.Server.IsBound) {
                return;
            }

            try {
                TcpClient tcpClient = tcpListener.EndAcceptTcpClient(result);
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
                
                Debug.Log($"Incoming connection from {tcpClient.Client.RemoteEndPoint}");

                for (int i = 1; i <= maxClients; i++) {
                    TCPConnection tcpConnection = clients[i].tcpConnection as TCPConnection;

                    if (tcpConnection == null) {
                        Debug.LogError("Invalid connection type for TCPConnection");
                        return;
                    }

                    if (tcpConnection.tcpClient == null) {
                        clients[i].ConnectToClient(tcpClient);
                        return;
                    }
                }
                
                Debug.LogWarning($"{tcpClient.Client.RemoteEndPoint} failed to connect: Server full");
            }
            catch (Exception ex) {
                Debug.Log($"Error receiving TCP connection: {ex}");
            }
        }


        private void UDPReceiveCallback(IAsyncResult result) {
            if (!udpListener.Client.IsBound) {
                return;
            }

            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using (Packet packet = new Packet(data)) {
                    int clientId = packet.ReadInt();
                
                    if (clientId == 0) {
                        return;
                    }

                    UDPConnection udpConnection = clients[clientId].udpConnection as UDPConnection;

                    if (udpConnection == null) {
                        Debug.LogError("Invalid connection type for UDPConnection");
                        return;
                    }

                    if (udpConnection.endPoint == null) {
                        udpConnection.ConnectToClient(clientEndPoint);
                        return;
                    }

                    if (udpConnection.endPoint.ToString() == clientEndPoint.ToString()) {
                        udpConnection.HandleData(data);
                    }
                }
            }
            catch (Exception ex) {
                Debug.Log($"Error receiving UDP connection: {ex}");
            }
        }
    

        private void ClientConnected(int clientId) {
            OnClientConnected?.Invoke(clientId);
        }

        private void ClientDisconnected(int clientId) {
            OnClientDisconnected?.Invoke(clientId);
        }

        private void DataReceived(int clientId, byte[] data) {
            OnDataReceived?.Invoke(clientId, data);
        }


        #region Send Methods
        public void SendTCPData(int toClient, Packet packet) {
            clients[toClient].SendTCPData(packet);
        }


        public void SendTCPDataToAll(Packet packet) {
            for (int i = 1; i <= maxClients; i++) {
                SendTCPData(i, packet);
            }
        }


        public void SendTCPDataToAll(int exceptClient, Packet packet) {
            for (int i = 1; i <= maxClients; i++) {
                if (i != exceptClient) SendTCPData(i, packet);
            }
        }
        

        public void SendUDPData(int toClient, Packet packet) {
            clients[toClient].SendUDPData(packet);
        }


        public void SendUDPDataToAll(Packet packet) {
            for (int i = 1; i <= maxClients; i++) {
                SendUDPData(i, packet);
            }
        }


        public void SendUDPDataToAll(int exceptClient, Packet packet) {
            for (int i = 1; i <= maxClients; i++) {
                if (i != exceptClient) SendUDPData(i, packet);
            }
        }
        #endregion
    }
}
