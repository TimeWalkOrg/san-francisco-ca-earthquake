using UnityEngine;
using System.Collections;

//Attach this to a prefab or gameobject that you would like to disable based on distance to another object. Like main camera or player.

public class UnluckDistanceDisabler : MonoBehaviour {
	public int _distanceDisable = 1000;
	public Transform _distanceFrom;
	public bool _distanceFromMainCam;	
	#if UNITY_4_5
	[Tooltip("The amount of time in seconds between checks")]
	#endif
	public float _disableCheckInterval = 10.0f;
	#if UNITY_4_5
	[Tooltip("The amount of time in seconds between checks")]
	#endif
	public float _enableCheckInterval = 1.0f;
	public bool _disableOnStart;
		
	public void Start()
	{
		if (_distanceFromMainCam){
			_distanceFrom = Camera.main.transform;
		}	
		InvokeRepeating("CheckDisable", _disableCheckInterval + (Random.value * _disableCheckInterval), _disableCheckInterval);
		InvokeRepeating("CheckEnable", _enableCheckInterval + (Random.value * _enableCheckInterval), _enableCheckInterval);	
		Invoke("DisableOnStart", 0.01f);
	}
	
	public void DisableOnStart(){
		if (_disableOnStart){
			gameObject.SetActive(false);
		}
	}

	public void CheckDisable(){
		if (gameObject.activeInHierarchy && (transform.position - _distanceFrom.position).sqrMagnitude > _distanceDisable * _distanceDisable){
			gameObject.SetActive(false);			
		}
	}

	public void CheckEnable(){
		if (!gameObject.activeInHierarchy && (transform.position - _distanceFrom.position).sqrMagnitude < _distanceDisable * _distanceDisable){
			gameObject.SetActive(true);	
		}
	}
}