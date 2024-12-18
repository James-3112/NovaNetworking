namespace NovaNetworking {
    public enum ServerToClientPackets {
        welcome,                // Send to the client that joined
        playerConnected,        // Send to all client that a player has joined and to spwan their player model
        playerDisconnected,     // Send to all client that a player has left and to destory their player model
        playerMovement          // Send to all client that a player has moved and to update their player model
    }


    public enum ClientToServerPackets {
        welcomeReceived,        // Send back the welcome received to established a connection
        playerInputs            // Send the players input to calculate movement (Server Authoritative)
    }
}
