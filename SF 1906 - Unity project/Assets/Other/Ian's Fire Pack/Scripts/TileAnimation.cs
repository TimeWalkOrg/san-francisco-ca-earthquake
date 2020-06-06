using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour{
	public int xFrames = 4;//horizontal frames
	public int yFrames = 4;//virtical frames
	public float speed;//How many frames per second?

	public bool billboard = true;//do we want to billboard this object?

	public Camera mainCamera;

	private int frame = 0;//current frame of animation
	private Renderer rendererReference;
	private int randomStart;//to offset each instance of this animation

	void Awake () {
		rendererReference = gameObject.GetComponent<Renderer>();
		rendererReference.materials[0].mainTextureScale = new Vector2(1.0f/xFrames, 1.0f/yFrames);
		if(billboard){	
			if(!mainCamera){
				mainCamera = Camera.main;
			}
		}
		
		randomStart = (int)(Random.value*xFrames*yFrames);

	}

	void Update () {
		frame = (int)Mathf.Repeat(Mathf.FloorToInt(Time.time*speed) + randomStart, xFrames*yFrames);

		int xOffset = frame % xFrames; 
		int yOffset = frame / xFrames;

		rendererReference.materials[0].mainTextureOffset = new Vector2(xOffset/(xFrames*1.0f), 1 - (yOffset+1)/(yFrames*1.0f));


		if(billboard){
			transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
	            mainCamera.transform.rotation * Vector3.up);
	    }
	}
}