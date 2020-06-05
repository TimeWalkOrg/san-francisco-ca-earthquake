using UnityEngine;
using System.Collections;

public class THB_nervous_man : MonoBehaviour {
	
	private Animator animator;
	
	void Awake () {
		animator = GetComponent <Animator>();
	}
	void Update () {
		//		key presses change behavior
//		if (Input.GetKeyDown(KeyCode.Alpha1)) animator.SetFloat ("Speed", 2.0f); // press "1" to make him walk
//		if (Input.GetKeyDown(KeyCode.Alpha2)) animator.SetFloat ("Speed", 0.0f); // press "2" to make him stop
//		if (Input.GetKeyDown(KeyCode.Alpha3)) animator.SetFloat ("Direction", 180.0f); // press "3" to change direction
//		if (Input.GetKeyDown(KeyCode.Alpha4)) animator.SetFloat ("Direction", 0.0f); // press "4" to change direction back
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.gameObject.tag == "Player") {
			//			animator.SetBool ("shootNow", true);
			animator.SetFloat ("Direction", 15.0f);
			//			animator.SetFloat ("AngularSpeed", 5.0f);
		}
	}
	
	void OnTriggerExit (Collider other) {
		if (other.gameObject.tag == "Player") {
			//			animator.SetBool ("shootNow", false);
			animator.SetFloat ("Speed", 0.0f);
			//			animator.SetFloat ("AngularSpeed", 0.0f);
		}
	}
}