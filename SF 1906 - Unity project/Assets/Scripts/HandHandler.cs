using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;


// [System.Serializable]
// public class PrimaryButtonEvent : UnityEvent<bool> { }
public class HandHandler : MonoBehaviour
{
    private InputDevice targetDevice;
    public UnityEvent primaryButtonPressed;
    bool lastButtonState = false;
    // bool primaryButtonClick = false;
    void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightController = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightController, devices);

        if(devices.Count > 0)
        {
            targetDevice = devices[0];
        }
    }

    void Update()
    {
        if(targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue) && primaryButtonValue && primaryButtonValue != lastButtonState)
        {
            primaryButtonPressed.Invoke();
        }
        // else primaryButtonClick = false;
        // Debug.Log(primaryButtonClick);

        // if(primaryButtonClick) 
        // {
        //     Debug.Log("Primary click");
        //     primaryButtonPressed.Invoke();
        // }
        lastButtonState = primaryButtonValue;
    }
}
