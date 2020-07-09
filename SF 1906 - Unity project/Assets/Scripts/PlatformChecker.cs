using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlatformChecker : MonoBehaviour
{
    public SC_FPSController fpsController;
    void OnEnable()
    {

#if UNITY_STANDALONE || UNITY_EDITOR

            fpsController.enabled = !isPresent();
#endif

    }

    public bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
            Debug.Log("xr");
                return true;
            }
        }
        return false;
    }
}
