using UnityEngine;
using Normal.Realtime;
public class InstantiateGrabbableObject : MonoBehaviour {
    private Realtime _realtime;
    private void awake() {
        // Get the Realtime component on this game object
        _realtime = GetComponent<Realtime>();
        // Notify us when Realtime successfully connects to the room
        _realtime.didConnectToRoom += DidConnectToRoom;
        Debug.Log("in script");
    }
    private void DidConnectToRoom(Realtime realtime) {
        print("spawn timer");
        //Instantiate the CubePlayer for this client once we've successfully connected to the room
        Realtime.Instantiate("UI Timer Controller",                 // Prefab name
        position: new Vector3(-185, 13, -185),          
        rotation: Quaternion.identity, // No rotation
        ownedByClient: false,   // Make sure the RealtimeView on this prefab is NOT owned by this client
        preventOwnershipTakeover: false,                // DO NOT prevent other clients from calling RequestOwnership() on the root RealtimeView.
        useInstance: realtime);           // Use the instance of Realtime that fired the didConnectToRoom event.
    }
}
