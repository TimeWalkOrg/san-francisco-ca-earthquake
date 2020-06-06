using UnityEngine;
using System;
using UnityEditor;
/****************************************
	FlockController Editor	
	Copyright Unluck Software	
 	www.chemicalbliss.com																															
*****************************************/
[CustomEditor(typeof(FlockController))]
[CanEditMultipleObjects]

[System.Serializable]
public class FlockControllerEditor: Editor {
	public SerializedProperty avoidanceMask;	
	
	
	public void OnEnable(){
		var target_cs = (FlockController)target;
        avoidanceMask= serializedObject.FindProperty("_avoidanceMask");
		
		//Fix upgrading older version
		if(target_cs._positionSphereDepth == -1){
			target_cs._positionSphereDepth = target_cs._positionSphere;
		}
		if(target_cs._spawnSphereDepth == -1){
			target_cs._spawnSphereDepth = target_cs._spawnSphere;
		}
	}
	
    public override void OnInspectorGUI() {	
    	var target_cs = (FlockController)target;
        Color warningColor = new Color32((byte)255, (byte)174, (byte)0, (byte)255);
		Color warningColor2 = Color.yellow;
		Color dColor = new Color32((byte)175, (byte)175, (byte)175, (byte)255);
		GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
		warningStyle.normal.textColor = warningColor;
		warningStyle.fontStyle = FontStyle.Bold;
		GUIStyle warningStyle2 = new GUIStyle(GUI.skin.label);
		warningStyle2.normal.textColor = warningColor2;
		warningStyle2.fontStyle = FontStyle.Bold;

    	GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		if(UnityEditor.EditorApplication.isPlaying)
		{
			GUI.enabled = false;
		}
		target_cs._updateDivisor = (int)EditorGUILayout.Slider("Frame Skipping", (float)target_cs._updateDivisor, 1.0f, 10.0f);
		GUI.enabled = true;
		if(target_cs._updateDivisor > 4)
		{
			EditorGUILayout.LabelField("Will cause choppy movement", warningStyle);
		}
		else if(target_cs._updateDivisor > 2)
		{
			EditorGUILayout.LabelField("Can cause choppy movement	", warningStyle2);
		}
		EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
    	target_cs._childPrefab = EditorGUILayout.ObjectField("Bird Prefab", target_cs._childPrefab, typeof(FlockChild),false) as FlockChild;
    	EditorGUILayout.LabelField("Drag & Drop bird prefab from project folder", EditorStyles.miniLabel); 
    	
    	
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
    	
    	EditorGUILayout.LabelField("Roaming Area", EditorStyles.boldLabel);

    	target_cs._positionSphere = EditorGUILayout.FloatField("Roaming Area Width" , target_cs._positionSphere);
    	if(target_cs._positionSphere < 0)
    	target_cs._positionSphere = 0.0f;
    	target_cs._positionSphereDepth = EditorGUILayout.FloatField("Roaming Area Depth" , target_cs._positionSphereDepth);
    	if(target_cs._positionSphereDepth < 0)
    	target_cs._positionSphereDepth = 0.0f;
    	target_cs._positionSphereHeight = EditorGUILayout.FloatField("Roaming Area Height" , target_cs._positionSphereHeight);
    	if(target_cs._positionSphereHeight < 0)
    	target_cs._positionSphereHeight = 0.0f;
///GROUPING
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
    	
    	EditorGUILayout.LabelField("Grouping", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Move birds into a parent transform", EditorStyles.miniLabel);
		target_cs._groupChildToFlock = EditorGUILayout.Toggle("Group to Flock", target_cs._groupChildToFlock);
		if(target_cs._groupChildToFlock)
		{
			GUI.enabled = false;
		}
		target_cs._groupChildToNewTransform = EditorGUILayout.Toggle("Group to New GameObject", target_cs._groupChildToNewTransform);
		target_cs._groupName = EditorGUILayout.TextField("Group Name", target_cs._groupName);
		GUI.enabled = true;
    	
    	
		EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
///FLOCK
    	EditorGUILayout.LabelField("Size of the flock", EditorStyles.boldLabel);
    	
    	target_cs._childAmount = (int)EditorGUILayout.Slider("Bird Amount", (float)target_cs._childAmount, 0.0f,999.0f);
    	target_cs._spawnSphere = EditorGUILayout.FloatField("Flock Width" , target_cs._spawnSphere);
    	if(target_cs._spawnSphere < 1)
    	target_cs._spawnSphere = 1.0f;
    	target_cs._spawnSphereDepth = EditorGUILayout.FloatField("Flock Depth" , target_cs._spawnSphereDepth);
    	if(target_cs._spawnSphereDepth < 1)
    	target_cs._spawnSphereDepth = 1.0f;
    	target_cs._spawnSphereHeight = EditorGUILayout.FloatField("Flock Height" , target_cs._spawnSphereHeight);
    	if(target_cs._spawnSphereHeight < 1)
    	target_cs._spawnSphereHeight = 1.0f;
    	target_cs._startPosOffset = EditorGUILayout.Vector3Field("Start Position Offset", target_cs._startPosOffset);
    	target_cs._slowSpawn = EditorGUILayout.Toggle("Slowly Spawn Birds" , target_cs._slowSpawn);
    	
    	
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
///BEHAVIOR		
    	EditorGUILayout.LabelField("Behaviors and Appearance", EditorStyles.boldLabel); 
    	EditorGUILayout.LabelField("Change how the birds move and behave", EditorStyles.miniLabel);
    	target_cs._minSpeed = EditorGUILayout.FloatField("Birds Min Speed" , target_cs._minSpeed);
    	target_cs._maxSpeed = EditorGUILayout.FloatField("Birds Max Speed" , target_cs._maxSpeed);
    	target_cs._diveValue = EditorGUILayout.FloatField("Birds Dive Depth" , target_cs._diveValue);  	
    	target_cs._diveFrequency = EditorGUILayout.Slider("Birds Dive Chance" , target_cs._diveFrequency, 0.0f, .7f);
    	target_cs._soarFrequency = EditorGUILayout.Slider("Birds Soar Chance" , target_cs._soarFrequency, 0.0f, 1.0f);
    	target_cs._soarMaxTime = EditorGUILayout.FloatField("Soar Time (0 = Always)" , target_cs._soarMaxTime);
    	
    	

		
		
    	target_cs._minDamping = EditorGUILayout.FloatField("Min Damping Turns" , target_cs._minDamping); 	
    	target_cs._maxDamping = EditorGUILayout.FloatField("Max Damping Turns" , target_cs._maxDamping);
    	EditorGUILayout.LabelField("Bigger number = faster turns", EditorStyles.miniLabel);  
    	
    	

    	
    	
    	target_cs._minScale = EditorGUILayout.FloatField("Birds Min Scale" , target_cs._minScale);
    	target_cs._maxScale = EditorGUILayout.FloatField("Birds Max Scale" , target_cs._maxScale);
    	EditorGUILayout.LabelField("Randomize size of birds when added", EditorStyles.miniLabel);
    	
    	
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
    	EditorGUILayout.LabelField("Disable Pitch Rotation", EditorStyles.boldLabel);
    	EditorGUILayout.LabelField("Flattens out rotation when flying or soaring upwards", EditorStyles.miniLabel);   	
    	target_cs._flatSoar = EditorGUILayout.Toggle("Flat Soar" , target_cs._flatSoar);
		target_cs._flatFly = EditorGUILayout.Toggle("Flat Fly" , target_cs._flatFly);
 		
 		
 		EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
    	EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
    	target_cs._soarAnimation = EditorGUILayout.TextField("Soar Animation", target_cs._soarAnimation);
    	target_cs._flapAnimation = EditorGUILayout.TextField("Flap Animation", target_cs._flapAnimation);
    	target_cs._idleAnimation = EditorGUILayout.TextField("Idle Animation", target_cs._idleAnimation);
    	target_cs._minAnimationSpeed = EditorGUILayout.FloatField("Min Anim Speed" , target_cs._minAnimationSpeed);
    	target_cs._maxAnimationSpeed = EditorGUILayout.FloatField("Max Anim Speed" , target_cs._maxAnimationSpeed);  	
		
		
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
    	EditorGUILayout.LabelField("Bird Trigger Flock Waypoint", EditorStyles.boldLabel);
    	EditorGUILayout.LabelField("Birds own waypoit triggers a new flock waypoint", EditorStyles.miniLabel);
    	target_cs._childTriggerPos = EditorGUILayout.Toggle("Bird Trigger Waypoint" , target_cs._childTriggerPos);
    	target_cs._waypointDistance = EditorGUILayout.FloatField("Distance To Waypoint" , target_cs._waypointDistance);
    	
    	
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
		EditorGUILayout.LabelField("Automatic Flock Waypoint", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Automaticly change the flock waypoint (0 = never)", EditorStyles.miniLabel);
		target_cs._randomPositionTimer = EditorGUILayout.FloatField("Auto Waypoint Delay" , target_cs._randomPositionTimer);
		if(target_cs._randomPositionTimer < 0){
			target_cs._randomPositionTimer = 0.0f;
		}
		
		
    	EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
    	EditorGUILayout.LabelField("Force Bird Waypoints", EditorStyles.boldLabel);
    	EditorGUILayout.LabelField("Force all birds to change waypoints when flock changes waypoint", EditorStyles.miniLabel);
		target_cs._forceChildWaypoints = EditorGUILayout.Toggle("Force Bird Waypoints" , target_cs._forceChildWaypoints);
		target_cs._forcedRandomDelay = (float)EditorGUILayout.IntField("Bird Waypoint Delay" , (int)target_cs._forcedRandomDelay);
		
		
		EditorGUILayout.EndVertical();
		GUI.color = dColor;
		EditorGUILayout.BeginVertical("Box");
		GUI.color = Color.white;
		
		
		EditorGUILayout.LabelField("Avoidance", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Birds will steer away from colliders (Ray)", EditorStyles.miniLabel);
		target_cs._birdAvoid = EditorGUILayout.Toggle("Bird Avoid" , target_cs._birdAvoid);
		if(target_cs._birdAvoid){
			EditorGUILayout.PropertyField(avoidanceMask, new GUIContent("Collider Mask"));
			
			target_cs._birdAvoidHorizontalForce = (int)EditorGUILayout.FloatField("Avoid Horizontal Force" , (float)target_cs._birdAvoidHorizontalForce);
						
			float minVal = target_cs._birdAvoidDistanceMin;
			float minLimit = .5f;
			float maxVal = target_cs._birdAvoidDistanceMax;
			float maxLimit = 8.0f;
			
			EditorGUILayout.LabelField("Min Avoid Distance:", minVal.ToString());
			EditorGUILayout.LabelField("Max Avoid Distance:", maxVal.ToString());
			if(System.Single.IsNaN(minVal)) minVal = minLimit;
			if(System.Single.IsNaN(maxVal)) maxVal = minLimit;
			EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
			
			target_cs._birdAvoidDistanceMin = minVal;
			target_cs._birdAvoidDistanceMax = maxVal;
			
			target_cs._birdAvoidDown = EditorGUILayout.Toggle("Bird Avoid Up" , target_cs._birdAvoidDown);
			target_cs._birdAvoidUp = EditorGUILayout.Toggle("Bird Avoid Down" , target_cs._birdAvoidUp);
			if(target_cs._birdAvoidDown || target_cs._birdAvoidUp)
				target_cs._birdAvoidVerticalForce = (int)EditorGUILayout.FloatField("Avoid Vertical Force" , (float)target_cs._birdAvoidVerticalForce);
		}
		EditorGUILayout.EndVertical();

		if(target_cs._forcedRandomDelay < 0){
			target_cs._forcedRandomDelay = 0.0f;
		}	
        if (GUI.changed)
            EditorUtility.SetDirty (target_cs);
    }
}