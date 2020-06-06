using UnityEngine;
using System.Collections;

public class ScrollUV_River : MonoBehaviour 
	{

		public float horizontalScrollSpeed = 0.25f;
		public float verticalScrollSpeed = 0.25f;

		private Renderer _myRenderer;

		private bool scroll = true;

		void Start () 
		{
			_myRenderer = GetComponent<Renderer>();
			if(_myRenderer == null)
				enabled = false;
		}

		public void FixedUpdate()
		{
			if (scroll)
			{
				float verticalOffset = Time.time * verticalScrollSpeed;
				float horizontalOffset = Time.time * horizontalScrollSpeed;
				_myRenderer.material.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
			}
		}

		public void DoActivateTrigger()
		{
			scroll = !scroll;
		}

	}