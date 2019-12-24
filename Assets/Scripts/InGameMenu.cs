using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
	public GameObject net;

	public void OnReturn()
	{
		this.gameObject.SetActive(false);
		net.GetComponent<Net>().paused = false;
	}

	public void OnMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}
}
