using UnityEngine;
using System.Collections;

public class StreamProjectile : Projectile {

	void OnCollisionEnter(Collision other) {
		if(other.gameObject.name == "BlackHoleClone"){
			GameObject.Destroy(this.gameObject);
		}
	}

}
