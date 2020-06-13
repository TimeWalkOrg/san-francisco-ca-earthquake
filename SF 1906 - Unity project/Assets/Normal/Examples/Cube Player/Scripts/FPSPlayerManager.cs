using UnityEngine;
using Normal.Realtime;

namespace Normal.Realtime.Examples {
    public class FPSPlayerManager : MonoBehaviour {
        private Realtime _realtime;
        public Transform startPosition;

        private void Awake() {
            // Get the Realtime component on this game object
            _realtime = GetComponent<Realtime>();

            // Notify us when Realtime successfully connects to the room
            _realtime.didConnectToRoom += DidConnectToRoom;
        }

        private void DidConnectToRoom(Realtime realtime) {
            // Instantiate the CubePlayer for this client once we've successfully connected to the room
            Realtime.Instantiate("FPS 03 - A Walk Down Market Street 1906",                 // Prefab name
                                position: startPosition.position,          // Start 1 meter in the air
                                rotation: Quaternion.identity, // No rotation
                           ownedByClient: true,                // Make sure the RealtimeView on this prefab is owned by this client
                preventOwnershipTakeover: true,                // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                             useInstance: realtime);           // Use the instance of Realtime that fired the didConnectToRoom event.
        }
    }
}
