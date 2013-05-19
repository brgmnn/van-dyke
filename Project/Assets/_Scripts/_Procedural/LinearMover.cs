using UnityEngine;
using System.Collections;

/*
 * LinearMover
 * 
 * Provides functionality to move the player from one waypoint to another.
 * No fancy turn stuff, just heads from point to point in a linear fashion.
 * 
 */

public class LinearMover {

	int passedPoints = 30;
	Vector3 currentGoal = Vector3.zero;
	GameObject target;
	int speed = 10;
	
	public LinearMover(GameObject t){
		target = t;
	}
	
	public void InitialMove(){
		if(!SectionManager.initialised) return;
		currentGoal = SectionManager.GetWaypoint(passedPoints);
		target.transform.position = currentGoal;
		passedPoints++;
	}
	
	public void Move(){
		if(!SectionManager.initialised) return;
		currentGoal = SectionManager.GetWaypoint(passedPoints);
		float distance = Vector3.Distance(target.transform.position, currentGoal);

		// If this is set to > 0, the player can get exceptionally close, but never hit exactly 0.
		if(distance > speed * Time.deltaTime){		
			Vector3 direction = Vector3.zero;
			Vector3 forwards = (currentGoal - target.transform.position).normalized;
			direction += forwards;
			target.transform.position += direction * speed * Time.deltaTime;		    
		} else {
			// If we are sat exactly on a point, change our goal to the next point
			passedPoints++;
		}
	}

}
