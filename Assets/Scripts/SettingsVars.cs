using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using MyClasses;

public class SettingsVars : MonoBehaviour
{
	bool original = false;

	GameObject mainMenu;

	public Settings settings;

	void Start()	
	{
		// Search for old settings
		if (GameObject.FindGameObjectsWithTag("Settings").Length > 1 && !original)
		{
			Destroy(this.gameObject);
		}
		else
		{
			// Create defualt settings object
			settings = new Settings();

			// Subescribe OnSceneLoaded
			SceneManager.sceneLoaded += OnSceneLoaded;

			// Call OnSceneLoaded
			OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		}
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenu")
		{
			mainMenu = GameObject.Find("Main Menu");
			original = true;
			DontDestroyOnLoad(this.gameObject);
			mainMenu.GetComponent<MainMenuScript>().activatedSettingsMenu.AddListener(OnActiveSettingsMenu);
		}
	}

	public void OnActiveSettingsMenu()
	{
		foreach(FieldInfo fi in settings.GetType().GetFields())
		{
			Slider slider = GameObject.Find(fi.Name).GetComponent<Slider>();
			Text text = GameObject.Find(fi.Name + "ValueText").GetComponent<Text>();
			slider.value = (float) fi.GetValue(settings);
			text.text = fi.GetValue(settings).ToString();
			slider.onValueChanged.AddListener(delegate {SetSetting(fi, slider, text);});
		}
	}

	public void SetSetting(FieldInfo fi, Slider slider, Text text)
	{
		fi.SetValue(settings, Convert.ChangeType(slider.value, fi.FieldType));
		text.text = fi.GetValue(settings).ToString();
	}
}
