/****************************************
	Copyright 2016 Unluck Software	
 	www.chemicalbliss.com 	 																																																																							
*****************************************/

using UnityEngine;
using System.Collections.Generic;


public class FlockController:MonoBehaviour{
    
    public FlockChild _childPrefab;			// Assign prefab with FlockChild script attached
    public int _childAmount = 250;				// Number of objects
    public bool _slowSpawn;					// Birds will not be instantiated all at once at start
    public float _spawnSphere = 3.0f;				// Range around the spawner waypoints will created
    public float _spawnSphereHeight = 3.0f;		// Height of the spawn sphere
    public float _spawnSphereDepth = -1.0f;
    public float _minSpeed = 6.0f;				// minimum random speed
    public float _maxSpeed = 10.0f;				// maximum random speed
    public float _minScale = .7f;				// minimum random size
    public float _maxScale = 1.0f;				// maximum random size
    public float _soarFrequency = 0.0f;			// How often soar is initiated 1 = always 0 = never
    public string _soarAnimation="Soar";		// Animation -required- for soar functionality
    public string _flapAnimation="Flap";		// Animation used for flapping
    public string _idleAnimation="Idle";		// Animation -required- for sitting idle functionality
    public float _diveValue = 7.0f;				// Dive depth
    public float _diveFrequency = 0.5f;			// How often dive 1 = always 0 = never
    public float _minDamping = 1.0f;				// Rotation tween damping, lower number = smooth/slow rotation (if this get stuck in a loop, increase this value)
    public float _maxDamping = 2.0f;
    public float _waypointDistance = 1.0f;		// How close this can get to waypoint before creating a new waypoint (also fixes stuck in a loop)
    public float _minAnimationSpeed = 2.0f;		// Minimum animation speed
    public float _maxAnimationSpeed = 4.0f;		// Maximum animation speed
    public float _randomPositionTimer = 10.0f;	// *** 
    public float _positionSphere = 25.0f;			// If _randomPositionTimer is bigger than zero the controller will be moved to a random position within this sphere
    public float _positionSphereHeight = 25.0f;	// Overides height of sphere for more controll
    public float _positionSphereDepth = -1.0f;
    public bool _childTriggerPos;			// Runs the random position function when a child reaches the controller
    public bool _forceChildWaypoints;		// Forces all children to change waypoints when this changes position
    public float _forcedRandomDelay = 1.5f;		// Random delay added before forcing new waypoint
    public bool _flatFly;					// Birds will not rotate upwards as much when flapping
    public bool _flatSoar;					// Birds will not rotate upwards as much when soaring
    public bool _birdAvoid;					// Avoid colliders left and right
    public int _birdAvoidHorizontalForce = 1000; // How much a bird will react to avoid collision left and right
    public bool _birdAvoidDown;				// Avoid colliders below
    public bool _birdAvoidUp;				// Avoid colliders above bird
    public int _birdAvoidVerticalForce = 300;	// How much a bird will react to avoid collision down and up
    public float _birdAvoidDistanceMax = 4.5f;	// Maximum distance to check for collision to avoid
    public float _birdAvoidDistanceMin = 5.0f;	// Minimum distance to check for collision to avoid
    public float _soarMaxTime;					// Stops soaring after x seconds, use to avoid birds soaring for too long
    public LayerMask _avoidanceMask = (LayerMask)(-1);		// Avoidance collider mask
    public List<FlockChild> _roamers;
    public Vector3 _posBuffer;
    public int _updateDivisor = 1;				//Skip update every N frames (Higher numbers might give choppy results, 3 - 4 on 60fps , 2 - 3 on 30 fps)
    public float _newDelta;
    public int _updateCounter;
    public float _activeChildren;
    public bool _groupChildToNewTransform;	// Parents fish transform to school transform
    public Transform _groupTransform;			//
    public string _groupName = "";				//
    public bool _groupChildToFlock;			// Parents fish transform to school transform
    public Vector3 _startPosOffset;
    public Transform _thisT;					// Reference to the transform component
    
    public void Start() {
    	_thisT = transform;
    	///FIX FOR UPDATING FROM OLDER VERSION
    	if(_positionSphereDepth == -1){
    		_positionSphereDepth = _positionSphere;
    	}	
    	if(_spawnSphereDepth == -1){
    		_spawnSphereDepth = _spawnSphere;
    	}
    	///FIX	
    	_posBuffer = _thisT.position+_startPosOffset;
    	if(!_slowSpawn){
    		AddChild(_childAmount);
    	}
    	if(_randomPositionTimer > 0) InvokeRepeating("SetFlockRandomPosition", _randomPositionTimer, _randomPositionTimer); // > C
    }
    
