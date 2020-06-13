using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// GetCamera finds the main camera and makes it a child of the specified GameObject (the "cameraHolder")
// Best to attach this script to the Player prefab (FPS or VR or whatever)

public class GetCamera : MonoBehaviour
{

    public Camera PrefabCameraToActivate;

    private Camera mainCamera;
    private Canvas HUDCanvas;

    void Start()
    {

    }

    // When this prefab is instantiated (and "Awakes"), grab the camera
    void Awake()
    {
        //This gets the Main Camera from the Scene
        mainCamera = Camera.main;
        //PrefabCameraToActivate.enabled = false; // disable player camera until prefab is "Awake"
        //mainCamera.enabled = true;
        HUDCanvas = GameObject.FindGameObjectWithTag("HUDCanvas").GetComponent<Canvas>();
        Debug.Log("HUDCanvas.name = " + HUDCanvas.name);

        EnablePlayerCamera();
        MoveHUDToPlayerCamera();
    }

    void EnablePlayerCamera()
    {
        // mainCamera.enabled = false;
        PrefabCameraToActivate.enabled = true;
    }
    void MoveHUDToPlayerCamera()
    {
        HUDCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        HUDCanvas.worldCamera = PrefabCameraToActivate.GetComponent<Camera>();
    }
}
