using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// This script was created so you can leave the main camera turned off in the editor for better editor performance.
    /// I noticed the editor was very "stuttery" moving around while a camera that has HDR is enabled in the scene.
    /// </summary>
    public class TurnOnCamera : MonoBehaviour
    {
        public Camera mainCamera;

        public void Awake()
        {
            mainCamera.gameObject.SetActive(true);
        }
    }
}