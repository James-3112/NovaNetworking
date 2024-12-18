using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

using UnityEngine;


namespace NovaNetworking {
    public class ClientNetworkManager : MonoBehaviour {
        [SerializeField] static string ip = "127.0.0.1";
        [SerializeField] static int port = 7777;

        private static Client client = new Client();

        public delegate void PacketHandler(Packet packet);
        public Dictionary<int, PacketHandler> packetHandlers = new Dictionary<int, PacketHandler>();


        private void Start() {
            GetPacketHandlers();

            client.OnDataReceived += HandleData;
            client.OnDisconnected += Disconnected;
            client.ConnectToServer(ip, port);
        }


        private void OnApplicationQuit() {
            client.Disconnect();
        }


        private void GetPacketHandlers() {
            MethodInfo[] methods = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                .Where(t => t.GetCustomAttributes<ClientReceiveAttribute>().Any())
                .ToArray();

            foreach (MethodInfo method in methods) {
                ClientReceiveAttribute attribute = method.GetCustomAttribute<ClientReceiveAttribute>();
                Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(PacketHandler), method, false);

                packetHandlers.Add(attribute.packetId, (PacketHandler)serverMessageHandler);
            }
        }
        

        private void Disconnected(int clientId) {
            Debug.Log($"Client has disconnected");
        }


        // Receiving
        private void HandleData(int clientId, byte[] packetBytes) {
            using (Packet packet = new Packet(packetBytes)) {
                int packetId = packet.ReadInt();
                packetHandlers[packetId](packet);
            }
        }


        [ClientReceive(ServerToClientPackets.welcome)]
        private static void Connected(Packet packet) {
            Debug.Log($"Client has connected");
            client.id = packet.ReadInt();

            // Send welcome received packet to the server and connect udp
            SendWelcomeReceived();
            client.udpConnection.ConnectToServer(ip, port);
        }


        // Sending
        private static void SendWelcomeReceived() {
            using (Packet packet = new Packet((int)ClientToServerPackets.welcomeReceived)) {
                packet.Write(client.id);
                client.SendTCPData(packet);
            }
        }
    }
}
