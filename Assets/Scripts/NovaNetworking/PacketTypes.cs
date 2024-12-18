namespace NovaNetworking {
    public enum ServerToClientPackets {
        welcome,                // Send to the client that joined
        clientConnected,        // Send to all client that a client has joined and to spwan their player model
        clientDisconnected,     // Send to all client that a client has left and to destory their player model
    }


    public enum ClientToServerPackets {
        welcomeReceived,        // Send back the welcome received to established a connection
    }
}
