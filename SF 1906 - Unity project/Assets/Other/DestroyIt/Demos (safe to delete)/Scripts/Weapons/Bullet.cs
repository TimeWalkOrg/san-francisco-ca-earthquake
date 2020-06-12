using UnityEngine;

namespace DestroyIt
{
    public class Bullet : MonoBehaviour
    {
        [Tooltip("The bullet's speed in game units per second.")]
        public float speed = 400f;
        [Tooltip("How many seconds the bullet will live, regardless of distance traveled.")]
        public float timeToLive = 0.5f;

        public Renderer streak;
        [Range(1,10)]
        [Tooltip("How often the bullet streak is visibile. 1 = 10% of the time. 10 = 100% of the time.")]
        public int streakVisibleFreq = 6;
        [Range(1,50)]
        [Tooltip("Once turned on or off, the bullet streak will remain stable (unchanged) for this many frames.")]
        public int streakMinFramesStable = 3;

        /// <summary>The position where the bullet started.</summary>
        public Vector3 StartingPosition { get; set; }

        /// <summary>How far in game units the bullet has traveled after being fired.</summary>
        public float DistanceTraveled
        {
            get { return Vector3.Distance(StartingPosition, transform.position); }
        }

        private float spawnTime = 0f;
        private bool hitSomething = false;
        private bool isInitialized = false;
        private int streakFramesStable = 0;

        public void OnEnable()
        {
            spawnTime = Time.time;
            hitSomething = false;
            StartingPosition = transform.position;

            if (streak != null)
                streak.gameObject.SetActive(Random.Range(1, 11) <= streakVisibleFreq);

            isInitialized = true;
        }

        public void Update()
        {
            if (!isInitialized) return;

            // Check if the bullet needs to be destroyed.
            if (Time.time > spawnTime + timeToLive || hitSomething)
            {
                ObjectPool.Instance.PoolObject(gameObject);
                return;
            }

            if (streak != null)
            {
                if (streakFramesStable > streakMinFramesStable)
                {
                    streak.gameObject.SetActive(Random.Range(1, 11) <= streakVisibleFreq);
                    streakFramesStable = 0;
                }
                else
                    streakFramesStable += 1;
            }

            Vector3 lineEndPoint = transform.position + (transform.forward * speed * Time.deltaTime);
            Debug.DrawLine(transform.position, lineEndPoint, Color.red, 5f);

            // Raycast in front of the bullet to see if it hit anything. Sort the hits from closest to farthest.
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, speed * Time.deltaTime);
            int hitIndex = -1; // index of the closest hit that is not a trigger collider
            float closestHitDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.isTrigger) continue;
                if (hits[i].distance < closestHitDistance)
                {
                    hitIndex = i;
                    closestHitDistance = hits[i].distance;
                }
            }

            if (hitIndex > -1)
            {
                InputManager.Instance.ProcessBulletHit(hits[hitIndex], transform.forward);
                hitSomething = true;
                return;
            }

            // Move the bullet forward.
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}