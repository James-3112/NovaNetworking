using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

using UnityEngine;


namespace NovaNetworking {
    public class ClientNetworkManager : MonoBehaviour {
        [SerializeField] string ip = "127.0.0.1";
        [SerializeField] int port = 7777;

        private static Client client;

        public delegate void MessageHandler(Message message);
        public Dictionary<int, MessageHandler> messageHandlers = new Dictionary<int, MessageHandler>();


        private void Start() {
            GetPacketHandlers();
    
            client = new Client();
            client.transport.OnConnected += OnConnected;
            client.transport.OnDisconnected += OnDisconnected;
            client.transport.OnDataReceived += OnDataReceived;
            client.transport.Connect(ip, port);
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

                messageHandlers.Add(attribute.messageId, (MessageHandler)serverMessageHandler);
            }
        }


        public static void Send(Message message) {
            client.transport.Send(message);
        }


        #region Events
        private void OnConnected() {
            Debug.Log($"Client has connected");
        }


        private void OnDisconnected() {
            Debug.Log($"Client has disconnected");
        }


        private void OnDataReceived(byte[] messageBytes) {
            Message message = new Message(messageBytes);
            int messageId = message.ReadInt();
            messageHandlers[messageId](message);
        }
        #endregion


        #region Receiving
        [ClientReceive(ServerToClientMessages.welcome)]
        private static void Connected(Message message) {
            client.id = message.ReadInt();
            SendWelcomeReceived();
        }


        [ClientReceive(ServerToClientMessages.clientConnected)]
        private static void ClientConnected(Message message) {
            int clientId = message.ReadInt();
            Debug.Log($"Client ({clientId}) has connected");
        }


        [ClientReceive(ServerToClientMessages.clientDisconnected)]
        private static void ClientDisconnected(Message message) {
            int clientId = message.ReadInt();
            Debug.Log($"Client ({clientId}) has disconnected");
        }
        #endregion


        #region Sending
        private static void SendWelcomeReceived() {
            Message message = new Message((int)ClientToServerMessages.welcomeReceived);
            message.Write(client.id);
            
            Send(message);
        }
        #endregion
    }
}
