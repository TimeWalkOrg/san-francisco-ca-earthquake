using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Normal.Realtime;


public class StopwatchToWorld : MonoBehaviour
{
    public Stopwatch stopwatch;
    
    public float timeNow;

    //in case you want to delay instead of starting when the room opens
    public GameObject text;
    private IEnumerator coroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        //should be 0.0 seconds
        timeNow = stopwatch.time;
        
        //Alternate version of starting the timer after a delay. 
        //coroutine = ExecuteAfterTime(3f);
        //StartCoroutine(coroutine);

    }

    // Update is called once per frame
    void Update()
    {
        //If the stopwatch has not started yetm and there is a room created, start the stop watch.
        //trying to start the stopwatch before the room exists fails, and no time will start.
        if(stopwatch.time == 0.0 && stopwatch.CheckRoom()) {
            stopwatch.StartStopwatch();
        }

        //only update the time if the stopwatch is running
        if(stopwatch.time != 0.0) {
            //Set our local time string to a rounded version of the "Normcore" based timer
            timeNow = stopwatch.time;
            timeNow = Mathf.Round(timeNow * 100f) / 100f;
            string timeNowString = timeNow.ToString();
            //Update our In-World text to the timer's time
            var worldTime = text.GetComponent<TMPro.TextMeshPro>();
            worldTime.SetText("Time Since Room Opened: \n" + timeNow + " seconds.");
        }  
    }

    IEnumerator ExecuteAfterTime(float time) {
        yield return new WaitForSeconds(time);
        print("DONE WAITING");
        stopwatch.StartStopwatch();
    }
}

