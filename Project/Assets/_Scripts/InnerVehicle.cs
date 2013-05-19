using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class InnerVehicle : MonoBehaviour {
	
	public AudioClip collisionSound;
	
	AudioSource collisionSource;
	AudioSource humSource;
	AudioSource musicSource;
	
	float lastCollision = 0;
	float soundSensitivity = 0.01f;
	
	void Start(){
		// Initialise our audio sources
		AudioSource[] aSources = GetComponents<AudioSource>();
		collisionSource = aSources[0];
		humSource = aSources[1];
		musicSource = aSources[2];
		
		humSource.clip = Game.labNoise;
		humSource.loop = true;
		humSource.Play();

		musicSource.clip = Game.labMusic;
		musicSource.loop = true;
		musicSource.Play();
	}
	
	// Pass trigger hits up to parent
	void OnTriggerEnter(Collider other) {
		Game.vehicle.GetComponent<Vehicle>().InnerTriggerEnter(other);
		
		if(other.tag.Contains("slow")){
	        if (lastCollision + 0.1f < Time.time) {
				lastCollision = Time.time;
	            collisionSource.PlayOneShot(collisionSound);
	            collisionSource.pitch = (Random.value * 0.5f + 0.5f);
	        }			
		}
	}
	
	void OnCollisionEnter(Collision other) {
		Game.vehicle.GetComponent<Vehicle>().InnerCollisionEnter(other);
		// Smashing sound when we hit stuff
        if (other.relativeVelocity.magnitude > soundSensitivity && lastCollision + 0.1f < Time.time) {
			lastCollision = Time.time;
            collisionSource.PlayOneShot(collisionSound);
            collisionSource.volume = (other.relativeVelocity.magnitude / 20 + 0.5f);
            collisionSource.pitch = (Random.value * 0.5f + 0.5f);
        }
	}
	
	public void TriggerSounds(){
        if (lastCollision + 0.1f < Time.time) {
			lastCollision = Time.time;
            collisionSource.PlayOneShot(collisionSound);
            collisionSource.pitch = (Random.value * 0.5f + 0.5f);
        }		
	}
	
}
