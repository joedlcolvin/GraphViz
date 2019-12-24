using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsMenu : MonoBehaviour
{
	public GameObject MainMenu;

	public void OnMainMenu() 
	{
		gameObject.SetActive(false);
		MainMenu.SetActive(true);
	}
}
