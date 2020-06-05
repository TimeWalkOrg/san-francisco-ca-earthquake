using UnityEngine;
using System.Collections;

public class THB_DoorActivator : MonoBehaviour {
	
	public Animator[] lights; 
	
	private Animator animator;
	
	void Awake () {
		animator = GetComponent <Animator>();
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy") {
			animator.SetBool ("DoorOpen", true);
			foreach (var light in lights) {
				light.SetTrigger ("Activate");
			}
		}
	}
	
	void OnTriggerExit (Collider other) {
		if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy") {
			animator.SetBool ("DoorOpen", false);
		}
	}
}