using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoNavigation : MonoBehaviour 
{
	public void Start()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
	
	public void LoadMainScenariosDemoScene()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		SceneManager.LoadScene("Main Scenarios Scene");
	}

	public void LoadSUVShowcaseDemoScene()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		SceneManager.LoadScene("SUV Showcase Scene");
	}
}
