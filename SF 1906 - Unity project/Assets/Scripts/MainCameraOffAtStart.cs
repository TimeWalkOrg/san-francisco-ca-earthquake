using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraOffAtStart : MonoBehaviour
{
    public GameObject mainCamera;
    private bool mainCameraOn = true;
    void Update()
    {
        if(mainCameraOn)
        {
            if (GameObject.FindGameObjectsWithTag("Player").Length != 0)
            {
                mainCamera.SetActive(false);
                mainCameraOn = false;
            }
            
        }
        
    }

}
