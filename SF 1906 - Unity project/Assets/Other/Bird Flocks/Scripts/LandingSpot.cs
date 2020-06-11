/**************************************									
	Copyright Unluck Software	
 	www.chemicalbliss.com						
***************************************/

using UnityEngine;
using System.Collections;


public class LandingSpot:MonoBehaviour{
    [HideInInspector]
    public FlockChild landingChild;
    [HideInInspector]
    public bool landing;
    int lerpCounter;
    [HideInInspector]
    public LandingSpotController _controller;
    bool _idle;
    public Transform _thisT;					//Reference to transform component
    
    public bool _gotcha;
    
    public void Start() {
    	if(_thisT == null)		_thisT = transform;
        if (_controller == null)
            _controller = _thisT.parent.GetComponent<LandingSpotController>();
        if (_controller._autoCatchDelay.x > 0)
            StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));   
    }
    
    public void OnDrawGizmos() {
    	if(_thisT == null)		_thisT = transform;
    	if (_controller == null)
            _controller = _thisT.parent.GetComponent<LandingSpotController>();
        
        Gizmos.color = Color.yellow;
        // Draw a yellow cube at the transforms position
        if ((landingChild != null) && landing)
            Gizmos.DrawLine(_thisT.position, landingChild._thisT.position);
        if (_thisT.rotation.eulerAngles.x != 0 || _thisT.rotation.eulerAngles.z != 0)
            _thisT.eulerAngles = new Vector3(0.0f, _thisT.eulerAngles.y, 0.0f);
        Gizmos.DrawCube(new Vector3(_thisT.position.x, _thisT.position.y, _thisT.position.z), Vector3.one * _controller._gizmoSize);
        Gizmos.DrawCube(_thisT.position + (_thisT.forward *  _controller._gizmoSize), Vector3.one * _controller._gizmoSize *.5f);
        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, .05f);
        Gizmos.DrawWireSphere(_thisT.position, _controller._maxBirdDistance);
    }
    
    public void LateUpdate() {
		if (landingChild == null) {
			_gotcha = false;
			_idle = false;
			lerpCounter = 0;
			return;
		}
        if(_gotcha){
        		landingChild.transform.position = _thisT.position + landingChild._landingPosOffset;
        		RotateBird();
        		return;
      		}
        if (_controller._flock.gameObject.activeInHierarchy && landing && (landingChild != null)) {
        	if(!landingChild.gameObject.activeInHierarchy){ 
        		Invoke("ReleaseFlockChild", 0.0f);
        	}
        	//Check distance to flock child
            float distance = Vector3.Distance(landingChild._thisT.position, _thisT.position+ landingChild._landingPosOffset);
            //Start landing if distance is close enough
            if (distance < 5 && distance > .5f) {
                if(_controller._soarLand){
                	landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._soarAnimation, .5f);
                	if (distance < 2)
               	 		landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, .5f);
                }
                landingChild._targetSpeed = landingChild._spawner._maxSpeed*_controller._landingSpeedModifier;
              	landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;      	
                landingChild._damping = _controller._landingTurnSpeedModifier;
                landingChild._avoid = false;
            } else if (distance <= .5f) {    	
                landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;      
                if (distance < _controller._snapLandDistance && !_idle) {
                    _idle = true;
                    landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._idleAnimation, .55f); 
                    
                } 
                if (distance > _controller._snapLandDistance){       	
                	landingChild._targetSpeed = landingChild._spawner._minSpeed*this._controller._landingSpeedModifier;
              	    landingChild._thisT.position += (_thisT.position + landingChild._landingPosOffset - landingChild._thisT.position) * Time.deltaTime *landingChild._speed*_controller._landingSpeedModifier *2;     	
              	}else{
              		_gotcha = true;
              	}
                landingChild._move = false;
         		RotateBird();
    
               // landingChild._damping =  _controller._landingTurnSpeedModifier;
            } else {
            	//Move towards landing spot
                landingChild._wayPoint = _thisT.position + landingChild._landingPosOffset;
               
            }
    		landingChild._damping += .01f;
        }
		StraightenBird();

	}
    
	public void StraightenBird(){
		if (landingChild._thisT.eulerAngles.x == 0) return;
		Vector3 r = landingChild._thisT.eulerAngles;
		r.z = 0;
		landingChild._thisT.eulerAngles = r;
	}

    public void RotateBird(){
    	if(_controller._randomRotate && _idle) return;
    	lerpCounter++;
    	Quaternion rot = landingChild._thisT.rotation;
        Vector3 rotE = rot.eulerAngles;     		
        rotE.y = Mathf.LerpAngle(landingChild._thisT.rotation.eulerAngles.y, _thisT.rotation.eulerAngles.y, lerpCounter * Time.deltaTime * _controller._landedRotateSpeed);  		
        rot.eulerAngles = rotE;
        landingChild._thisT.rotation = rot;
    }
    
    public IEnumerator GetFlockChild(float minDelay,float maxDelay) {
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        if (_controller._flock.gameObject.activeInHierarchy && (landingChild == null)) {
            FlockChild fChild = null;
    
            for(int i = 0; i < _controller._flock._roamers.Count; i++) {
                FlockChild child = _controller._flock._roamers[i];
                if (!child._landing && !child._dived) {         
                	if(!_controller._onlyBirdsAbove){     	
    	                if ((fChild == null) && _controller._maxBirdDistance > Vector3.Distance(child._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(child._thisT.position, _thisT.position)) {
    	                    fChild = child;
    	                    if (!_controller._takeClosest) break;
    	                } else if ((fChild != null) && Vector3.Distance(fChild._thisT.position, _thisT.position) > Vector3.Distance(child._thisT.position, _thisT.position)) {
    	                    fChild = child;
    	                }
                    }else{
                    	if ((fChild == null) && child._thisT.position.y > _thisT.position.y && _controller._maxBirdDistance > Vector3.Distance(child._thisT.position, _thisT.position) && _controller._minBirdDistance < Vector3.Distance(child._thisT.position, _thisT.position)) {
    	                    fChild = child;
    	                    if (!_controller._takeClosest) break;
    	                } else if ((fChild != null) && child._thisT.position.y > _thisT.position.y && Vector3.Distance(fChild._thisT.position, _thisT.position) > Vector3.Distance(child._thisT.position, _thisT.position)) {
    						fChild = child;
    	                }
                    }
                }
            }
            if (fChild != null) {
                landingChild = fChild;
                landing = true;
               	landingChild._landing = true;
               	if(_controller._autoDismountDelay.x > 0) Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
				_controller._activeLandingSpots++;
            } else if (_controller._autoCatchDelay.x > 0) {
                StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
            }
        }
    }
    
    public void InstantLand() {
        if (_controller._flock.gameObject.activeInHierarchy && (landingChild == null)) {
            FlockChild fChild = null;
          
            for(int i = 0; i < _controller._flock._roamers.Count; i++) {
                FlockChild child = _controller._flock._roamers[i];
                if (!child._landing && !child._dived) {
                         fChild = child;           
                }
            }
            if (fChild != null) {
                landingChild = fChild;
                landing = true;
    			_controller._activeLandingSpots++;
                landingChild._landing = true;
                landingChild._thisT.position = _thisT.position + landingChild._landingPosOffset;
                landingChild._model.GetComponent<Animation>().Play(landingChild._spawner._idleAnimation);
				landingChild._thisT.Rotate(Vector3.up, Random.Range(0f, 360f));
              if(_controller._autoDismountDelay.x > 0)  Invoke("ReleaseFlockChild", Random.Range(_controller._autoDismountDelay.x, _controller._autoDismountDelay.y));
            } else if (_controller._autoCatchDelay.x > 0) {
                StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x, _controller._autoCatchDelay.y));
            }
        }
    }
    
    public void ReleaseFlockChild() {
		
        if (_controller._flock.gameObject.activeInHierarchy && (landingChild != null)) {
			_gotcha = false;
            lerpCounter = 0;
            if (_controller._featherPS != null){
    			_controller._featherPS.position = landingChild._thisT.position;
    			_controller._featherPS.GetComponent<ParticleSystem>().Emit(Random.Range(0,3));
            }           
    		landing = false;
            _idle = false;
            landingChild._avoid = true;
            //Reset flock child to flight mode
            landingChild._damping = landingChild._spawner._maxDamping;
            landingChild._model.GetComponent<Animation>().CrossFade(landingChild._spawner._flapAnimation, .2f);
    		landingChild._dived = true;
            landingChild._speed = 0.0f;    
            landingChild._move = true;
            landingChild._landing = false;
            landingChild.Flap();     	
            landingChild._wayPoint = new Vector3(landingChild._wayPoint.x, _thisT.position.y+10, landingChild._wayPoint.z);

			if (_controller._autoCatchDelay.x > 0) {
                StartCoroutine(GetFlockChild(_controller._autoCatchDelay.x + 0.1f, _controller._autoCatchDelay.y + 0.1f));
            }

			landingChild = null;
			_controller._activeLandingSpots--;
		}
    }
}
