using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbySwitcher : MonoBehaviour {

    public GameObject roomConnector;
    public GameObject LobbyTitle;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        OVRButtonPress();
        OVRInput.Update();
    }

    private void FixedUpdate() {
        OVRInput.FixedUpdate();
    }

    void OVRButtonPress() {
        if (OVRInput.Get(OVRInput.Button.One) == true || Input.GetKeyDown(KeyCode.Alpha1)) {
            //ChangeRoomKey("SF Lobby B Side");
            SceneManager.LoadScene(1);
        }
        else if (OVRInput.Get(OVRInput.Button.Two) == true || Input.GetKeyDown(KeyCode.Alpha2)) {
            //ChangeRoomKey("SF Lobby");
            SceneManager.LoadScene(2);
        }
    }


    void ChangeRoomKey(String LobbyName) {
        //roomConnector.GetComponent<Realtime>().normcoreAppSettings.CreateInstance("54598b51 - eee1 - 45a9 - 9eee - 86a29a440b26", string matcherURL = wss://normcore-matcher.normcore.io:3000)
        var tempRoomName = roomConnector.GetComponent<Realtime>().room.name;
        if (tempRoomName != LobbyName) {
            roomConnector.GetComponent<Realtime>().Connect(LobbyName);
            LobbyTitle.GetComponent<TMPro.TextMeshProUGUI>().SetText(LobbyName);
            roomConnector.GetComponent<Realtime>().room.Tick(Time.deltaTime);
        }   
    }

}
