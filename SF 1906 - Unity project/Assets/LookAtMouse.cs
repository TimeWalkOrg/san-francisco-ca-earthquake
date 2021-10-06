using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    [SerializeField] Camera playerCam;
    private GameObject player;
    private float dampening = .71f;

    // Start is called before the first frame update
    void Start()
    {
        GetChild();
    }

    private void GetChild() {
        player = this.gameObject;
            //.gameObject.transform.GetChild(0).GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        Vector3 STWP = (new Vector3(mouse.x, mouse.y, playerCam.nearClipPlane));
        //print(STWP);
        //print(mouse);
        Vector3 mouseWorld = playerCam.ScreenToWorldPoint(STWP);
        //print("SWPT" + STWP);
        Vector3 forward = mouseWorld - player.transform.position;
        print("Forward" + forward);
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rotation, Time.deltaTime * dampening);
        //print(player.transform.rotation);
    }
}
