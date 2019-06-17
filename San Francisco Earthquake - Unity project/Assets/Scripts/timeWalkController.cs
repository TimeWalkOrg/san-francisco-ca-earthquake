using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeWalkController : MonoBehaviour
{
    //define a list where the prefabs will be stored
    public static List<GameObject> myListObjects = new List<GameObject>();
    public static int numSpawned = 0;
    public static int numToSpawn = 3;
    public static int currentObjectIndex = 0;
    public static int objectsListLength = 0;
    public static GameObject currentObject;
    public static int incrementObject = 0;
    public static bool audioPlayingNow = true;

    public AudioSource audioData;

    void Start()
    {
        // NOTE: make sure all building prefabs are inside this folder: "Assets/Resources/Prefabs"

        Object[] subListObjects = Resources.LoadAll("Prefabs", typeof(GameObject));
        foreach (GameObject subListObject in subListObjects)
        {
            GameObject lo = (GameObject)subListObject;
            myListObjects.Add(lo);
            ++objectsListLength;
        }
        GameObject myObj = Instantiate(myListObjects[currentObjectIndex]) as GameObject;
        currentObject = myObj;

        audioData = GetComponent<AudioSource>();
        audioData.Play(0);
        // Debug.Log("started");
    }

    void SpawnNextObject(int incrementNumber)
    {
        Destroy(currentObject);
        currentObjectIndex = currentObjectIndex + incrementNumber;
        if (currentObjectIndex >= objectsListLength)
        {
            currentObjectIndex = 0;
        }

        if (currentObjectIndex < 0)
        {
            currentObjectIndex = objectsListLength - 1;
        }

        GameObject myObj = Instantiate(myListObjects[currentObjectIndex]) as GameObject;
        // myObj.transform.position = transform.position; // NO: instead we will use the object's default position
        currentObject = myObj;
        // Debug.Log("currentObjectIndex = " + currentObjectIndex);
    }

    void Update()
    {
        incrementObject = 0;
        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.N))
        // if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            incrementObject = 1;
            SpawnNextObject(incrementObject);
        }
        if (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.P))

        {
            incrementObject = -1;
            SpawnNextObject(incrementObject);
        }

        // Pause-unpause music
        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch) || Input.GetKeyDown(KeyCode.Space))

        {
            if (audioPlayingNow)
            {
                audioData.Pause();
                audioPlayingNow = false;
            }
            else
            {
                audioData.UnPause();
                audioPlayingNow = true;
            }

        }
    }

}
