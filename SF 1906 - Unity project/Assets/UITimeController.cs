using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeController : MonoBehaviour { 
    public GameObject text;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float timeNow = 59f - Time.realtimeSinceStartup;
        timeNow = Mathf.Round(timeNow * 100f) / 100f;
        string timeNowString = timeNow.ToString();
        var countdown = text.GetComponent<TMPro.TextMeshPro>();
        countdown.SetText("Countdown to Earthquake: " + timeNowString);

    }
}
