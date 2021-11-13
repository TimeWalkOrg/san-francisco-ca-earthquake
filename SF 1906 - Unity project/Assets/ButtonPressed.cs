using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Normal.Realtime;

public class ButtonPressed : Selectable
{

    public Button thisButton;
    public GameObject roomConnector;
    private bool selected = false;
    private string newRoomKey = "54598b51-eee1-45a9-9eee-86a29a440b26";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OVRButtonPress();
        ButtonSelect();
        OVRInput.Update();
    }

    private void FixedUpdate() {
        OVRInput.FixedUpdate();
    }

    private void ButtonSelect() {
        if (selected) {
            ChangeRoomKey("SF Lobby 1");
        }
    }

    //check if the button is highlighted and then the ovr 'A' button is pressed
    void OVRButtonPress() {
        if(IsHighlighted() == true) {
            if(OVRInput.Get(OVRInput.Button.One) == true && selected == false) {
                thisButton.Select();
                selected = true;
            }
            else if(OVRInput.Get(OVRInput.Button.One) == true && selected == true) {
                GameObject myEventSystem = GameObject.Find("EventSystem");
                myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
                selected = true;
            }
        }
    }

    void ChangeRoomKey(String LobbyName) {
        //roomConnector.GetComponent<Realtime>().normcoreAppSettings.CreateInstance("54598b51 - eee1 - 45a9 - 9eee - 86a29a440b26", string matcherURL = wss://normcore-matcher.normcore.io:3000)
        var tempRoomName = roomConnector.GetComponent<Realtime>().room.name;
        if(tempRoomName != LobbyName) {
            roomConnector.GetComponent<Realtime>().Connect(LobbyName);
        }
            //("54598b51 - eee1 - 45a9 - 9eee - 86a29a440b26");
    }
}
