using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class MainMenuScript : MonoBehaviour
{
	public UnityEvent activatedSettingsMenu = new UnityEvent();

	public GameObject controlMenu;
	public GameObject settingsMenu;

	public void OnControlMenu() 
	{
		gameObject.SetActive(false);
		controlMenu.SetActive(true);
	}

	public void OnSettingsMenu() 
	{
		gameObject.SetActive(false);
		settingsMenu.SetActive(true);
		activatedSettingsMenu.Invoke();
	}

	public void StartViz()
	{
		SceneManager.LoadScene("Viz");
	}

	public void OnQuit()
	{
		Application.Quit();
	}
}
