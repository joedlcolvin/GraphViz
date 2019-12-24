using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

	public GameObject SelectedSphere;
	public GameObject Network;
	public GameObject Center;

	public float rotSpeed = 5f;
	public float zoomSpeed = 7f;

	float minDistance = 2f;

	Transform focus;

	public float r;

	int nSelected;

	void Start()
	{
		r = transform.position.magnitude;
		focus = Center.transform;
		transform.rotation = updateRotation(transform, focus);
	}

	void Update()
	{
		int n = SelectedSphere.transform.childCount;	
		if (Input.GetKeyDown(KeyCode.Space))
		{
			focus = Center.transform;
			r = updateR(transform, focus);
			for(int i=0;i<n;i++)
			{
				SelectedSphere.transform.GetChild(0).SetParent(Network.transform);
			}
		} 
		else if(n>1)
		{
			for(int i=0;i<n-1;i++)
			{
				SelectedSphere.transform.GetChild(0).SetParent(Network.transform);
			}
			Transform sphereTransform = SelectedSphere.transform.GetChild(0);
			focus = sphereTransform;
			r = updateR(transform, focus);
		}
		else if (n==1 && nSelected ==0)
		{
			Transform sphereTransform = SelectedSphere.transform.GetChild(0);
			focus = sphereTransform;
			r = updateR(transform, focus);
		}
		nSelected = n;
		transform.position = updatePosition(transform, focus);
		transform.rotation = updateRotation(transform, focus);
	}

	float updateR(Transform cam, Transform focus)
	{
		Vector3 relPos = cam.position - focus.position;
		return relPos.magnitude;
	}

	Vector3 updatePosition(Transform cam, Transform focus)
	{
		// Get input direction
		Vector3 input = inputVector();
		Vector3 direction = input.normalized;

		// Update position tangent to sphere
		transform.Translate(direction*rotSpeed*Time.deltaTime*Mathf.Pow(1f,1f));

		// Zoom
		if (Input.GetKey(KeyCode.E))
		{
			r = Mathf.Max(r - zoomSpeed*Time.deltaTime, minDistance);
		}
		if (Input.GetKey(KeyCode.Q))
		{
			r = Mathf.Max(r + zoomSpeed*Time.deltaTime, minDistance);
		}

		// Normalize relative position to focus so as to stay on sphere
		Vector3 newRelPos = cam.position - focus.position;
		newRelPos = newRelPos.normalized*r;
		return focus.position + newRelPos;
	}

	Quaternion updateRotation(Transform cam, Transform focus)
	{
		Vector3 relPos = focus.position - cam.position;
		Quaternion lookRot = Quaternion.LookRotation(relPos);
		return lookRot;
	}

	Vector3 inputVector()
	{
		Vector3 input = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			input.y += 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			input.y -= 1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			input.x += 1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			input.x -= 1f;
		}
		return input;
	}
}
