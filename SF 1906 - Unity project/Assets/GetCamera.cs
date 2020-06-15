using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// GetCamera finds the main camera and makes it a child of the specified GameObject (the "cameraHolder")
// Best to attach this script to the Player prefab (FPS or VR or whatever)

public class GetCamera : MonoBehaviour
{

    public Camera PrefabCameraToActivate;

    private Camera mainCamera;

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
        EnablePlayerCamera();
    }

    void EnablePlayerCamera()
    {
        // mainCamera.enabled = false;
        PrefabCameraToActivate.enabled = true;
    }
}
