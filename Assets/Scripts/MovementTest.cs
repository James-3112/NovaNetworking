using NovaNetworking;
using UnityEngine;

public class MovementTest : MonoBehaviour {
    // Client
    private void CalculateInputs() {
        // Send inputs to server
    }

    [ClientReceive(ServerToClientPackets.playerMovement)]
    private static void UpdateMovement(Packet packet) {
        // Update the players position
    }

    
    // Server
    [ServerReceive(ClientToServerPackets.playerInputs)]
    private static void ServerReceiveInputs(Packet packet) {
        // Calculate Movement
        // Send movement back
    }
}
