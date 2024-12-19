using System;
using UnityEngine;


namespace NovaNetworking {
    public class Client {
        public int id = 0;
        public Transport transport = new UDPTransport();

        public void Send(Message packet) {
            packet.WriteLength();
            transport.Send(packet);
        }
    }
}
