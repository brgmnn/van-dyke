using UnityEngine;
using System.Collections;

public class MainUI : MonoBehaviour {
	public Texture2D cursorImage;
	public Texture2D cursorImageActive;
	float totalScore = 0;
	private int cursorWidth = 32;
    private int cursorHeight = 32;
	private float cursorMul = 1.0f;
	private bool paused = false;
	public GameObject vehicle;
    static int guiDepth = 10;
	private static bool left_mouse_up = false;
	private static bool left_mouse_down = false;
	
	void Start () {
		Screen.showCursor = false;
	}
	
	void Update(){		
        if( Input.GetKeyUp(KeyCode.Space)){
			togglePause();	
		}
		
		left_mouse_up = false;
	}	

	void OnGUI () {
		GUI.depth = guiDepth;
		GUI.skin = Skin.sMainSkin;
		// currentScore();
		if(!paused) mousePointer();
		
		if(paused){
			
			int buttonWidth = Screen.width/10;
			int buttonHeight = Screen.width/20;
			
			if(GUI.Button(new Rect((Screen.width/4)-buttonWidth/2, (Screen.height/2)-buttonHeight/2,buttonWidth,buttonHeight), "Restart")) {
				togglePause();
				Application.LoadLevel("StartScreen");
			}	
			if(GUI.Button(new Rect((3*Screen.width/4)-buttonWidth/2, (Screen.height/2)-buttonHeight/2,buttonWidth,buttonHeight), "Restart")) {
				togglePause();
				Application.LoadLevel("StartScreen");
			}	
		}
	
	}
	
	public static void SimulateMouseDown() {
		left_mouse_down = true;
	}
	
	public static void SimulateMouseUp() {
		left_mouse_up = true;
		left_mouse_down = false;
	}
	
	void currentScore () {
		totalScore += (Time.deltaTime * 100 * vehicle.GetComponent<Vehicle>().speed) * 
			(10+PlayerController.challengesDefeatedStreak)/10;
		string score = totalScore.ToString("#,##0");
		Rect leftPos = new Rect (Screen.width/4-50, 10, 100, 50);
		Rect rightPos = new Rect ((3*Screen.width/4)-50 , 10, 100, 50);
		GUIStyle scoreFormat = new GUIStyle();
		scoreFormat.fontSize = 20;
		scoreFormat.fontStyle = FontStyle.Bold;
		scoreFormat.normal.textColor = Color.white;
		GUI.Label(leftPos, score, scoreFormat);
		GUI.Label(rightPos, score, scoreFormat);
	}
	
	public static Vector3 boundedCrosshairPosition(Vector3 unboundedCrosshairPosition){ 
		float sensitivity = 0.5f;
		float cursorX;
		float cursorY;
		if(Game.mouseKeyboardInput) {
			cursorX = unboundedCrosshairPosition.x;
			cursorY = unboundedCrosshairPosition.y;
		} else {
			cursorY = unboundedCrosshairPosition.y*Screen.height;
			
			if(Game.backPlayer.IsCreator()) {
				cursorX = sensitivity*(unboundedCrosshairPosition.x*Screen.width)
					+ (1.0f-sensitivity)*(Screen.width/2)
					- (Screen.width/4);
			} else {
				cursorX = sensitivity*(unboundedCrosshairPosition.x*Screen.width)
					+ (1.0f-sensitivity)*(Screen.width/2)
					+ (Screen.width/4);
			}
		}
		
		if(Game.backPlayer.IsCreator()){
			return new Vector3(Mathf.Min(Screen.width/2, cursorX),cursorY,0);
		} else {
			return new Vector3(Mathf.Max(Screen.width/2, cursorX),cursorY,0);
		}
		
	}

	void mousePointer(){
		Texture2D cursorImageTmp = cursorImage;
		
		Vector3 crosshair;
		float cursorX, cursorY;
		if(Game.mouseKeyboardInput){
			crosshair = boundedCrosshairPosition(Input.mousePosition);
		} else {
			crosshair = boundedCrosshairPosition(ActionManager.aiming);		
		}
		
		float adjusted_x = crosshair.x - (cursorWidth*cursorMul/2); 
		float adjusted_y = Screen.height - crosshair.y - (cursorHeight*cursorMul/2);
		float scaledCursorWidth = cursorWidth*cursorMul;
		float scaledCursorHeight = cursorHeight*cursorMul;
		
		// on mouse hold
		if( Input.GetMouseButton(0) || left_mouse_down ){
			if(Time.time - vehicle.GetComponent<Vehicle>().gunChargingSince > 1f){
				cursorImageTmp = cursorImageActive;
			} else if(Time.time - vehicle.GetComponent<Vehicle>().gunChargingSince > 0.1f)
				cursorMul += 0.25f*Time.deltaTime;
		} else {
			cursorMul = 1;	
		}
		
		// Establish which player is at the back right now
		if(Game.backPlayer.IsDestroyer()){		
			if(Game.mouseKeyboardInput){
				GUI.DrawTexture(
				new Rect(adjusted_x, adjusted_y,
					scaledCursorWidth, scaledCursorHeight), cursorImageTmp);
			} else {
				GUI.DrawTexture(
				new Rect(adjusted_x, adjusted_y,
					scaledCursorWidth, scaledCursorHeight), cursorImageTmp);
			}	
		} else {
			if(Game.mouseKeyboardInput){
				GUI.DrawTexture(
				new Rect(adjusted_x, adjusted_y,
					scaledCursorWidth, scaledCursorHeight), cursorImageTmp);
			} else {
				GUI.DrawTexture(
				new Rect(adjusted_x, adjusted_y,
					scaledCursorWidth, scaledCursorHeight), cursorImageTmp);
			}	
		}
	}
	
	void togglePause(){
		if(paused) {
			Screen.showCursor = false;
			Time.timeScale = 1.0f;
		} else {
			Screen.showCursor = true;
			Time.timeScale = 0.0f;
		}		
		paused = !paused;	
	}
}
