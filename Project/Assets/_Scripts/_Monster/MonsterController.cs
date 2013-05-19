using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour {

	public BezierMover bm;

	public float speed;
	float initialSpeed = 2.6f;
	public GameObject mayaMonster;
	int delaySeconds = 6;
	
	float distanceToPlayer;
	float minDistance = 10.0f;
	int state = (int)states.unspawned;
	
	enum states {
		unspawned,
		spawning,
		idle
	}
	
	Chance littleChance;
	Chance bigChance;
	Chance grabChance;

	// Use this for initialization
	void Start () {
		speed = initialSpeed;
		littleChance = new Chance(0.7f * Game.difficulty,2f);
		bigChance = new Chance(0.50f * Game.difficulty, 15f);
		// This means grab will fire at most once a second
		// So if you do escape you aren't instantly fucked.
		grabChance = new Chance(0.8f,1f);
		bm = new BezierMover(gameObject);
		bm.speed = speed;

	}
	
	// Update is called once per frame
	void Update () {
		// Delays monster by n seconds
		if(TimePassed(delaySeconds)) return;
		
		// Secretly change the monster's speed to be a tad in line with the player's
		// float handicap = ((Game.vehicle.GetComponent<Vehicle>().speed - this.speed)/2000);
		// speed = speed + handicap;
		
		// Slowly increase monster speed
		
		bm.speed = speed * (2.0f+Game.difficulty)/3.0f;

		// Don't do anything until the monster has spawned
		if(state == (int)states.unspawned || state == (int)states.spawning) {
			Spawn();
			return;
		}

		bm.Move();
		if(PlayerController.gameOver) return;

		distanceToPlayer = Vector3.Distance(transform.position,Game.vehicle.transform.position);
		// MusicIntensity(); // SOUNDS BAD
		
		
		// Ensures we don't fire more than one attack on one frame
		// And that we don't fire attacks when very close
		if(GrabAttack() || 
			distanceToPlayer > minDistance && (
				LittleAttack() ||
				BigAttack()
			)){
			// Do nothing
		}
	}
	
	bool TimePassed(int time){
		if(Time.timeSinceLevelLoad > time){
			return false;
		} else {
			return true;
		}
	}
	
	void Idle(){
		mayaMonster.animation.Play("idle",AnimationPlayMode.Queue);		
	}
	
	void Spawn(){
		// Check whether we have already begun spawning
		if(state == (int)states.unspawned){
			mayaMonster.animation.Play("spawn");
			Idle();
			state = (int)states.spawning;
		} else if(state == (int)states.spawning) {
			bm.Move(); // Looks crap because monster is moving with floor.
			if(!mayaMonster.animation.IsPlaying("spawn")){
				state = (int)states.idle;
			}
		}
	}
	
	bool GrabAttack(){
		if(distanceToPlayer > 5.0f) return false;
		
		mayaMonster.animation.Play("grab");
		Idle();
		if(grabChance.ShouldFire()){
			PlayerController.gameOver = true;
			// Attempt to position self correctly
			if(distanceToPlayer < 2.5f){
				speed = Game.vehicle.GetComponent<Vehicle>().speed - 2;
			}else {
				speed = Game.vehicle.GetComponent<Vehicle>().speed;
			}
			AudioSource[] a_s = Game.vehicleInner.GetComponents<AudioSource>();
			a_s[2].Stop();
			audio.clip = Game.deathSound;
			audio.Play();
			return true;
		} else if(grabChance.FiredThisFrame()) {
			// Player escaped by fluke!
			Game.vehicle.GetComponent<Vehicle>().speed += 2;
		}	
		return false;
	}

	bool BigAttack(){
		if(!bigChance.ShouldFire()) return false;
		Tutorial.SetBackContent("monsterBig");
		mayaMonster.animation.Play("big");
		Idle();
		Debug.Log("CREATING BIG");
		GameObject bigOrig = GameObject.Find("BigProjectile");
		GameObject bclone = (GameObject)GameObject.Instantiate((Object)bigOrig);
		bclone.name = "BigProjectileClone";
		bclone.GetComponent<Projectile>().Activate();
		return true;
	}

	bool LittleAttack(){
		if(!littleChance.ShouldFire()) return false;
		Tutorial.SetBackContent("monsterLittle");
		mayaMonster.animation.Play("little");
		Idle();
		Debug.Log("CREATING LITTLE");
		GameObject littleOrig = GameObject.Find("LittleProjectile");
		GameObject clone = (GameObject)GameObject.Instantiate((Object)littleOrig);
		clone.name = "LittleProjectileClone";
		clone.GetComponent<Projectile>().Activate();
		return true;
	}

	void MusicIntensity(){
		AudioSource music = Game.vehicleInner.GetComponents<AudioSource>()[2];
		float pitchUp = 1/(10.0f*distanceToPlayer);
		music.pitch = 1 + Mathf.Min(pitchUp,0.1f);
	}
	
}
