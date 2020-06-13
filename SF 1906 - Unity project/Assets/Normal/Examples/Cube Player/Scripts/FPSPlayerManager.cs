using UnityEngine;
using Normal.Realtime;

namespace Normal.Realtime.Examples {
    public class FPSPlayerManager : MonoBehaviour {
        private Realtime _realtime;
        public Transform startPosition; // camera object (will be grabbed by player prefab when instantiated)
        public GameObject playerPrefab; // prefab of FPS or VR object

        private void Awake() {
            // Get the Realtime component on this game object
            _realtime = GetComponent<Realtime>();

            // Notify us when Realtime successfully connects to the room
            _realtime.didConnectToRoom += DidConnectToRoom;
        }

        private void DidConnectToRoom(Realtime realtime) {
            // Instantiate the CubePlayer for this client once we've successfully connected to the room
            // Make sure the prefab is saved in a folder named "Resources" (anywhere inside the Assets folder)
            Debug.Log("playerPrefab.name = " + playerPrefab.name);
            Realtime.Instantiate(playerPrefab.name,              // Prefab name
                                position: startPosition.position,   // Set start position for player
                                rotation: startPosition.rotation,   // Set start rotation (was Quaternion.identity)
                           ownedByClient: true,                     // Make sure the RealtimeView on this prefab is owned by this client
                preventOwnershipTakeover: true,                     // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                             useInstance: realtime);                // Use the instance of Realtime that fired the didConnectToRoom event.
        }
    }
}
