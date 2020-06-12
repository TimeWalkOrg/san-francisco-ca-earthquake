using UnityEngine;

namespace DestroyIt
{
    public class ShockWall : MonoBehaviour
    {
        public float blastForce = 200f;
        public float damageAmount = 200f;
        public Vector3 origin;

        private void OnTriggerEnter(Collider col)
        {
            Destructible destObj = col.gameObject.GetComponentInParent<Destructible>();

            // If it has a rigidbody, apply force
            Rigidbody rbody = col.attachedRigidbody;
            if (rbody != null && !rbody.isKinematic)
                rbody.AddExplosionForce(blastForce, origin, 0f, 0.5f);

            // Check for Chip-Away Debris
            ChipAwayDebris chipAwayDebris = col.gameObject.GetComponent<ChipAwayDebris>();
            if (chipAwayDebris != null)
            {
                if (Random.Range(1, 100) > 50) // Do this about half the time...
                {
                    chipAwayDebris.BreakOff(blastForce, 0f, 0.5f);
                    return; //Skip the destructible check if the debris hasn't chipped away yet.
                }

                return;
            }

            // If it's a destructible object, apply damage
            if (destObj != null)
            {
                destObj.ApplyDamage(new ExplosiveDamage()
                {
                    DamageAmount = damageAmount,
                    BlastForce = blastForce,
                    Position = origin,
                    Radius = 0f,
                    UpwardModifier = 0.5f
                });
            }
        }
    }
}