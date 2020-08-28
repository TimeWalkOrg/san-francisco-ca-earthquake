using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class TimeWalk : MonoBehaviour
{
    public float earthquakeStartDelayInSeconds = 10.0f;
    private bool earthquakeIsStarted = false;
    //public AudioSource earthquakeAudioClip;
    public GameObject earthquakeObjects;
    public CameraShake cameraShake;
    public TextMeshPro locationHUD;
    public TextMeshPro dateHUD;
    public TextMeshPro timeHUD;
    public TextMeshPro weatherHUD;
    public TextMeshPro currentDateUIText;
    public TextMeshPro currentTimeUIText;
    public TextMeshPro hudVRText;
    private DateTime startClockTime;
    public int startYear;
    public int startMonth;
    public int startDay;
    public int startHour;
    public int startMinute;
    public int startSecond;
    private TimeSpan timeDelta;

    void Start()
    {
        startClockTime = new DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond);
        DateTime startActualTime = DateTime.Now;
        timeDelta = startClockTime - startActualTime;
        string startDateString = startClockTime.ToString("MMMM dd, yyyy");
        currentDateUIText.text = startDateString;
        earthquakeObjects.gameObject.SetActive(false);
    }

    void Update()
    {
        float timeNow = Time.realtimeSinceStartup;
        if(!earthquakeIsStarted && (timeNow> earthquakeStartDelayInSeconds))
        {
            cameraShake.shakecamera(); // starts the camera shaking effect
            //earthquakeAudioClip.Play(); // plays the sound
            earthquakeIsStarted = true;
            earthquakeObjects.gameObject.SetActive(true);
        }


        // Clock time setting
        System.DateTime time = DateTime.Now;
        time = time + timeDelta;
        int currentHour = time.Hour;
        string appendTimeText = "am";
        if(currentHour > 12)
        {
            currentHour = currentHour - 12;
            appendTimeText = "pm";
        }
        string hour = time.Hour.ToString();
        string minute = LeadingZero(time.Minute);
        string second = LeadingZero(time.Second);
        currentTimeUIText.text = hour + ":" + minute + ":" + second + " " + appendTimeText;
        timeHUD.text = currentTimeUIText.text;

        //TODO: Update Date when clocks rolls past midnight!

        UpdateVR();
    }


    void UpdateVR()
    {
        hudVRText.text = locationHUD.text + "\n\n"
            + dateHUD.text + "\n"
            + timeHUD.text + "\n"
            + weatherHUD.text;
    }
    string LeadingZero(int n)
    {
        return n.ToString().PadLeft(2, '0');
        //return n.ToString();
    }
}
