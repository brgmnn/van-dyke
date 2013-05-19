using UnityEngine;
using System.Collections;

public class LittleProjectile : Projectile {
	
	protected virtual void Update(){
		base.Update();
		
		float distanceToPlayer = Vector3.Distance(transform.position,Game.vehicle.transform.position);
		
		if(distanceToPlayer < 1.0f){
			Debug.Log("Little hit");
			Game.vehicle.GetComponent<Vehicle>().ChangeSpeed(1);
			GameObject.Destroy(gameObject);
		}
	}

}
