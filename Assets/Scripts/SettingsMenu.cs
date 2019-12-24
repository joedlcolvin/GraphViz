using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
	public GameObject mainMenu;

	public GameObject settings;

	public void OnMainMenu() 
	{
		gameObject.SetActive(false);
		mainMenu.SetActive(true);
	}
}
