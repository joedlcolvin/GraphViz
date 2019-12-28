using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGameUi : MonoBehaviour
{

	public GameObject inGameMenu;
	public GameObject camera;

	GameObject selector;

	public UnityEvent pause;

	float r;

	void Start()
	{
		UnityEvent pause = new UnityEvent();
		selector = this.transform.Find("Selector").gameObject;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			inGameMenu.SetActive(true);
			pause.Invoke();
		}

		// Rescale selector based on distance from selected node
		r = camera.GetComponent<CameraMovement>().r;
		selector.transform.localScale = Vector3.one*(3/(r));
	}
}
