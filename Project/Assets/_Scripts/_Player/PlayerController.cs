using UnityEngine;
using System.Collections;

/*
 * PlayerController
 * 
 * Base class to be extended to better define individual player roles.
 * 
 */

public class PlayerController : MonoBehaviour {

	public int state;
	public int position;
	public Camera camera;
	
	public enum roles {
		creator = 0,
		destroyer,
	};

	public enum positions {
		front = 0,
		back,
	};
	

	public static float lateralMovement = 6f;
	public static int challengesDefeatedStreak = 0;
	
	public static bool gameOver = false;
	
	private bool chargingChange = false;
	private bool chargingGun = false;
	private float chargingSmoothnessFactor = 0.1f;
	private float lastChargingUpdate = 0.0f;
	
	Transform powerIndicator;
	
	// Usable abilities right now
	// Index determined by ActionManager.abilities
	public static bool[] usableAbilities = {false, false, false, false, false, false, false, false};
	
	public float SteerVehicle(){
		// Lateral movement		
		if(Input.GetKey(KeyCode.Comma)){
			Game.vehicle.GetComponent<Vehicle>().MoveLeft();
		} else if(Input.GetKey(KeyCode.Period)){
			Game.vehicle.GetComponent<Vehicle>().MoveRight();
		} else if(!Game.mouseKeyboardInput){
			Game.vehicle.GetComponent<Vehicle>().Move(ActionManager.steering.x);
		}
		return 0.0f;
	}
	
	public void CompletedChallenge(){
		challengesDefeatedStreak++;
	}
	
	public void FailedChallenge(){
		challengesDefeatedStreak = 0;
	}
	

	public bool IsDriver(){
		if(this.position == (int)positions.front) return true;
		else return false;
	}
	
	public bool IsGunner(){
		if(this.position == (int)positions.back) return true;
		else return false;
	}
	
	public bool IsCreator(){
		if(this.state == (int)roles.creator) return true;
		else return false;
	}
	
	public bool IsDestroyer(){
		if(this.state == (int)roles.destroyer) return true;
		else return false;
	}
	
	public static void SwitchPosition(){		
		PlayerController tempPlayer = Game.frontPlayer;
		Game.frontPlayer = Game.backPlayer;
		Game.backPlayer = tempPlayer;
		
		// Fire appropriate animation
		if(tempPlayer.GetComponent<PlayerController>().IsCreator()){
			Game.vehicleInner.animation.Play("spin1");	
		} else {
			Game.vehicleInner.animation.Play("spin2");
		}
		
		Game.frontPlayer.position = (int)positions.front;
		Game.backPlayer.position = (int)positions.back;
		
		// Switch rear player camera so beam fires for correct screen
		Game.frontPlayer.camera.tag = null;
		Game.backPlayer.camera.tag = "MainCamera";
		
	}
	
	// Called when the player loses
	static void Die(){
		gameOver = true;
	}
	
	void OnGUI(){
		if(gameOver){
			GUIStyle labelStyle = new GUIStyle();
			labelStyle.fontSize = 100;
			labelStyle.alignment = TextAnchor.MiddleCenter;
			//right
			GUI.Label(new Rect(Screen.width * 3/4,Screen.height/2, 2, 2), "Game Over!",labelStyle);
			//left
			GUI.Label(new Rect(Screen.width/4,Screen.height/2, 0.1f, 0.1f), "Game Over!",labelStyle);

			
			if(GUI.Button(new Rect(Screen.width* 3/4 - 50, Screen.height/2 + 50,100,50), "Restart")) {
				gameOver = false;
				gameOver = false;
				Time.timeScale = 1;
				Application.LoadLevel("StartScreen");
				
							SectionManager.Restart();
			
			
			}	
		}
	}

