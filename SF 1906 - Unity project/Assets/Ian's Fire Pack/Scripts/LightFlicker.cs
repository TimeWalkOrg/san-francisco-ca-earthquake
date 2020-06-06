using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour{
	private float initialValue;//Initial light intensity value
	private Vector3 initialPosition;//Initial transform position
	private Vector3 initialScale;
	private float initialTime;//Initial time offset allows each instance of the fires to appear to flicker differently 
	private Light lightRef;

	public float amount = 0.01f;//Amount to ajust intensity of light
	public float speed = 8;//speed at which to ajust intensity

	public bool adjustLocation;//do we want to randomly offset this position? 
	public float locationAdjustAmount = 1;

	public bool adjustScale = false; 
	public float scaleAdjustAmount = 1;
	public Transform scaleObject;//Incase the scale needs to be applied to a different transform


	void Start () {
		initialTime = Random.value*100;//Get random offset

		lightRef = gameObject.GetComponent<Light>();
		if (lightRef) {
			initialValue = lightRef.intensity;
		}
		if(scaleObject == false){
			scaleObject = transform;
		}

		initialPosition = transform.position;
		initialScale = scaleObject.localScale;
	}

	void Update () {
		float intensityNoise = Mathf.PerlinNoise(Time.time*speed, initialTime);
		if(lightRef){//use perlin noise to ajust intensity
			lightRef.intensity = initialValue + intensityNoise*amount;
		}

		if(adjustLocation){//use perlin noise to ajust position
			Vector3 offset = new Vector3(
									Mathf.PerlinNoise(Time.time*speed, initialTime + 5) - 0.5f,
									intensityNoise - 0.5f,//reuse intensity noise for y offset
									Mathf.PerlinNoise(Time.time*speed, initialTime + 10) - 0.5f);

			transform.position = initialPosition + offset * locationAdjustAmount * 2;
		}

		if(adjustScale){//use perlin noise to ajust scale
			scaleObject.localScale = initialScale * ((intensityNoise-0.5f)*scaleAdjustAmount + 1);
		}
}
}