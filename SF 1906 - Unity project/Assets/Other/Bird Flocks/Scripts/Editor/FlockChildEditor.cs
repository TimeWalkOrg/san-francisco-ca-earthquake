using UnityEngine;
using System;
using UnityEditor;
/****************************************
	Copyright Unluck Software	
 	www.chemicalbliss.com																															
*****************************************/
[CustomEditor(typeof(FlockChild))]

[System.Serializable]
public class FlockChildEditor: Editor {
    public override void OnInspectorGUI() {
    	var target_cs=(FlockChild)target;
        DrawDefaultInspector();
    	if((target_cs._thisT==null) || (target_cs._model==null) || (target_cs._modelT==null)){
    		EditorGUILayout.LabelField("Find and fill empty variables", EditorStyles.boldLabel); 
			if(GUILayout.Button("Click Me! ")) {
			 	target_cs.FindRequiredComponents();
			}
		}
		if (GUI.changed)	EditorUtility.SetDirty (target_cs);
    }
}