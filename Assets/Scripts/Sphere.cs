using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sphere : MonoBehaviour
{
//	void OnMouseDown(){
//		GameObject clickedNodes = GameObject.Find("ClickedNodes");
//		transform.SetParent(clickedNodes.transform);
//	}

	public GameObject SelectedSphere;


	void Start()
	{
		SelectedSphere = GameObject.Find("Selected Sphere");
	}

	void Update()
	{
		GameObject parent = transform.parent.gameObject;
		if (parent.name == "Selected Sphere")
		{
			
		}
	}

	void OnMouseOver()
	{
		if(Input.GetMouseButtonDown(0))
		{
			print("Clicked");
			transform.parent = SelectedSphere.transform;
		} 
//		if (Input.GetMouseButtonDown(1))
//		{
//			GameObject clickedNodes = GameObject.Find("RightClickedNodes");
//			transform.SetParent(clickedNodes.transform);
//		}
	}
}