    public void AddChild(int amount){
    	if(_groupChildToNewTransform)InstantiateGroup();
    	for(int i=0;i<amount;i++){
    		FlockChild obj = (FlockChild)Instantiate(_childPrefab);	
    	    obj._spawner = this;
    	    _roamers.Add(obj);
    	   AddChildToParent(obj.transform);
    	}	
    }
    
    public void AddChildToParent(Transform obj){	
        if(_groupChildToFlock){
    		obj.parent = transform;
    		return;
    	}
    	if(_groupChildToNewTransform){
    		obj.parent = _groupTransform;
    		return;
    	}
    }
    
    public void RemoveChild(int amount){
    	for(int i=0;i<amount;i++){
    		FlockChild dObj = _roamers[_roamers.Count-1];
    		_roamers.RemoveAt(_roamers.Count-1);
    		Destroy(dObj.gameObject);
    	}
    }
    
    public void Update() {
    	if(_activeChildren > 0){
    		if(_updateDivisor > 1){
    			_updateCounter++;
    		    _updateCounter = _updateCounter % _updateDivisor;	
    			_newDelta = Time.deltaTime*_updateDivisor;	
    		}else{
    			_newDelta = Time.deltaTime;
    		}	
    	}
    	UpdateChildAmount();
    }
    
    public void InstantiateGroup(){
    	if(_groupTransform != null) return;
    	GameObject g = new GameObject();
    
    	_groupTransform = g.transform;
    	_groupTransform.position = _thisT.position;
    	
    	if(_groupName != ""){
    		g.name = _groupName;
    		return;
    	}	
    	g.name = _thisT.name + " Fish Container";
    }
    
    public void UpdateChildAmount(){	
    	if(_childAmount>= 0 && _childAmount < _roamers.Count){
    		RemoveChild(1);
    		return;
    	}
    	if (_childAmount > _roamers.Count){	
    		AddChild(1);
    	}
    }
    
    public void OnDrawGizmos() {
    	if(_thisT == null) _thisT = transform;
    		if(!Application.isPlaying && _posBuffer != _thisT.position+_startPosOffset){
    			_posBuffer = _thisT.position+_startPosOffset;
           		
           	}
           	if(_positionSphereDepth == -1){
    				_positionSphereDepth = _positionSphere;
    			}	
    			if(_spawnSphereDepth == -1){
    				_spawnSphereDepth = _spawnSphere;
    			}
           	Gizmos.color = Color.blue;
           	Gizmos.DrawWireCube (_posBuffer, new Vector3(_spawnSphere*2, _spawnSphereHeight*2 ,_spawnSphereDepth*2));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube (_thisT.position, new Vector3((_positionSphere*2)+_spawnSphere*2, (_positionSphereHeight*2)+_spawnSphereHeight*2 ,(_positionSphereDepth*2)+_spawnSphereDepth*2));
        }
    
    //Set waypoint randomly inside box
    public void SetFlockRandomPosition() {
    	Vector3 t = Vector3.zero;
    	t.x = Random.Range(-_positionSphere, _positionSphere) + _thisT.position.x;
    	t.z = Random.Range(-_positionSphereDepth, _positionSphereDepth) + _thisT.position.z;
    	t.y = Random.Range(-_positionSphereHeight, _positionSphereHeight) + _thisT.position.y;
    //	var hit : RaycastHit;
    //	if (Physics.Raycast(_posBuffer, t, hit, Vector3.Distance(_posBuffer, t))){
    //			_posBuffer.LookAt(hit.point);
    //			t = hit.point - (_thisT.forward*-3);
    //	}
    	_posBuffer = t;	
    	if(_forceChildWaypoints){
    		for(int i = 0; i < _roamers.Count; i++) {
      		 	(_roamers[i]).Wander(Random.value*_forcedRandomDelay);
    		}	
    	}
    }
    
    //Instantly destroys all birds
    public void destroyBirds() {
    		for(int i = 0; i < _roamers.Count; i++) {
    			Destroy((_roamers[i]).gameObject);	
    		}
    		_childAmount = 0;
    		_roamers.Clear();
    }
}
