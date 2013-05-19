using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * PopupInfo:
 * 
 * Dictionary value; holds the data of a popup and whether that popup has been shown yet.
 * 
 */

public class PopupInfo {
	public bool shown;
	public string title;
	public string contents;
	
	public PopupInfo(string title,string contents){
		shown = false;	
		this.title = title;
		this.contents = contents;
	}
}

/*
 * Tutorial:
 * 
 * Allows for creation and destruction of tutorial popups.
 * When a popup is showing the game auto pauses itself.
 * Popups are cleared with the "Switch players" gesture.
 * 
 * Use the "blank" key when you have no message for one player.
 * 
 */

public class Tutorial : MonoBehaviour {
	static bool dialogueThisFrame = false;
	static PopupInfo leftPopup;
	static PopupInfo rightPopup;
	static Dictionary<string, PopupInfo> tutorialParts;
	
	// Renders the popup every frame
	void OnGUI () {
		// Do nothing if we are not in tutorial mode
		if(!Game.tutorialMode) return;
		if(dialogueThisFrame){
			Popup();
		}
	}
	
	// Pause the game if there is a dialogue up
	void Update(){
		if(!Game.tutorialMode) return;
		if(dialogueThisFrame){
			Time.timeScale = 0;
		} else {
			Time.timeScale = 1;		
		}
	}
	
	// Shorthand to set a popup just for the front player
	public static void SetFrontContent(string key){
		if(Game.frontPlayer.IsCreator()){
			SetPopupContent(key, "blank");
		} else {
			SetPopupContent("blank", key);
		}
	}
	
	// Shorthand to set a popup just for the back player
	public static void SetBackContent(string key){
		if(Game.backPlayer.IsCreator()){
			SetPopupContent(key, "blank");
		} else {
			SetPopupContent("blank", key);
		}		
	}

	// Shorthand to set a popup just for the creator
	public static void SetCreatorContent(string key){
		SetPopupContent(key, "blank");
	}
	
	// Shorthand to set a popup just for the destroyer
	public static void SetDestroyerContent(string key){
		SetPopupContent("blank", key);
	}

	// Shorthand to send same popup content to both players
	public static void SetBothPopupContent(string key){
		SetPopupContent(key, key);
	}
	
	// Activate the popup, and set its contents based on Dictionary lookup
	// Different popup for left and right hand screen
	public static void SetPopupContent(string leftKey, string rightKey){
		
		try{
			rightPopup = tutorialParts[rightKey];
		} catch (KeyNotFoundException) {
			Debug.LogError ("Missing Tutorial key: " + rightKey);
		}
		
		try{
			leftPopup = tutorialParts[leftKey];
		} catch (KeyNotFoundException) {
			Debug.LogError ("Missing Tutorial key: " + leftKey);
		}
		// Don't show the popup if we have shown both before
		if(leftPopup.shown && rightPopup.shown) return;
		
		// If not, mark them as shown
		leftPopup.shown = true;
		rightPopup.shown = true;
		
		tutorialParts[leftKey] = leftPopup;
		tutorialParts[rightKey] = rightPopup;
		
		dialogueThisFrame = true;
	}
	
	// Hides the popup
	public static void ClearDialogue(){
		if(dialogueThisFrame) ActionManager.tutorialCoolDown = Time.time;
		dialogueThisFrame = false;
	}
	
	static void Popup() {
		int vOffset = Screen.height/10;
		int hOffset = Screen.width*1/12;
		int titleHeight = Screen.height/10;
		int titleSeperation = Screen.height/10;
		int width = Screen.width*4/12;
		int height = Screen.height*7/10;
		
		GUI.Box(new Rect(hOffset,vOffset,width,titleHeight),leftPopup.title,Skin.sMainSkin.customStyles[1]);
		GUI.Label(new Rect(hOffset,vOffset+titleSeperation,width,height), leftPopup.contents,Skin.sMainSkin.customStyles[1]);
		
		GUI.Box(new Rect(hOffset+ (Screen.width/2),vOffset,width,titleHeight),rightPopup.title,Skin.sMainSkin.customStyles[1]);
		GUI.Label(new Rect(hOffset+ (Screen.width/2),vOffset+titleSeperation,width,height), rightPopup.contents,Skin.sMainSkin.customStyles[1]);
	}	
	
