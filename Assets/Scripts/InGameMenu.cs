using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class InGameMenu : MonoBehaviour
{
	public UnityEvent continue_;
	public UnityEvent stop;

	void Start()
	{
		continue_ = new UnityEvent();
		stop = new UnityEvent();
	}
	
	public void OnReturn()
	{
		continue_.Invoke();
		this.gameObject.SetActive(false);
	}

	public void OnMainMenu()
	{
		stop.Invoke();
		SceneManager.LoadScene("MainMenu");
	}
}
