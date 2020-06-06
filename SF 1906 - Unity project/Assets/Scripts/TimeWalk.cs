using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TimeWalk : MonoBehaviour
{
    public float earthquakeStartDelayInSeconds = 10.0f;
    private bool earthquakeIsStarted = false;
    public AudioSource earthquakeAudioClip;
    public CameraShake cameraShake;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float timeNow = Time.realtimeSinceStartup;
        if(!earthquakeIsStarted && (timeNow> earthquakeStartDelayInSeconds))
        {
            cameraShake.shakecamera(); // starts the camera shaking effect
            earthquakeAudioClip.Play(); // plays the sound
            earthquakeIsStarted = true;
        }
    }
}
