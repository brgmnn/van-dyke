using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour {
	
	public float speed = 0.3f; // Start speed
	float initialSpeed = 3.0f; // Speed we hold if not being slown down
	float minSpeed = 1f;
	float maxSpeed = 7f;
	float acceleration = 0.5f;
	float deceleration = 0.5f;
	public BezierMover bm;	
	public float lateralMovementCurrent = 0;
	public float gunChargingSince;
	
	private Vector3 crosshairPosition;
	
	public GameObject creator;
	public GameObject destroyer;
	public GameObject vehicleInner;
	public GameObject laserBeam;
	
	private static bool left_mouse_up = false;
	private static bool left_mouse_down = false;
	private static bool left_mouse_hold = false;
	
	static float lateralMovement = 3f;
	
	public void ChangeSpeed(int TargetSpeed){
		switch (TargetSpeed){
		case 1:
			speed *= 0.8f;
			break;
		case 2:
			speed *= 0.6f;
			break;
		case 3:
			speed *= 0.5f;
			break;
		case 4:
			speed *= 1.4f;
			break;
		case 5:
			speed *= 1.6f;
			break;
		case 6:
			speed *= 1.8f;
			break;
		}
		if (speed < minSpeed){
			speed = minSpeed;
		}else if(speed > maxSpeed){
			speed = maxSpeed;
		}
		
	}
	
	void Start () {
		bm = new BezierMover(transform.gameObject);
		bm.InitialMove();
		laserBeam.renderer.enabled = false;
	}
	
	
	public void InnerTriggerEnter(Collider other) {
    	// Debug.Log("Vehicle triggered " + other.ToString());
		ActionManager.Trigger(other);
	}
	
	public void InnerCollisionEnter(Collision other) {
    	// Debug.Log("Vehicle collided with " + other.ToString());
		ActionManager.Trigger(other.collider);
	}
	
	public void MoveLeft(){
		float moveFormula = Time.deltaTime*speed*2;
		lateralMovementCurrent = vehicleInner.transform.localPosition.x;

		if(lateralMovementCurrent - moveFormula < -lateralMovement) return;
		vehicleInner.transform.Translate(-moveFormula, 0.0f, 0.0f,Space.Self);			
	}

	public void MoveRight(){			
		float moveFormula = Time.deltaTime*speed*2;
		lateralMovementCurrent = vehicleInner.transform.localPosition.x;

		if(lateralMovementCurrent + moveFormula > lateralMovement) return;
		vehicleInner.transform.Translate(moveFormula, 0.0f, 0.0f,Space.Self);
	}
	
	// Kinect movement; allows for proportional movement rather than binary linear movement
	public void Move(float direction){
		direction -= 0.5f; // Range now -0.5 .. +0.5
		float moveFormula = Time.deltaTime*speed*6*direction;
		lateralMovementCurrent = vehicleInner.transform.localPosition.x;
		if(lateralMovementCurrent + moveFormula > lateralMovement || 
		   lateralMovementCurrent + moveFormula < -lateralMovement) return;
		vehicleInner.transform.Translate(moveFormula, 0.0f, 0.0f,Space.Self);

	}
	
	public static void SimulateMouseDown() {
		left_mouse_down = true;
	}
	
	public static void SimulateMouseHold() {
		left_mouse_hold = true;
	}
	
	public static void SimulateMouseUp() {
		left_mouse_up = true;
		left_mouse_hold = false;
	}
	
	private void OnGUI() {
//		Vector3 crosshairPosition;
//		GUI.Label(new Rect(20,20,Screen.width-20,Screen.height-20),"crosshair = "+crosshairPosition);
	}
	
	void Update () {
		bm.speed = speed;
		bm.Move();

		if(speed < initialSpeed){
			speed += (1/speed) * acceleration * (Time.deltaTime);
		}else if(speed > initialSpeed){
			speed -=  (1/speed) * deceleration * (Time.deltaTime);
		}
		if(speed < initialSpeed && speed + 5.0 * Time.deltaTime > initialSpeed){
			speed = initialSpeed;
		}else if(speed > initialSpeed && speed - 3.0 * Time.deltaTime < initialSpeed){
			speed = initialSpeed;
		}
		
		// on mouse click ...
//		Vector3 crosshairPosition;
		if(Game.mouseKeyboardInput){
	    	crosshairPosition = MainUI.boundedCrosshairPosition(Input.mousePosition); 
		} else {
			crosshairPosition = MainUI.boundedCrosshairPosition((Vector3)ActionManager.aiming); 
		}
		Ray ray = Camera.main.ScreenPointToRay(crosshairPosition);
		RaycastHit hit;
		int layerMask;
		if( Input.GetMouseButtonDown(0) || left_mouse_down){
			gunChargingSince = Time.time;
		}
		

		// Ignore all but walls and projectiles (so we don't hit the vehicle)
		layerMask = 1 << 8 | 1 << 9;
        if(Physics.Raycast(ray, out hit, 100.0f,layerMask) && hit.transform != null) { 
			laserBeam.renderer.enabled = true;
			Vector3 start = laserBeam.transform.position, end = hit.point;
			// Set beginning to be where line object is
			laserBeam.GetComponent<LineRenderer>().SetPosition(0,start);
			// Set end of line to aim where we hit
			laserBeam.GetComponent<LineRenderer>().SetPosition(1,end);
        }
		

		layerMask = 1 << 8; // Layer mask for Shootable projectiles
      
        if(Physics.Raycast(ray, out hit, 100.0f, layerMask) && hit.transform != null) { 
			if(hit.transform.name == "Inner"){
				Debug.Log ("Hit inner");
				GameObject.Destroy(hit.transform.parent.gameObject);
			} else {
				Debug.Log ("Hit big");
				GameObject.Destroy(hit.transform.gameObject);
			}
        }
	}
	
//	public void OnGUI() {
//		GUI.Label(new Rect(20,40,Screen.width-20,Screen.height-20),"left_mouse_down = "+left_mouse_down);
//		GUI.Label(new Rect(20,60,Screen.width-20,Screen.height-20),"left_mouse_hold = "+left_mouse_hold);
//		GUI.Label(new Rect(20,80,Screen.width-20,Screen.height-20),"left_mouse_up   = "+left_mouse_up);
//	}

}

