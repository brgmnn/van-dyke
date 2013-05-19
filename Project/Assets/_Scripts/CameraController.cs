using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	float dampTime = 5f;
	Transform target;
	public GameObject targetPoint;

	// Use this for initialization
	void Start () {
		transform.position = targetPoint.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(transform.position, targetPoint.transform.position, dampTime * Time.deltaTime);
		transform.rotation = targetPoint.transform.rotation;
	}
}
