using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// This is a helper script that listens to a Destructible object's RepairedEvent and runs additional code when the object is repaired.
    /// Put this script on a GameObject that has the Destructible script.
    /// </summary>
    [RequireComponent(typeof(Destructible))]
    public class WhenRepaired : MonoBehaviour
    {
        private Destructible destObj;

        private void Start()
        {
            // Try to get the Destructible script on the object. If found, attach the OnRepaired event listener to the RepairedEvent.
            destObj = gameObject.GetComponent<Destructible>();
            if (destObj != null)
                destObj.RepairedEvent += OnRepaired;
        }

        private void OnDisable()
        {
            // Unregister the event listener when disabled/destroyed. Very important to prevent memory leaks due to orphaned event listeners!
            destObj.RepairedEvent -= OnRepaired;
        }

        /// <summary>When the Destructible object is repaired, the code in this method will run.</summary>
        private void OnRepaired()
        {
            Debug.Log(string.Format("{0} was repaired {1} hit points", destObj.name, destObj.LastRepairedAmount));
        }
    }
}