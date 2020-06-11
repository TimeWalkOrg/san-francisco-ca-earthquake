using UnityEngine;
using System;


public class RandomMaterial:MonoBehaviour{
    public Renderer targetRenderer;
    public Material[] materials;
    
    public void Start() {
    	ChangeMaterial();
    }
    
    public void ChangeMaterial(){
    	targetRenderer.sharedMaterial = materials[UnityEngine.Random.Range(0, materials.Length)];
    }
}