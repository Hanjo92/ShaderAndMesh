using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
	private const float MinFOV = 20f;
	private const float MaxFOV = 150f;

    [SerializeField] private Camera targetCamera;
	[Header("Camera Option")]
	[SerializeField] private float moveSpeed = 20;
	[SerializeField] private float rotateSpeed = 50;

	[Header("Zoom")]
	[SerializeField] private float FOVSpeed = 300;

	private void Awake()
	{
		targetCamera ??= Camera.main;
	}

	private void Update()
	{
		if(targetCamera == null)
			return;
		if(Utility.CheckMouseOnUI())
			return;

		/// Rotate
		if(Input.GetMouseButton(0))
		{
			var x = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
			var y = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
			var euler = transform.eulerAngles;
			euler.x = Utility.ConvertAngleTo180Range(euler.x) + y;
			euler.x = Mathf.Clamp(euler.x, -90f, 90f);
			euler.y += x;
			euler.z = 0;
			transform.eulerAngles = euler;
		}

		var horizontal = Input.GetAxis("Horizontal");
		var vertical = Input.GetAxis("Vertical");
		var moveVector = new Vector3(horizontal, 0f, vertical) * moveSpeed * Time.deltaTime;
		if(moveVector.sqrMagnitude > 0)
		{
			transform.Translate(moveVector);
		}

		var scroll = Input.GetAxis("Mouse ScrollWheel") * FOVSpeed * Time.deltaTime;
		if(scroll != 0f)
		{
			var targetFOV = Mathf.Clamp( targetCamera.fieldOfView + scroll, MinFOV, MaxFOV);
			targetCamera.fieldOfView = targetFOV;
		}
	}
}
