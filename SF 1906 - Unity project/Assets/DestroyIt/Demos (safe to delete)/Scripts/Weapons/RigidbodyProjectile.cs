using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// Attach this to any rigidbody object that acts as a projectile and may collide with 
    /// Destructible objects. This script will play particle effects when the object hits 
    /// something, and will do damage to Destructible objects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyProjectile : MonoBehaviour
    {
        public HitBy weaponType = HitBy.Cannonball;
        [Tooltip("Impact velocity must be at least this amount to be detected as a hit.")]
        public float minHitVelocity = 10f;

        private Rigidbody rbody;
        private Vector3 lastVelocity;

        public void OnEnable()
        {
            rbody = GetComponent<Rigidbody>();
        }

        public void FixedUpdate()
        {
            lastVelocity = rbody.velocity;
        }

        public void OnCollisionEnter(Collision collision)
        {
            // Check that the impact is forceful enough to cause damage
            if (collision.relativeVelocity.magnitude < minHitVelocity) return;

            if (collision.contacts.Length == 0) return;

            Collider other = collision.contacts[0].otherCollider;

            // Play hit effects
            HitEffects hitEffects = other.gameObject.GetComponentInParent<HitEffects>();
            if (hitEffects != null && hitEffects.effects.Count > 0)
                hitEffects.PlayEffect(weaponType, collision.contacts[0].point, collision.contacts[0].normal);
            
            // Apply impact damage to Destructible objects without rigidbodies
            Destructible destructibleObj = other.gameObject.GetComponentInParent<Destructible>();
            if (destructibleObj != null)
            {
                if (other.attachedRigidbody == null || other.attachedRigidbody.GetComponent<Destructible>() == null)
                {
                    if (collision.relativeVelocity.magnitude >= destructibleObj.ignoreCollisionsUnder)
                    {
                        destructibleObj.ProcessDestructibleCollision(collision, gameObject.GetComponent<Rigidbody>());
                        rbody.velocity = lastVelocity;
                    }
                }
            }

            // Check for Chip-Away Debris
            ChipAwayDebris chipAwayDebris = collision.contacts[0].otherCollider.gameObject.GetComponent<ChipAwayDebris>();
            if (chipAwayDebris != null) 
                chipAwayDebris.BreakOff(collision.relativeVelocity * -1, collision.contacts[0].point);
        }
    }
}