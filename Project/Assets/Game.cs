using System.Collections;
using UnityEngine;
/*
 * WORLD WIDE GAME OBJECT
 * STORES VALUES THAT ARE NEEDED THROUGHOUT THE GAME
 * 
 */

public class Game : MonoBehaviour {
	public static GameObject vehicle;
	public static GameObject vehicleInner;
	public static AudioClip labMusic;
	public static AudioClip lhcMusic;
	public static AudioClip labNoise;
	public static AudioClip lhcNoise;
	public static bool tutorialMode = false;
	public static AudioClip deathSound;
	public static bool mouseKeyboardInput = false;
	public bool mouseKeyboard1nput = false;
	// Stop construction of instance of this
	
	public static PlayerController frontPlayer;
	public static PlayerController backPlayer;
	public static PlayerController creatorPlayer;
	public static PlayerController destroyerPlayer;
	public static MonsterController monster;
	
	// Increases over time to make the game harder
	public static float difficulty;
	
	// Hacky but allows us to select static object in unity editor
	public GameObject i_vehicle;
	public GameObject i_vehicleInner;
	public AudioClip i_labMusic;
	public AudioClip i_lhcMusic;
	public AudioClip i_labNoise;
	public AudioClip i_lhcNoise;
	public AudioClip i_deathSound;
	public MonsterController i_monster;
	
	void Start(){
		vehicle = i_vehicle;
		vehicleInner = i_vehicleInner;
		labMusic = i_labMusic;
		lhcMusic = i_lhcMusic;
		labNoise = i_labNoise;
		lhcNoise = i_lhcNoise;
		lhcNoise = i_lhcNoise;	
		deathSound = i_deathSound;	
		mouseKeyboardInput = mouseKeyboard1nput;
		monster = i_monster;
		difficulty = 1.0f;
		
		if(tutorialMode) tutorialSetup();
	}
	
	void Update() {
		difficulty += Time.deltaTime/(60*5);
	}
	
	void tutorialSetup(){
		Tutorial.SetPopupContent("creatorIntroduction","destroyerIntroduction");	
	}
}
