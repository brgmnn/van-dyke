using UnityEngine;
using System.Collections;

public class FloorRestoreProperties : SectionProperties {

	
	public override void  RestoreFail(){
		int i = 0;
		// If this is false the player has already restored the ceiling.
		if(PlayerController.usableAbilities[(int)ActionManager.abilities.restore] == false) 
		{
			//Debug.Log("used ability");	
			return;
		}
		// If not.. punishment time.
		Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(2);
		//Debug.Log("failed section");
		if(Game.frontPlayer.IsCreator())
		{
			for(i = 0; i < 10 ; i++)
			{
				Game.vehicleInner.animation.CrossFadeQueued("strain1", 0.1f ,QueueMode.CompleteOthers);
			}
			Game.vehicleInner.animation["idle"].time = 0.0f;
		}
		else
		{
			for(i = 0; i < 10 ; i++)
			{
				Game.vehicleInner.animation.CrossFadeQueued("strain2", 0.1f ,QueueMode.CompleteOthers);
			}
			Game.vehicleInner.animation["idle"].time = 0.0f;
		}
		
		
		
	}
	
	public  override void Restore(){
		
		//Debug.Log("restoring");
		mayaSection.animation.Play("restore");
			PlayerController.usableAbilities[(int)ActionManager.abilities.restore] = false;
		}
	
}




