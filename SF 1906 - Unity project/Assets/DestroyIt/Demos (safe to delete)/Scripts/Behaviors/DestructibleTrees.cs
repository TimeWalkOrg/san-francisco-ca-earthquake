using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DestroyIt
{
    public class DestructibleTrees : MonoBehaviour
    {
        /*
        public Terrain terrain;
        private TreeInstance[] _originalTrees;

        void Start()
        {
            terrain = GetComponent<Terrain>();

            // backup original terrain trees
            _originalTrees = terrain.terrainData.treeInstances;

            // create capsule collider for every terrain tree
            for (int i = 0; i < terrain.terrainData.treeInstances.Length; i++)
            {
                TreeInstance treeInstance = terrain.terrainData.treeInstances[i];
                //GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                //CapsuleCollider capsuleCollider = capsule.collider as CapsuleCollider;
                //capsuleCollider.center = new Vector3(0, 5, 0);
                //capsuleCollider.height = 10;
                Destructible tree = treeInstance .AddComponent<Destructible>();
                tree.terrainIndex = i;
                capsule.transform.position = Vector3.Scale(treeInstance.position, terrain.terrainData.size);
                capsule.tag = "Tree";
                capsule.transform.parent = terrain.transform;
                capsule.renderer.enabled = false;

            }
        }
        void OnApplicationQuit()
        {
            // restore original trees
            terrain.terrainData.treeInstances = _originalTrees;
        }
        */

        public void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Collision with " + collision.gameObject.name);
        }
    }
}