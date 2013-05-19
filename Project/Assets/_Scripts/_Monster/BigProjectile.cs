using UnityEngine;
using System.Collections;

public class BigProjectile : Projectile {

	public override void Activate() {
		/*moves the big projectile along the lateral position of the vehicle (as it was when the power was activated)*/
		path = Game.vehicle.GetComponent<Vehicle>().lateralMovementCurrent;	
		base.Activate();
	}
	
	protected override void Update() {
		if (!activated) return;
		// Disabling Big looking at the target seems necessary to stop flickering.
		bm.Move();
		
		/*works out the distance from the placer every update to ensure it destroys once it passes the player*/
		distToPlayer = Vector3.Distance(transform.position,Game.vehicle.transform.position);
		if (distToPlayer < 10){
			GameObject.Destroy(this.gameObject, 20);
		}
	}
	
}
