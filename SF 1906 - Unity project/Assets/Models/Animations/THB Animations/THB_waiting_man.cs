using UnityEngine;
using System.Collections;

public class THB_waiting_man : MonoBehaviour {

	private Animator animator;

//make these "private" after debugging
	public float walkdirection = 0.0f; 
	public float walkspeed;
	public float walkingangle_y;
	public bool turningaround;
	public float smooth; // defines speed of turning around (methinks)
	public Quaternion target;
	
	void Awake () {
		animator = GetComponent <Animator>();
		animator.SetFloat ("Speed", walkspeed);
		animator.SetFloat ("Direction", walkdirection);
//		Debug.Log("Quaternian is " + Quaternion.identity);
	}

	void Update () {
		if (turningaround) {
			//	Rotate him around
			target = Quaternion.Euler(0, walkingangle_y, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);	
//			Debug.Log("transform.rotation = " + transform.rotation + ", and target = " + target);
			if (transform.rotation == target) {	
				// when we finish turning around, start walking again
//				Debug.Log("Done turning to " + target);
				turningaround = false;

			}
		}
	}
	
	void OnTriggerEnter (Collider other) {

		if (other.gameObject.tag == "TurnAround") {
			turningaround = false; // just entered
//			Debug.Log ("I am in ENTER and other.gameObject.tag is " + other.gameObject.tag);
			if(!turningaround){
				animator.SetFloat ("Speed", 0.0f);
				if(walkingangle_y == 324.0f){
					walkingangle_y = 144.0f;
//					Debug.Log("changed walkingangle_y to " + walkingangle_y);
				} else {
					walkingangle_y = 324.0f;
//					Debug.Log("changed walkingangle_y to " + walkingangle_y);
				}
//				Debug.Log("walkingangle_y is " + walkingangle_y + ", and turningaround is " + turningaround);
				turningaround = true;				
				animator.SetFloat ("Speed", walkspeed);
			}
		}
	}
	
	void OnTriggerExit (Collider other) {
//				turningaround = false;
				if (other.gameObject.tag == "TurnAround") {
//						Debug.Log ("I am in EXIT and other.gameObject.tag is " + other.gameObject.tag);
						if (transform.rotation == target) {	
								// when we finish turning around, start walking again
//								Debug.Log ("in exit, done turning to " + target);
								turningaround = false;
						}
				}
		}
}