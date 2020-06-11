/**************************************									
	LandingSpotController
	Copyright Unluck Software	
 	www.chemicalbliss.com					
***************************************/

using UnityEngine;
using System.Collections;


public class LandingSpotController:MonoBehaviour{
    public bool _randomRotate = true;					// Random rotation when a bird lands
    public Vector2 _autoCatchDelay = new Vector2(10.0f, 20.0f);		// Random Min/Max time for landing spot to make a bird land
    public Vector2 _autoDismountDelay = new Vector2(10.0f, 20.0f);	// Random Min/Max time for birds to automaticly fly away from landing spot
    public float _maxBirdDistance = 20.0f;					// The maximum distance to a bird for it to land
    public float _minBirdDistance = 5.0f;						// The minimum distance to a bird for it to land
    public bool _takeClosest;							// Toggle this to make landingspots make the closest bird to it land
    public FlockController _flock;							// Assign the FlockController to pick birds from
    public bool _landOnStart;							// Put birds on the landing spots at start
    public bool _soarLand = true;						// Birds will soar while aproaching landing spot
    public bool _onlyBirdsAbove;						// Only birds above landing spot will land
    public float _landingSpeedModifier = .5f;				// Adjust bird movement speed while clost to the landing spot
    public float _landingTurnSpeedModifier = 5.0f;
    public Transform _featherPS;							// Update: Changed from GameObject to Transform 
    public Transform _thisT;								// Transform reference
    public int _activeLandingSpots;
    
    public float _snapLandDistance = 0.1f;						// Increase this if landing spots are moving
    public float _landedRotateSpeed = 0.01f;

	public float _gizmoSize = 0.2f;

    public void Start() {
    	if(_thisT == null) _thisT = transform;
    	if(_flock == null){
    	 _flock = (FlockController)GameObject.FindObjectOfType(typeof(FlockController));
    	 Debug.Log(this + " has no assigned FlockController, a random FlockController has been assigned");
    	 }
    	 
    	#if UNITY_EDITOR
    	if(_autoCatchDelay.x >0 &&(_autoCatchDelay.x < 5||_autoCatchDelay.y < 5)){
    		Debug.Log(this.name + ": autoCatchDelay values set low, this might result in strange behaviours");
    	}
    	#endif
    	
    	if(_landOnStart){
    		StartCoroutine(InstantLandOnStart(.1f));
    	}
    }
    
    public void ScareAll() {
    	ScareAll(0.0f,1.0f);
    }
    
    public void ScareAll(float minDelay,float maxDelay) {
    	for(int i=0;  i< _thisT.childCount; i++){
    		if(_thisT.GetChild(i).GetComponent<LandingSpot>() != null){
    		LandingSpot spot = _thisT.GetChild(i).GetComponent<LandingSpot>();
    		spot.Invoke("ReleaseFlockChild", Random.Range(minDelay,maxDelay));
    		}
    	}
    }
    
    public void LandAll() {
    	for(int i=0;  i< _thisT.childCount; i++){	
    		if(_thisT.GetChild(i).GetComponent<LandingSpot>() != null){		
    		LandingSpot spot = _thisT.GetChild(i).GetComponent<LandingSpot>();
    		StartCoroutine(spot.GetFlockChild(0.0f,2.0f));
    		}
    	}
    }
    
    //This function was added to fix a error with having a button calling InstantLand
    public IEnumerator InstantLandOnStart(float delay) {
    	yield return new WaitForSeconds(delay);
    	for(int i=0;  i< _thisT.childCount; i++){			
    		if(_thisT.GetChild(i).GetComponent<LandingSpot>() != null){
    		LandingSpot spot = _thisT.GetChild(i).GetComponent<LandingSpot>();
    		spot.InstantLand();
    		}
    	}
    }
    
    public IEnumerator InstantLand(float delay) {
    	yield return new WaitForSeconds(delay);
    	for(int i=0;  i< _thisT.childCount; i++){	
    		if(_thisT.GetChild(i).GetComponent<LandingSpot>() != null){		
    		LandingSpot spot = _thisT.GetChild(i).GetComponent<LandingSpot>();
    		spot.InstantLand();
    		}
    	}
    }
}
