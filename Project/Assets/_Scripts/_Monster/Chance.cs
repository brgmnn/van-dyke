using UnityEngine;
using System.Collections;

public class Chance {
	
	// Dictates how often the function will fire
	private float reg;
	// Decides the probability of true evlauation on successfull fires
	private float prob;
	
	private float lastFiredAt;
	private bool firedThisFrame;

	public Chance(float probability = 0.1f, float regularity = 0.5f){
		this.reg = regularity;
		this.prob = probability;
		this.lastFiredAt = Time.time;
	}
	

	// Returns true or false probabilistically dependent on initialisation variables
	public bool ShouldFire(){
		float currTime = Time.time;
		firedThisFrame = false;

		if(currTime - lastFiredAt < this.reg) return false;
		
		lastFiredAt = currTime;
		
		if(Random.value < this.prob){
			firedThisFrame = true;
			return true;
		} else {
			firedThisFrame = true;
			return false;		
		}
	}
	
	
	// Useful to know if we beat the odds, or if it just fired last frame
	public bool FiredThisFrame(){
		if(firedThisFrame) return true;
		else return false;
	}
	
}
