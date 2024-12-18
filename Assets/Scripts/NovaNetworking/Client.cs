using System;
using UnityEngine;


namespace NovaNetworking {
    public class Client {
        public int id = 0;
        public Transport transport = new Transport();


        public Client() {}
        
        public Client(int id) {
            this.id = id;
        }


        public void Send(Message packet) {
            packet.WriteLength();
            transport.Send(packet);
        }
    }
}
