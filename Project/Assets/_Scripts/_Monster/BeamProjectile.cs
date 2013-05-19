using UnityEngine;
using System.Collections;

public class BeamProjectile : Projectile {

	void OnCollisionEnter(Collision other) {
		if(other.gameObject.name == "WallClone"){
			GameObject.Destroy(this.gameObject);
		}
	}

}
