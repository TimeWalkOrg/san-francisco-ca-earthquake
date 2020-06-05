using UnityEngine;
using System.Collections;

public class THB_navigating_man : MonoBehaviour {
	
//	public Animator[] lights; 
	
	private Animator animator;
//	private float WalkDirection = 0.0f;
	private float WalkSpeed = 0.3f;
	
	void Awake () {
		animator = GetComponent <Animator>();
		animator.SetFloat ("Speed", WalkSpeed);
	}
	void Update () {
		//		put something here about key presses as with the dog animation
		//		if (Input.GetKeyDown(KeyCode.Alpha1)) animator.SetFloat ("Speed", WalkSpeed); // press "1" to make him walk
		//		if (Input.GetKeyDown(KeyCode.Alpha2)) animator.SetFloat ("Speed", 0.0f); // press "2" to make him stop
		//		if (Input.GetKeyDown(KeyCode.Alpha3)) animator.SetFloat ("Direction", 180.0f); // press "3" to change direction
		//		if (Input.GetKeyDown(KeyCode.Alpha4)) animator.SetFloat ("Direction", 0.0f); // press "4" to change direction back
	}

}