using UnityEngine;
using System.Collections;

public class StartScreen : MonoBehaviour {
	
	public Texture2D title;
	int boxPositionX = Screen.width*1/100;
	int boxPositionY = Screen.height*3/8;
	int titlePositionX = Screen.width*1/100;
	int titlePositiony = Screen.height*1/16;
	int buttonWidth = Screen.width*2/10;
	int buttonHeight = Screen.height*1/10;	
	int buttonPadding = Screen.width*1/100;
	int buttonSpacing = Screen.height*12/100;
	int width = Screen.width*2/10 + 2*Screen.width*1/100;
	int height = Screen.height*4/9;
	int titleWidth = Screen.width*5/10;
	int titleHeight = Screen.height*3/10;
	
	void Start(){
		Screen.showCursor = true;	
	}
	
	void OnGUI () {
		GUI.skin = Skin.sMainSkin;
		// Make a background box
		GUI.DrawTexture(new Rect(titlePositionX,titlePositiony,titleWidth,titleHeight), title, ScaleMode.ScaleToFit);
		GUI.Box(new Rect(boxPositionX,boxPositionY,width,height), "");

		// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
		if(GUI.Button(new Rect(boxPositionX + 10, boxPositionY + 10,buttonWidth,buttonHeight), "Start")) {
			SectionManager.Restart();
			Time.timeScale = 1;
			Application.LoadLevel("Lab");
			Game.tutorialMode = false;
		}

		if(GUI.Button(new Rect(boxPositionX + 10, boxPositionY + 10*2 + buttonHeight,buttonWidth,buttonHeight), "Tutorial")) {
			SectionManager.Restart();
			Time.timeScale = 1;
			Application.LoadLevel("Lab");
			Game.tutorialMode = true;
		}

		if(GUI.Button(new Rect(boxPositionX + 10, boxPositionY + 10* 3 + 2*buttonHeight ,buttonWidth,buttonHeight), "Exit")) {
			Application.Quit();
		}
	
	}
}