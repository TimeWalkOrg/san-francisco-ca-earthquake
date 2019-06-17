using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeWalkHideAfterXSeconds : MonoBehaviour
{
    public float secondsToDisplay = 10f;
    void Start()
    {
        if (gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        StartCoroutine(LateCall());
    }

    IEnumerator LateCall()
    {

        yield return new WaitForSeconds(secondsToDisplay);

        gameObject.SetActive(false);
        //Do Function here...
    }
}
