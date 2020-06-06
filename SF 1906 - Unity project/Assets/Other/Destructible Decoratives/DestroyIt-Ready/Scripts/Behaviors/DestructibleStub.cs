/*
Note from ModelShark Studio:
Permission is granted to include this script in your Unity assets for commercial or non-commercial use.
Permission is also granted to modify this script so long as it does not include other code from our DestroyIt product.
The purpose of this permission is to allow you to sell DestroyIt-Ready assets without needing to include DestroyIt code dependencies.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace DestroyItReady
{
    /// <summary>
    /// This script is a propery placeholder for the Destructible script. Use the converters under Windows -> DestroyIt
    /// to convert an object with this script into a functional Destructible object.
    /// </summary>
    public class DestructibleStub : MonoBehaviour
    {
        public int totalHitPoints = 50;
        public int currentHitPoints = 50;
        public List<DamageLevel> damageLevels;
        public GameObject destroyedPrefab;
        public GameObject destroyedPrefabParent;
        public ParticleSystem fallbackParticle;
        public Material fallbackParticleMaterial;
        [FormerlySerializedAs("damageLevelParticles")]
        public List<DamageEffect> damageEffects;
        public float velocityReduction = .5f; 
        public float ignoreCollisionsUnder = 2f;
        public List<GameObject> unparentOnDestroy;
        public bool disableKinematicOnUparentedChildren = true;
        public List<MaterialMapping> replaceMaterials;
        public bool canBeDestroyed = true;
        public bool canBeRepaired = true;
        public bool canBeObliterated = true;
        public List<string> debrisToReParentByName;
        public bool debrisToReParentIsKinematic;
        public List<string> childrenToReParentByName;
        public int destructibleGroupId;
        public bool isDebrisChipAway;
        public float chipAwayDebrisMass = 1f;
        public float chipAwayDebrisDrag;
        public float chipAwayDebrisAngularDrag = 0.05f;
        public bool autoPoolDestroyedPrefab = true;
        public bool useFallbackParticle = true;
        public Vector3 centerPointOverride;
        public bool sinkWhenDestroyed;

        public void OnDrawGizmos()
        {
            if (damageEffects == null) return;
            foreach (DamageEffect effect in damageEffects)
            {
                if (effect == null) continue;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.TransformPoint(effect.Offset), new Vector3(0.1f, 0.1f, 0.1f));
                Quaternion rotatedVector = transform.rotation * Quaternion.Euler(effect.Rotation);
                Gizmos.DrawRay(transform.TransformPoint(effect.Offset), rotatedVector * Vector3.forward * .5f);
            }
        }
    }

    [Serializable]
    public class DamageEffect
    {
        public int TriggeredAt;
        public Vector3 Offset;
        public Vector3 Rotation;
        public GameObject Effect;
        public bool HasStarted;
        public bool UseDependency;
        public Tag TagDependency;
    }

    [Serializable]
    public class DamageLevel
    {
        public int maxHitPoints;
        public int minHitPoints;
        public int healthPercent;
        public bool hasError;
        public int visibleDamageLevel;
    }

    [Serializable]
    public class MaterialMapping
    {
        public Material SourceMaterial;
        public Material ReplacementMaterial;
    }

    public enum Tag
    {
        ClingingDebris = 0,
        ClingPoint = 7,
        Concrete = 1,
        Glass = 2,
        MaterialTransferred = 3,
        Metal = 4,
        Paper = 5,
        Wood = 6,
        Powered = 8,
        Pooled = 9,
        Untagged = 10,
        DestructibleGroup = 11,
        Rubber = 12,
        Stuffing = 13
    }
}