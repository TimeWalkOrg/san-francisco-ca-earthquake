using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playSoundOnceWhenStartsMoving : MonoBehaviour {
    public AudioClip soundToMake;
    AudioSource audioSource;
    private bool isMoving = false;
    private float oldPositionZ;
    public float minMovement;
    private float movementValue;

    // Use this for initialization
    void Start () {
        oldPositionZ = transform.position.z;
        audioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        // check if started moving
        movementValue = Mathf.Abs(oldPositionZ - transform.position.z);
        if (movementValue <= minMovement) //not moving fast
        {
            isMoving = false;
        } else
        {
            isMoving = true;
        }
        if (movementValue == 0) //stopped
        {
            audioSource.Stop();
        }

            oldPositionZ = transform.position.z;
        if (!audioSource.isPlaying && isMoving)
            audioSource.PlayOneShot(soundToMake, 1.0F);
    }
}
