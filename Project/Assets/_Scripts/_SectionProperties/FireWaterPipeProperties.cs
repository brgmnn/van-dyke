using UnityEngine;
using System.Collections;

public class FireWaterPipeProperties : SectionProperties {

void WaterPipeExplode(Transform s){
		//TODO causes error with iris door, still works 
		try{
			s.animation.Play("breakobject");
		}catch{
		}
		
		GameObject water0 = s.transform.FindChild("water0").gameObject;
		water0.particleEmitter.emit = true;
		GameObject water1 = s.transform.FindChild("water1").gameObject;
		water1.particleEmitter.emit = true;
		GameObject flame0 = s.transform.FindChild("flame0").gameObject;
		flame0.particleEmitter.emit = false;
		GameObject flame1 = s.transform.FindChild("flame1").gameObject;
		flame1.particleEmitter.emit = false;
		GameObject flame2 = s.transform.FindChild("flame2").gameObject;
		flame2.particleEmitter.emit = false;	
	//	GameObject destroyObject = s.transform.FindChild("destroyObject").gameObject;
	//	destroyObject.tag = "Untagged";
	}
	
	
	
	public override void Destroy(){
		WaterPipeExplode(mayaSection.transform);
		
		if(mayaSection.transform.FindChild("Water")){
			WaterPipeExplode(mayaSection.transform);
		}
		if(mayaSection.transform.FindChild("water0")){
			WaterPipeExplode(mayaSection.transform);
		}
		base.Destroy();
		
		
	}
}