	// Use this for initialization
	void Start () {
		if(IsDriver()){
			Game.frontPlayer = this;
		} else {
			Game.backPlayer = this;
		}
		if(IsCreator()){
			Game.creatorPlayer = this;
		} else {
			Game.destroyerPlayer = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(IsDriver()){
			SteerVehicle();
		}
		
		// To be replaced by Kinect input
		int action = 0;
		ActionManager.Perform(this,action);
		
		// Fallback keyboard input
		KeyPresses();
		
		if (IsGunner()) {
			if (lastChargingUpdate < Time.fixedTime - chargingSmoothnessFactor) {
				if (chargingGun == true)
					chargingChange = true;
				
				chargingGun = false;
			}
			
			if (chargingGun)
				Vehicle.SimulateMouseHold();
				
			if (chargingChange == true && chargingGun == true) {
				print ("simulate mouse down!");
				Vehicle.SimulateMouseDown();
				MainUI.SimulateMouseDown();
			} else if (chargingChange == true && chargingGun == false) {
				print ("simulate mouse up!");
				Vehicle.SimulateMouseUp();
				MainUI.SimulateMouseUp();
			}
			chargingChange = false;
		}
			
			
		if(gameOver){
			float newTime = Time.timeScale - (Time.deltaTime/3);
			if(newTime > 0.05){
				Time.timeScale = Time.timeScale - (Time.deltaTime/3);
				Game.vehicle.GetComponent<Vehicle>().speed -= (Time.deltaTime/3);
			} else {
			 	Time.timeScale = 0;
				Game.vehicle.GetComponent<Vehicle>().speed = 0;
			}
		}
	}
	
	// Use Q W E R for four P1 action handles
	// Use U I O P for four P2 action handles
	// Use G for swapping player positions
	void KeyPresses() {
		if(this.state == (int)roles.creator){
			if(		  Input.GetKeyDown(KeyCode.Q)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handLeft);
			} else if(Input.GetKeyDown(KeyCode.W)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handUp);
			} else if(Input.GetKeyDown(KeyCode.E)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handRight);
			} else if(Input.GetKeyDown(KeyCode.R)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handDown);
			}
			
			if(		  Input.GetKeyDown(KeyCode.G)){
				ActionManager.Perform(this,(int)ActionManager.actions.switchPlaces);	
			}
		} else if(this.state == (int)roles.destroyer){
			if(		  Input.GetKeyDown(KeyCode.U)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handLeft);
			} else if(Input.GetKeyDown(KeyCode.I)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handUp);
			} else if(Input.GetKeyDown(KeyCode.O)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handRight);
			} else if(Input.GetKeyDown(KeyCode.P)) {
				ActionManager.Perform(this,(int)ActionManager.actions.handDown);
			}
			if(		  Input.GetKeyDown(KeyCode.H)){
				ActionManager.Perform(this,(int)ActionManager.actions.switchPlaces);	
			}
		}
	}
	
	// leave this in for the kinect please
	public void PerformAction(int action) {
		if (action == (int)ActionManager.actions.superShoot) {
			if (chargingGun == false)
				chargingChange = true;
			
			chargingGun = true;
			lastChargingUpdate = Time.fixedTime;

//			RearSuperShoot(new Vector3(ActionManager.aiming.x, ActionManager.aiming.y, 0.0f));
		} else
			ActionManager.Perform(this, action);
	}
	
	public void UpdateAimSteer(float x, float y) {
		if (!IsDriver()) {
			ActionManager.aiming.x = x;
			ActionManager.aiming.y = y;
		} else {
			ActionManager.steering.x = x;
		}
	}
	
	public static Transform CurrentSectionTransform(int lookAhead = 0) {
		int passedPoints = Game.vehicle.GetComponent<Vehicle>().bm.passedPoints;
		return SectionManager.GetSection(passedPoints + lookAhead).obj.transform;
	}
	
	public static void RearSuperShoot(Vector3 crosshairPosition) {
//		crosshairPosition = MainUI.boundedCrosshairPosition(crosshairPosition);
		Game.backPlayer.SuperShoot(crosshairPosition);
	}
	
	/*
	 *  Creator / Destroyer specific shoot ability
	 */
	void SuperShoot(Vector3 crosshairPosition) {
        Ray ray = Camera.main.ScreenPointToRay(crosshairPosition);
		RaycastHit hit;
		int layerMask = 1 << 9; // Layer mask for Floors
	 
		if(IsCreator()){
			// Create wall	
	         if(Physics.Raycast(ray, out hit, 100.0f, layerMask) && hit.transform != null) 
	         { 
				GameObject wallOrig = GameObject.Find("ShootWall");
				GameObject wallClone = (GameObject)GameObject.Instantiate((Object)wallOrig);
				wallClone.name = "ShootWallClone";
				// Instantiate the clone where the player clicked
				wallClone.transform.position = hit.point;
				// Wall rotated to face the players current direction
				wallClone.transform.rotation = gameObject.transform.rotation;

	         }				
		}
		
		if(IsDestroyer()){
			// Create black hole
	         if(Physics.Raycast(ray, out hit, 100.0f, layerMask) && hit.transform != null) 
	         { 
				GameObject holeOrig = GameObject.Find("ShootBlackHole");
				GameObject holeClone = (GameObject)GameObject.Instantiate((Object)holeOrig);
				holeClone.name = "BlackHoleClone";
				// Instantiate the clone where the player clicked
				holeClone.transform.position = hit.point;
				// Wall rotated to face the players current direction
				holeClone.transform.rotation = gameObject.transform.rotation;

	         }					
		}
	}

}
