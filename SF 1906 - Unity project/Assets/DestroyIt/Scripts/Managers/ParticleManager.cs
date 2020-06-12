using System;
using System.Linq;
using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// Particle Manager (Singleton) - manages the playing of particle effects and handles performance throttling.
    /// Call the PlayEffect() method, and this script decides whether to play the effect based on how many are currently active.
    /// </summary>
    [DisallowMultipleComponent]
    public class ParticleManager : MonoBehaviour
    {
        public int maxDestroyedParticles = 20; // max particles to allow during [withinSeconds].
        public int maxPerDestructible = 5;  // max particles to allow for a single Destructible object or DestructibleGroup.
        public float withinSeconds = 4f;    // remove particles from the managed list after this many seconds.
        public float updateFrequency = .5f; // The time (in seconds) this script updates its counters

        public static ParticleManager Instance { get; private set; }
        public ActiveParticle[] ActiveParticles 
        { 
            get { return activeParticles; } 
            private set { activeParticles = value;} 
        }
        public bool IsMaxActiveParticles
        {
            get { return ActiveParticles.Length >= maxDestroyedParticles; }
        }

        private float nextUpdate;
        private ActiveParticle[] activeParticles;
        private ParticleManager() { } // hide constructor

        // Events
        public event Action ActiveParticlesCounterChangedEvent;

        public void Awake()
        {
            ActiveParticles = new ActiveParticle[0];
            Instance = this;
            nextUpdate = Time.time + updateFrequency;
        }

        public void Update()
        {
            if (!(Time.time > nextUpdate)) return;
            if (activeParticles.Length == 0) return;

            int removeIndicesCounter = 0;
            int[] removeIndices = new int[0];
            bool isChanged = false;
            for (int i = 0; i < ActiveParticles.Length;i++ )
            {
                if (Time.time >= ActiveParticles[i].InstantiatedTime + withinSeconds)
                {
                    isChanged = true;
                    removeIndicesCounter++;
                    Array.Resize(ref removeIndices, removeIndicesCounter);
                    removeIndices[removeIndicesCounter - 1] = i;
                }
            }
            activeParticles = activeParticles.RemoveAllAt(removeIndices);
            if (isChanged)
                FireActiveParticlesCounterChangedEvent();

            // Reset the nextUpdate counter.
            nextUpdate = Time.time + updateFrequency; 
        }

        /// <summary>Plays a particle effect and adjusts its texture to have maximum damge level progressive damage (if enabled).</summary>
        public void PlayEffect(ParticleSystem particle, Destructible destObj, Vector3 pos, Quaternion rot, int parentId, bool transferSourceMat)
        {
            if (particle == null) return;

            // Check if we're at the maximum active particle limit. If so, ignore the request to play the particle effect.
            if (IsMaxActiveParticles) return;

            // Check if we've reached the max particle limit per destructible object for this object already.
            int parentParticleCount = ActiveParticles.Count(x => x.ParentId == parentId);
            if (parentParticleCount > maxPerDestructible) return;

            // Instantiate and add to the ActiveParticles counter
            GameObject spawn = ObjectPool.Instance.Spawn(particle.gameObject, pos, rot);
            if (spawn == null || spawn.GetComponent<ParticleSystem>() == null) return;
            ActiveParticle aParticle = new ActiveParticle() { GameObject = spawn, InstantiatedTime = Time.time, ParentId = parentId };
            Array.Resize(ref activeParticles, activeParticles.Length + 1);
            ActiveParticles[activeParticles.Length - 1] = aParticle;
            FireActiveParticlesCounterChangedEvent();
            
            // Replace the materials on the particle effect with the one passed in.
            if (!transferSourceMat) return;
            if (spawn.GetComponent<ParticleSystem>() == null) return;

            foreach (ParticleSystemRenderer psr in spawn.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                if (psr.renderMode != ParticleSystemRenderMode.Mesh) continue;

                if (destObj.fallbackParticleMaterial != null)
                    psr.material = destObj.fallbackParticleMaterial;
                else
                {
                    // NOTE: Due to a Unity issue, MaterialPropertyBlocks do not work with ParticleSystemRenderers when the RenderMode is set to Mesh.
                    // The code below should work, but it doesn't, so we have to comment it out for now and hope this eventually gets fixed in Unity.
                    // In the meantime, we have to do it the inefficient way and assign the material directly on the ParticleSystemRenderer.
                    //DestructionManager.Instance.SetProgressiveDamageTexture(psr, destObj.GetDestroyedParticleEffectMaterial(), destObj.damageLevels[destObj.damageLevels.Count - 1]);

                    //TODO: Remove this code and use the above code instead, if the MaterialPropertyBlock bug ever gets fixed in Unity.
                    psr.material = destObj.GetDestroyedParticleEffectMaterial();
                    Texture2D detailMask = DestructionManager.Instance.GetDetailMask(psr.sharedMaterial, destObj.damageLevels[destObj.damageLevels.Count - 1]);
                    psr.material.SetTexture("_DetailMask", detailMask);
                }
            }
        }

        /// <summary>Fires when the number of Active Particles changes.</summary>
        public void FireActiveParticlesCounterChangedEvent()
        {
            if (ActiveParticlesCounterChangedEvent != null) // first, make sure there is at least one listener.
                ActiveParticlesCounterChangedEvent(); // if so, trigger the event.
        }
    }
}