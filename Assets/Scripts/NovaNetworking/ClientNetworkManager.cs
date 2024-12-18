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

        public delegate void MessageHandler(Message message);
        public Dictionary<int, MessageHandler> messageHandlers = new Dictionary<int, MessageHandler>();


        private void Start() {
            GetPacketHandlers();

            client.transport.OnDataReceived += HandleData;
            client.transport.OnDisconnected += Disconnected;
            client.transport.ConnectToServer(ip, port);
        }


        private void OnApplicationQuit() {
            client.transport.Disconnect();
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
                Delegate serverMessageHandler = Delegate.CreateDelegate(typeof(MessageHandler), method, false);

                messageHandlers.Add(attribute.packetId, (MessageHandler)serverMessageHandler);
            }
        }
        

        private void Disconnected() {
            Debug.Log($"Client has disconnected");
        }


        // Receiving
        private void HandleData(byte[] packetBytes) {
            Message message = new Message(packetBytes);
            int packetId = message.ReadInt();
            messageHandlers[packetId](message);
        }


        [ClientReceive(ServerToClientPackets.welcome)]
        private static void Connected(Message packet) {
            Debug.Log($"Client has connected");
            client.id = packet.ReadInt();

            SendWelcomeReceived();
        }


        [ClientReceive(ServerToClientPackets.clientConnected)]
        private static void ClientConnected(Message packet) {
            int clientId = packet.ReadInt();
            Debug.Log($"Client ({clientId}) has connected");
        }


        [ClientReceive(ServerToClientPackets.clientDisconnected)]
        private static void ClientDisconnected(Message packet) {
            int clientId = packet.ReadInt();
            Debug.Log($"Client ({clientId}) has disconnected");
        }


        // Sending
        private static void SendWelcomeReceived() {
            Message message = new Message((int)ClientToServerPackets.welcomeReceived);
            message.Write(client.id);
            client.Send(message);
        }
    }
}