	static void CheckAppearedBefore() {
			
	}
	
	// TUTORIAL STRINGS
	
	void Awake(){
		tutorialParts = new Dictionary<string, PopupInfo>();
		// Here we are adding each PopupInfo(Title, Contents) to the hash table (Dictionary)
		// so that we may later access them by a string lookup conveniently.
		tutorialParts.Add("creatorIntroduction", new PopupInfo(
			@"Welcome to the Tutorial!",
			@"You are the CREATOR, while your friend is the DESTROYER. You have the ability to RESTORE broken stuff, and to SUPERCHARGE electricals.

You start as the driver. You can STEER the vehicle left and right with your LEFT HAND. Try not to hit things!

You can swap places by both lifting a leg at once."));

		tutorialParts.Add("destroyerIntroduction", new PopupInfo(
			@"Welcome to the Tutorial!",
			@"You are the DESTROYER, while your friend is the CREATOR. You have the ability to DESTROY weak stuff, and to SUPERHEAT explosive canisters. 

You start as the gunner. You can AIM the vehicle's gun with your LEFT HAND. You'll be shooting down the monsters attacks.

You can swap places by both lifting a leg at once."));

		// MONSTER POWERS
	
		tutorialParts.Add("monsterBig", new PopupInfo(
			@"Oh no!",
			@"Looks like the monster is using his most powerful attack!
	You can't stop this, so tell the driver to DODGE it!"
			));
	
		tutorialParts.Add("monsterLittle", new PopupInfo(
			@"Little attack incoming!",
			@"Looks like the monster has fired a bolt energy!
	Try shooting at it with the hover pads beam. Use your LEFT HAND to aim, the beam ALWAYS FIRES."
			));
		
		tutorialParts.Add("monsterStream", new PopupInfo(
			@"Watch out!",
			@"Looks like the monster has thrown a stream of energy along the floor!
	Only the antimatter professor can stop this! Supercharge your weapon by RAISING your RIGHT HAND till the crosshairs are blue, and aim it in FRONT of the stream."
			));
		
		tutorialParts.Add("monsterBeam", new PopupInfo(
			@"Heads up!",
			@"Looks like the monster has fired a dark bolt through the air!
		Only the regular professor can stop this! Supercharge your weapon by RAISING your RIGHT HAND till the crosshairs are blue, and aim it in front of the stream."
			));
	
		// CREATOR POWERS
		
		tutorialParts.Add("restorePower", new PopupInfo(
			@"Watch out!",
			@"There used to be a floor up ahead, ONLY you can restore it so we can pass over.
	To use your restore power, RAISE your RIGHT HAND near the broken floor."
			));
		
		tutorialParts.Add("superchargePower", new PopupInfo(
			@"Zap it!",
			@"Use your supercharge ability to overpower electrical circuits, it will make doors open.
	To use your supercharge power, RAISE your RIGHT HAND near the door."
			));
		
		// DESTROYER POWERS
		
		tutorialParts.Add("destroyPower", new PopupInfo(
			@"Don't step on the cracks!",
			@"When you see damaged looking areas, try using your destroy power to break through.
	To use your destroy power, RAISE your RIGHT HAND near what you want to destroy."
			));
		
		tutorialParts.Add("superheatPower", new PopupInfo(
			@"Blow the bridge!",
			@"That canister looks explosive, try using your superheat power to make it explode, clearing the way.
	To use your superheat power, RAISE your RIGHT HAND near what you want to make explode."
			));
		
		// MISCELLANEOUS 
		
		tutorialParts.Add("blank", new PopupInfo(
			@"Patience is a virtue...",
			@"Looks like your quantum friend over there has something to read, sorry about the wait, but you know what they're like!"
			));
		
	}

}
