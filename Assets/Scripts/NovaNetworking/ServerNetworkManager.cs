using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

using UnityEngine;


namespace NovaNetworking {
    public class ServerNetworkManager : MonoBehaviour {
        [SerializeField] static int maxClients = 20;
        [SerializeField] static int port = 7777;

        private static Server server = new Server();

        public delegate void PacketHandler(int clientId, Packet packet);
        public Dictionary<int, PacketHandler> packetHandlers = new Dictionary<int, PacketHandler>();


        private void Start() {
            GetPacketHandlers();

            server.OnDataReceived += HandleData;
            server.OnClientConnected += ClientConnected;
            server.OnClientDisconnected += ClientDisconnected;
            server.Start(maxClients, port);
        }


        private void OnApplicationQuit() {
            server.Stop();
        }


        private void GetPacketHandlers() {
            MethodInfo[] methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                .Where(t => t.GetCustomAttributes<ServerReceiveAttribute>().Any())
                .ToArray();

            foreach (MethodInfo method in methods) {
                ServerReceiveAttribute attribute = method.GetCustomAttribute<ServerReceiveAttribute>();
                Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(PacketHandler), method, false);

                packetHandlers.Add(attribute.packetId, (PacketHandler)serverMessageHandler);
            }
        }


        private void HandleData(int clientId, byte[] packetBytes) {
            using (Packet packet = new Packet(packetBytes)) {
                int packetId = packet.ReadInt();
                packetHandlers[packetId](clientId, packet);
            }
        }
        

        private void ClientConnected(int clientId) {
            SendWelcome(clientId);
        }


        private void ClientDisconnected(int clientId) {
            Debug.Log($"Client {clientId} has disconnected");
        }


        // Receiving
        [ServerReceive(ClientToServerPackets.welcomeReceived)]
        private static void WelcomeReceived(int clientId, Packet packet) {
            int clientIdCheck = packet.ReadInt();

            TCPConnection tcpConnection = server.clients[clientId].tcpConnection as TCPConnection;

            if (tcpConnection == null) {
                Debug.LogError("Invalid connection type for TCPConnection");
                return;
            }

            Debug.Log($"{tcpConnection.tcpClient.Client.RemoteEndPoint} connected successfully and is now client {clientId}");

            if (clientId != clientIdCheck) {
                Debug.LogWarning($"Client \"{tcpConnection.tcpClient.Client.RemoteEndPoint}\" (ID: {clientId}) has assumed the wrong client ID: ({clientIdCheck})");
            }
        }


        // Sending
        private static void SendWelcome(int clientId) {
            using (Packet packet = new Packet((int)ServerToClientPackets.welcome)) {
                packet.Write(clientId);
                server.SendTCPData(clientId, packet);
            }
        }
    }
}
