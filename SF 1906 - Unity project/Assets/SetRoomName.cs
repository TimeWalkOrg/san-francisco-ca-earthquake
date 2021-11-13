using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Normal.Realtime;

public class SetRoomName : MonoBehaviour
{
    private GameObject lobbyName;
    private Realtime realtime;
    // Start is called before the first frame update
    void Awake()
    {
        realtime = this.GetComponent<Realtime>();
        lobbyName = GameObject.FindGameObjectWithTag("LobbyName");
        if (lobbyName != null) {
            if (lobbyName.GetComponent<Text>() != null) {
                string roomName = lobbyName.GetComponent<Text>().text;
                if (roomName != "") {
                    realtime.GetComponent<Realtime>().Connect(roomName);
                }
            }
        }
        else {
            Debug.LogError("No Room Name Found; Connecting to Default Room Name 'SF Earthquake'");
            realtime.GetComponent<Realtime>().Connect("SF Earthquake");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
