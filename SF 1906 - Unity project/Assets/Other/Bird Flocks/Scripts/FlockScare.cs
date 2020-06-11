using UnityEngine;
using System.Collections;

public class FlockScare : MonoBehaviour {

	public LandingSpotController[] landingSpotControllers;     //List of landingspot controllers containing landingspots to scare
	public float scareInterval = 0.1f;                         //How often to check the next spot if it is close enough to scare (Every Nth second)
	public float distanceToScare = 2f;                          //How far from a landingspot this transform can call scare function
	public int checkEveryNthLandingSpot = 1;                  //Skip landingspots to move on to the next controller quicker (All spots will be included before the count restarts completely)
	public int InvokeAmounts = 1;                              //How many instances of Invokes to run (Only go above 1 when more controllers than the Interval can handle) 
														//Can not be changed while game is running unless script is disabled then enabled

	private int lsc;                                    //Counters used to keep track of what landingspots to check for distance
	private int ls;
	private LandingSpotController currentController;

	void CheckProximityToLandingSpots() {
		IterateLandingSpots();
		if (currentController._activeLandingSpots > 0 && CheckDistanceToLandingSpot(landingSpotControllers[lsc])) {
			landingSpotControllers[lsc].ScareAll();
		}
		Invoke("CheckProximityToLandingSpots", scareInterval);
	}

	//Counts trough all the landingspots inside all the controllers (For performance this is not done in a loop)
	void IterateLandingSpots() {
		ls += checkEveryNthLandingSpot;
		currentController = landingSpotControllers[lsc];
		int cc = currentController.transform.childCount;
		if (ls > cc - 1) {
			ls = ls - cc;
			if (lsc < landingSpotControllers.Length - 1)
				lsc++;
			else
				lsc = 0;
		}
	}

	//Checks distance to landingspots in the current landingspot controller
	bool CheckDistanceToLandingSpot(LandingSpotController lc) {		
	Transform lcT = lc.transform;
	Transform lsT = lcT.GetChild(ls);
	LandingSpot lcSpot = lsT.GetComponent<LandingSpot>();
	if(lcSpot.landingChild != null){
			float d = (lsT.position - transform.position).sqrMagnitude;  //Vector3.Distance(lcT.GetChild(ls).position, transform.position) 	
		if(d<distanceToScare*distanceToScare){
			return true;
		}
	}
	return false;	
}

//Creates multiple instances of the Invoke that checks distance to landingspots
void Invoker() {
	for (int i = 0; i < InvokeAmounts; i++){
			float s = (scareInterval / InvokeAmounts)*i;
		Invoke("CheckProximityToLandingSpots", scareInterval + s);
	}
}

//Starts checking for birds to scare when component is enabled
void OnEnable() {
	CancelInvoke("CheckProximityToLandingSpots");
	if (landingSpotControllers.Length > 0)
		Invoker();
#if UNITY_EDITOR
	else
		Debug.Log("Please assign LandingSpotControllers to FlockScare");
#endif
}

//Stops checking for birds to scare when component is disabled
void OnDisable() {
	CancelInvoke("CheckProximityToLandingSpots");
}

#if UNITY_EDITOR
//Shows area of scaryness (Only used in editor)
void OnDrawGizmos() {
	Gizmos.color = Color.red;
	Gizmos.DrawWireSphere(transform.position, distanceToScare);
	if (InvokeAmounts <= 0) InvokeAmounts = 1;
}
#endif

}
