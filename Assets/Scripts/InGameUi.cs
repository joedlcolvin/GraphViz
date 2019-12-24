using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUi : MonoBehaviour
{

	public GameObject inGameMenu;
	public GameObject net;
	public GameObject camera;

	GameObject selector;

	float r;

	void Start()
	{
		selector = this.transform.Find("Selector").gameObject;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			inGameMenu.SetActive(true);
			net.GetComponent<Net>().paused = true;
		}
		r = camera.GetComponent<CameraMovement>().r;
		selector.transform.localScale = Vector3.one*(3/(r));
	}
}
