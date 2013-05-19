using UnityEngine;
using System.Collections;

/**
 * Projectile: Anything that comes from the Monster and chases the player.
 * Slowdown is taken care of in ActionManager
 */
public class Projectile : MonoBehaviour {

	public BezierMover bm;
	public float speedBoost = 2.0f;
	
	protected float distToPlayer = 0.0f;

	protected bool activated = false;
	protected float path = 0.0f;
		
	public virtual void Activate() {
		activated = true;
		this.bm = new BezierMover(gameObject);
		bm.SkipForward(Game.monster.bm);
		bm.speed = Game.monster.speed + speedBoost;
	}
	
	protected virtual void Update() {
		if (activated) {
			bm.Move();
		}
		
				/*works out the distance from the placer every update to ensure it destroys once it passes the player*/
		distToPlayer = Vector3.Distance(transform.position,Game.vehicle.transform.position);
		if (distToPlayer < 10){
		//	GameObject.Destroy(this.gameObject, 20);
		}
		
		
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject == Game.vehicleInner) {
			GameObject.Destroy(this.gameObject);
		}
	}

}
