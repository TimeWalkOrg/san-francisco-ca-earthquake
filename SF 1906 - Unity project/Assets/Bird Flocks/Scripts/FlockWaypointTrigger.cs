using UnityEngine;

public class FlockWaypointTrigger:MonoBehaviour{
    public float _timer =1.0f;
    public FlockChild _flockChild;
    
    public void Start() {
    	if(_flockChild == null)
    	_flockChild = transform.parent.GetComponent<FlockChild>();
    	float timer = Random.Range(_timer, _timer*3);
    	InvokeRepeating("Trigger", timer, timer);	
    }
    
    public void Trigger() {
    	_flockChild.Wander(0.0f);
    }
}
