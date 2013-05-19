	using UnityEngine;

public class SectionProperties : MonoBehaviour {

	// 0 = lab, 1 = lhc, 2 = lab>lhc, 3 = lhc>lab
	public int sectionType = 0;
	public GameObject wayPointsObj;
	public GameObject endPointObj;
	public GameObject mayaSection;
	
	// Disables failure punishments for multiple consecutive walls
	bool ignoreFailure = false;
	
	// Used to disable challenges that haven't been completed when another has
	public bool multipleChallenges = false;
	
	// CREATOR ABILITIES
	public virtual void Restore(){
		mayaSection.animation.Play("restore");
		
		foreach (AnimationState state in mayaSection.animation) {
	        state.speed = 0.5f * Game.vehicle.GetComponent<Vehicle>().speed;
	    }
		
		PlayerController.usableAbilities[(int)ActionManager.abilities.restore] = false;
	

		
	}
	
	public virtual void Supercharge(){
		mayaSection.animation.Play("supercharge");
		
		if(mayaSection.transform.FindChild("Xraybeam") !=null){
			//BEAMSHOOT REMOVED
		}
	}
	
	public virtual void Create(){}
	
	public virtual void Repair(){}
	
	// CREATOR FAIL TRIGGERS
	public virtual void RestoreFail(){
		// If this is false the player has already restored the ceiling.
		if(PlayerController.usableAbilities[(int)ActionManager.abilities.restore] == false) 
			return;
		// If not.. punishment time.
		Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(2);		
	}
	
	public virtual void SuperchargeFail(){
		if(PlayerController.usableAbilities[(int)ActionManager.abilities.supercharge] == false) 
			return;
		// If not.. punishment time.		
		Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(2);
	}
	
	public virtual void CreateFail(){}
	
	public virtual void RepairFail(){}
	
	// DESTROYER ABILITIES
	public virtual void Superheat(){
		Vector3 explosionPos = Vector3.zero;
		float radius = 3;
		float power = 1000;
		
		foreach (Transform child in transform.FindChild("Superheat").transform) {
		    if(child.tag != "Explosive") continue;
			explosionPos = child.position;
			PlayerController.usableAbilities[(int)ActionManager.abilities.superheat] = false;
			break;
		}

		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
		
        foreach (Collider hit in colliders) {
            if (hit.rigidbody){
				//Debug.Log ("Exploded!");
                hit.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);
			}
        }		
		
		try
		{
			GameObject superheatCollider = mayaSection.transform.FindChild("superheatCollider").gameObject;
			superheatCollider.tag = "Untagged";
		}catch{
		}
		
	}
	
	public virtual void Destroy(){

		if(mayaSection.transform.FindChild("weakFloor")){
			mayaSection.animation.Play("destroyFloor");	
		}
		if(mayaSection.transform.FindChild("weakWall")){
			mayaSection.animation.Play("destroy");	
		}
		PlayerController.usableAbilities[(int)ActionManager.abilities.breakobject] = false;
	}
	
	public virtual void Rift(){}
	
	// DESTROYER FAIL TRIGGERS
	public virtual void SuperheatFail(){}
	
	public virtual void DestroyFail(){
		if(PlayerController.usableAbilities[(int)ActionManager.abilities.breakobject] == false) {
			return;
		}
		
		if(mayaSection.animation["destroy"]){
			mayaSection.animation.Play("destroy");	
		} else if(mayaSection.animation["destroyFloor"]) {
			mayaSection.animation.Play("destroyFloor");	
		}
		Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(2);
	}
	
	public virtual void RiftFail(){}
	
	// GENERAL TRIGGERS

	/*
	 * Will fire the animation of whatever gameobject is put in the genericAnim
	 * slot of the Section.
	 */
	public virtual void playerEnterAnimationTrigger(){
		mayaSection.animation.Play();
	}
	
	// LEGACY FUNCTIONS, SHOULD BE REPLACED BY CUSTOM SECTION SCRIPTS
	

	//Leaving in for the split level section 
	//Needs porting out but can stay here for a wee whie
	
	/*
	static void RestoreGeneratorFail() {
			// If this is false the player has already restored the generator.
		if(PlayerController.usableAbilities[(int)ActionManager.abilities.restore] == false) {
			return;
		} else {
		// If not.. punishment time.
		Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(1);
		}
	}
	
	static void RestoreGenerator(SectionProperties sp, Transform s) {
		sp.mayaSection.animation.Play("restore");
		GameObject flame0 = s.transform.FindChild("flame0").gameObject;
		flame0.particleEmitter.emit = false;
		GameObject flame1 = s.transform.FindChild("flame1").gameObject;
		flame1.particleEmitter.emit = false;
	}
	*/

}

