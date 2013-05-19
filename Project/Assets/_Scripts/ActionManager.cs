using UnityEngine;
using System.Collections;

/**
 * Action Manager
 * 
 * Called by PlayerController when a Kinect gesture is detected.
 * Processes the gesture with the section the player is in.
 * Triggers the appropriate animations / events.
 * 
 * Also when the vehicle hits a trigger, to allow animation initiation.
 * 
 * This is where logic such as how far away animations should trigger
 * should be set.
 * 
 */
public class ActionManager {
	
	// actions: represents possible gestures
	// used for input.
	public enum actions {
		switchPlaces = 1,
		handUp,
		handLeft,
		handRight,
		handDown,
		
		superShoot, // I know it's not in keeping with handUp/whatever but I plan on changing it later
		shoot
	}
	
	public static Monostable p1_switch = new Monostable(1.0f);
	public static Monostable p2_switch = new Monostable(1.0f);
	public static Monostable switch_cd = new Monostable(2.0f);
	
	// abilities: represents possible character abilities
	// used for PlayerController.usableAbilities toggling and lookup
	public enum abilities {
		restore = 0,		// Front
		restoreWall,			// Back
		superheat,
		breakobject,
		supercharge,
		rift,
	}
	
	public static Vector2 aiming = Vector2.zero;
	public static Vector2 steering = Vector2.zero;
	public static float tutorialCoolDown = -1.0f;
	

	// Takes a target and an action, checks wether anything should be triggered,
	// and triggers the appropriate event if needed.
	static public void Perform(PlayerController target, int action){
		if(action == (int)actions.switchPlaces){
			if (target.IsCreator()) {
				p1_switch.set();
			}
			if (target.IsDestroyer()) {
				p2_switch.set();
			}
			
			// Switch their programmed positions
			if (p1_switch.get() && p2_switch.get() && Time.time > tutorialCoolDown + 1f) {
				if(Time.timeScale > 0.5 && !switch_cd.get()){
					PlayerController.SwitchPosition();
					switch_cd.set();
				} else if(Game.tutorialMode){
					// If in tutorial mode, switch positions instead closes the tutorial prompt
					Tutorial.ClearDialogue();
				}

			}
			
			return;
		}
		// Get section properties for the current section
		SectionProperties sp = PlayerController.CurrentSectionTransform(2).GetComponent<SectionProperties>();
		if(target.IsDriver() && target.IsCreator()){
			switch (action){
			case (int)actions.handUp: // restore floor / ceiling and supercharge.
				if(PlayerController.usableAbilities[(int)abilities.restore]){
					sp.Restore();
				}
				else if(PlayerController.usableAbilities[(int)abilities.supercharge]){
					sp.Supercharge();
				}
				break;
			}
		} else if(target.IsDriver() && target.IsDestroyer()){
			switch (action){
			case (int)actions.handUp: // Destroy weak object and superheat.
				if(PlayerController.usableAbilities[(int)abilities.breakobject]){
					sp.Destroy();
				}
				else if(PlayerController.usableAbilities[(int)abilities.superheat]){
					sp.Superheat();
				}
				break;
			}			
		} else if(target.IsGunner() && target.IsCreator()){
		} else if(target.IsGunner() && target.IsDestroyer()){
		}
	}
	
	static public void Trigger(Collider trigger){
		SectionProperties sp = PlayerController.CurrentSectionTransform(0).GetComponent<SectionProperties>();
		switch (trigger.name){
		case "restoreCeilingStart": case "restoreFloorStart": case "restoreStairsStart" : case "restoreGeneratorStart" :
			PlayerController.usableAbilities[(int)abilities.restore] = true;
			Tutorial.SetCreatorContent("restorePower");
			break;
		/*case "restoreGeneratorEnd" :
			sp.RestoreGeneratorFail();
			PlayerController.usableAbilities[(int)abilities.restore] = false;
			break;*/
		case "restoreFloorEnd": 
			sp.RestoreFail();
			PlayerController.usableAbilities[(int)abilities.restore] = false;
			break;
		case "restoreCeilingEnd": 
			sp.RestoreFail();
			PlayerController.usableAbilities[(int)abilities.restore] = false;
			break;
			case "restoreStairsEnd": 
			sp.RestoreFail();
			PlayerController.usableAbilities[(int)abilities.restore] = false;
			break;
		case "restoreWallStart": 
			PlayerController.usableAbilities[(int)abilities.restoreWall] = true;
			Tutorial.SetCreatorContent("restorePower");
			break;
		case "restoreWallEnd":
			sp.RestoreFail();
			PlayerController.usableAbilities[(int)abilities.restoreWall] = false;
			break;
		case "destroyFloorStart": 
			PlayerController.usableAbilities[(int)abilities.breakobject] = true;
			Tutorial.SetDestroyerContent("destroyPower");
			break;
		case "destroyFloorEnd": 
			sp.DestroyFail();
			PlayerController.usableAbilities[(int)abilities.breakobject] = false;
			break;
		case "destroyWallStart": 
			PlayerController.usableAbilities[(int)abilities.breakobject] = true;
			Tutorial.SetDestroyerContent("destroyPower");
			break;
		case "destroyWallEnd":
			sp.DestroyFail();
			PlayerController.usableAbilities[(int)abilities.breakobject] = false;
			break;
		case "superheatStart":
			Tutorial.SetDestroyerContent("superheatPower");
			PlayerController.usableAbilities[(int)abilities.superheat] = true;
			break;
		case "superheatEnd":
			sp.SuperheatFail();
			PlayerController.usableAbilities[(int)abilities.superheat] = false;
			break;
		case "breakStart":
			PlayerController.usableAbilities[(int)abilities.breakobject] = true;
			Tutorial.SetDestroyerContent("destroyPower");
			break;
		case "breakEnd":
			sp.DestroyFail();
			PlayerController.usableAbilities[(int)abilities.breakobject] = false;
			break;
		case "superchargeStart":
			PlayerController.usableAbilities[(int)abilities.supercharge] = true;
			Tutorial.SetCreatorContent("superchargePower");
			break;
		case "superchargeEnd":
			sp.SuperchargeFail();
			PlayerController.usableAbilities[(int)abilities.supercharge] = false;
			break;
		case "playerEnterAnimationTrigger":
			sp.playerEnterAnimationTrigger();
			break;
		case "weakWall":
			sp.DestroyFail();
			break;
		
		
		}

	
		switch (trigger.tag){
		case "slowLight":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(1);
			break;
		case "slowModerate":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(2);
			break;
		case "slowHeavy":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(3);
			break;
		case "fastLight":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(4);
			break;
		case "fastModerate":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(5);
			break;
		case "fastHeavy":
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(6);
			break;
		case "monSlowLight":
			//Vehicle.ChangeSpeed(1);
			break;
		case "monSlowModerate":
			//Vehicle.ChangeSpeed(2);
			break;
		case "monSlowHeavy":
			//Vehicle.ChangeSpeed(3);
			break;
		}
		
		// Destroys projectiles that hit the player
		// No "p" so its not case sensitive.
		if(trigger.gameObject.name.Contains("rojectile")){
			GameObject.Destroy(trigger.gameObject);
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(1);
		}
	
	}
	
	static public void Shoot(GameObject target, Vector3 direction){
	}
}
