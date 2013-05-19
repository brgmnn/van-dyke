using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

using UnityEngine;

public class Person {
	public string name;
	public int age;
	public double height;
}

public class KinectSkeleton : MonoBehaviour {
	
	//public KUInterface kui;
	public int player = 1;
	private float scale_factor = 10.0f;
	
	public PlayerController controller;
	
	public GameObject skHead;
	public GameObject skHipCenter;
	public GameObject skSpine;
	public GameObject skShoulderCenter;
	public GameObject skLeftShoulder;
	public GameObject skLeftElbow;
	public GameObject skLeftWrist;
	public GameObject skLeftHand;
	public GameObject skRightShoulder;
	public GameObject skRightElbow;
	public GameObject skRightWrist;
	public GameObject skRightHand;
	
	private static KinectSkeleton ks_master = null;
	
	// threading variables
	private volatile Mutex joint_data_mutex = new Mutex();
	private Thread thr_joint_data_update;
	private volatile bool is_running = true;
	
	//public string pathJointScalingsFile;
	private Dictionary<Joints, GameObject> skeleton = new Dictionary<Joints, GameObject>();
	private Dictionary<GameObject, Joints> inverse_skeleton = new Dictionary<GameObject, Joints>();
	
	private static KinectSkeletonClient client;
	private bool tracking_player = false;
	private volatile Vector3[] game_joint_pos = new Vector3[(int)Joints.COUNT];
	internal volatile Vector3[] raw_joint_pos = new Vector3[(int)Joints.COUNT];
	
	internal volatile float[] bone_lengths = new float[(int)Joints.COUNT];
	internal volatile Vector3[] model_joint_pos = new Vector3[(int)Joints.COUNT];
	
	private Dictionary<String, Gesture> gestures = new Dictionary<String, Gesture>();
	private Dictionary<String, AnimatedGesture> animated_gestures = new Dictionary<String, AnimatedGesture>();
	private Dictionary<String, Mapping> mappings = new Dictionary<String, Mapping>();
	
	// Use this for initialization
	void Start () {
		if (ks_master == null)
			ks_master = this;
		
		raw_joint_pos[(int)Joints.HIP_CENTER] = new Vector3(0.0f, 0.0f, 0.0f);
		game_joint_pos[(int)Joints.HIP_CENTER] = new Vector3(0.0f, 0.0f, 0.0f);
		
		// adds all the game objects to the associated hashmap for the enum type
		if (skHead != null)
			add_skeleton_joint (Joints.HEAD, skHead);
		if (skSpine != null)
			add_skeleton_joint (Joints.SPINE, skSpine);
		if (skHipCenter != null)
			add_skeleton_joint (Joints.HIP_CENTER, skHipCenter);
		if (skShoulderCenter != null)
			add_skeleton_joint (Joints.SHOULDER_CENTER, skShoulderCenter);
		
		if (skLeftShoulder != null)
			add_skeleton_joint (Joints.SHOULDER_LEFT, skLeftShoulder);
		if (skLeftElbow != null)
			add_skeleton_joint (Joints.ELBOW_LEFT, skLeftElbow);
		if (skLeftWrist != null)
			add_skeleton_joint (Joints.WRIST_LEFT, skLeftWrist);
		if (skLeftHand != null)
			add_skeleton_joint (Joints.HAND_LEFT, skLeftHand);
		
		if (skRightShoulder != null)
			add_skeleton_joint (Joints.SHOULDER_RIGHT, skRightShoulder);
		if (skRightElbow != null)
			add_skeleton_joint (Joints.ELBOW_RIGHT, skRightElbow);
		if (skRightWrist != null)
			add_skeleton_joint (Joints.WRIST_RIGHT, skRightWrist);
		if (skRightHand != null)
			add_skeleton_joint (Joints.HAND_RIGHT, skRightHand);
		
		// get the bone lengths and positions of the default pose for the rigged character.
		foreach (KeyValuePair<Joints, GameObject> joint in skeleton) {
			if (joint.Key != Joints.HIP_CENTER) {
				bone_lengths[(int)joint.Key] = joint.Value.transform.localPosition.magnitude;
				model_joint_pos[(int)joint.Key] = joint.Value.transform.localPosition;
			}
		}
		
		// start the kinect skeleton server and the kinect skeleton client for this game
		client = new KinectSkeletonClient();
		
		// starts the joint data update thread
		thr_joint_data_update = new Thread(this.run_joint_data_update);
		thr_joint_data_update.Start();
		
		// some testing gestures
		// this should be moved out and in to a json file ideally.
		
		// ok
//		Gesture left_hand_up = new Gesture("left-hand-up", (int)ActionManager.actions.handUp);
//		left_hand_up.constraints.Add( new JointConstraint(	this,
//														Joints.HAND_LEFT,
//														Joints.SHOULDER_LEFT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.GREATER_THAN,
//														new Vector3(-10.0f, 1.2f, -10.0f) ) );
//		Gesture left_hand_down = new Gesture("left-hand-down", (int)ActionManager.actions.handDown);
//		left_hand_down.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_LEFT,
//														Joints.SPINE,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.LESS_THAN,
//														new Vector3(-10.0f, 0.0f, -10.0f) ) );
		// ok
//		Gesture left_hand_left = new Gesture("left-hand-left", (int)ActionManager.actions.handLeft);
//		left_hand_left.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_LEFT,
//														Joints.SHOULDER_LEFT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.LESS_THAN,
//														new Vector3(-2.0f, 10.0f, 10.0f) ) );
		
		// ok
//		Gesture left_hand_right = new Gesture("left-hand-right", (int)ActionManager.actions.handRight);
//		left_hand_right.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_LEFT,
//														Joints.SHOULDER_LEFT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.GREATER_THAN,
//														new Vector3(1.0f, -10.0f, -10.0f) ) );
		
		
		// ok
		Gesture right_hand_up = new Gesture("right-hand-up", (int)ActionManager.actions.handUp);
		right_hand_up.constraints.Add( new JointConstraint(this,
														Joints.HAND_RIGHT,
														Joints.SHOULDER_RIGHT,
														JointConstraint.Relations.COMPONENT_DISTANCE,
														JointConstraint.Operators.GREATER_THAN,
														new Vector3(-10.0f, 1.2f, -10.0f) ) );
		// ok
//		Gesture right_hand_left = new Gesture("right-hand-left", (int)ActionManager.actions.handLeft);
//		right_hand_left.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_RIGHT,
//														Joints.SHOULDER_RIGHT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.LESS_THAN,
//														new Vector3(-1.0f, 10.0f, 10.0f) ) );
//		// ok
//		Gesture right_hand_right = new Gesture("right-hand-right", (int)ActionManager.actions.handRight);
//		right_hand_right.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_RIGHT,
//														Joints.SHOULDER_RIGHT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.GREATER_THAN,
//														new Vector3(2.0f, -10.0f, -10.0f) ) );
		
//		Gesture super_charge_charging = new Gesture("super-charge-charging", (int)ActionManager.actions.superShoot);
//		super_charge_charging.constraints.Add ( new JointConstraint(this,
//														Joints.HAND_LEFT,
//														Joints.SHOULDER_LEFT,
//														JointConstraint.Relations.DISTANCE,
//														JointConstraint.Operators.LESS_THAN,
//														new Vector3(4.0f, 0.0f, 0.0f) ) );
//		super_charge_charging.constraints.Add( new JointConstraint(this,
//														Joints.HAND_RIGHT,
//														Joints.SHOULDER_RIGHT,
//														JointConstraint.Relations.COMPONENT_DISTANCE,
//														JointConstraint.Operators.GREATER_THAN,
//														new Vector3(-10.0f, 1.2f, -10.0f) ) );
		
//		gestures.Add("super-charge-charging", super_charge_charging);
		
		
		// gestures to handle either left knee raised or right knee raised
		Gesture switch_places_ll = new Gesture("switch-places-left-leg", (int)ActionManager.actions.switchPlaces);
		switch_places_ll.constraints.Add( new JointConstraint(this,
														Joints.KNEE_LEFT,
														Joints.HIP_CENTER,
														JointConstraint.Relations.COMPONENT_DISTANCE,
														JointConstraint.Operators.GREATER_THAN,
														new Vector3(-10.0f, -3.0f, -10.0f) ) );
		Gesture switch_places_rl = new Gesture("switch-places-right-leg", (int)ActionManager.actions.switchPlaces);
		switch_places_rl.constraints.Add( new JointConstraint(this,
														Joints.KNEE_RIGHT,
														Joints.HIP_CENTER,
														JointConstraint.Relations.COMPONENT_DISTANCE,
														JointConstraint.Operators.GREATER_THAN,
														new Vector3(-10.0f, -3.0f, -10.0f) ) );
		
//		AnimatedGesture stupid_wave = new AnimatedGesture("stupid-wave", 2);
//		stupid_wave.add_keyframe(right_hand, 1.0f, 0.0f);
//		stupid_wave.add_keyframe(left_hand, 1.0f, 0.0f);
//		animated_gestures.Add("stupid-wave", stupid_wave);
		
		// 2
		gestures.Add("switch-places-ll", switch_places_ll);
		gestures.Add("switch-places-rl", switch_places_rl);
//		gestures.Add( "left-hand-up", left_hand_up );
		gestures.Add("right-hand-up", right_hand_up);
		
		// 4
//		gestures.Add ("left-hand-left", left_hand_left);
//		gestures.Add ("right-hand-left", right_hand_left);
//		gestures.Add ("left-hand-right", left_hand_right);
//		gestures.Add ("right-hand-right", right_hand_right);
		
		
		Mapping aim_x = new Mapping(this, Joints.HAND_LEFT, new Vector3(1, 0, 0));
		aim_x.push_joint(Joints.HIP_CENTER, new Vector3(1, 0, 0), Mapping.Operators.SUBTRACT);
		
		aim_x.set_bound(4.5f);
		aim_x.normalize();
		mappings.Add("aim_x", aim_x );
		
		Mapping aim_y = new Mapping(this, Joints.HAND_LEFT, new Vector3(0, 1, 0));
		aim_y.push_joint(Joints.SHOULDER_RIGHT, new Vector3(0, 1, 0), Mapping.Operators.SUBTRACT);
		
		aim_y.set_bound(4.5f);
		aim_y.normalize();
		mappings.Add("aim_y", aim_y );
	}
	
	private void add_skeleton_joint( Joints joint, GameObject obj ) {
		skeleton.Add (joint, obj);
		inverse_skeleton.Add (obj, joint);
	}
	
	private void OnGUI() {
//		Mapping aim_x;
//		Mapping aim_y;
//		
//		if (mappings.TryGetValue("aim_x", out aim_x)) {
//			GUI.Label(new Rect(20,20,Screen.width-20,Screen.height-20),"aim x = "+aim_x.get_value());
//		}
//		
//		if (mappings.TryGetValue("aim_y", out aim_y)) {
//			GUI.Label(new Rect(20,40,Screen.width-20,Screen.height-20),"aim y = "+aim_y.get_value());
//		}
	}
	
	void OnDrawGizmos() {
	}
	
	// Update is called once per frame
	void Update () {
		joint_data_mutex.WaitOne();
		// loops over each joint and calculates the game position for it
		foreach (KeyValuePair<Joints, GameObject> joint in skeleton) {
				switch (joint.Key) {
				case Joints.HIP_CENTER:
					break;
				default:
					Joints parent_joint;
				
					if (inverse_skeleton.TryGetValue(joint.Value.transform.parent.gameObject, out parent_joint)) {
						Vector3 direction = raw_joint_pos[(int)joint.Key] - raw_joint_pos[(int)parent_joint];
						game_joint_pos[(int)joint.Key] = Vector3.Reflect(direction, Vector3.forward);
					}
					break;
				}
			
			if (tracking_player)
				update_joint(joint.Key, joint.Value);
		}
		
		if (tracking_player)
			check_gestures();
		
		joint_data_mutex.ReleaseMutex();
	}
	
	private void check_gestures() {
		// loops over each gesture and tests if the body is currently in that position.
		foreach (KeyValuePair<string, Gesture> pair in gestures) {
			if (pair.Value.is_active()) {
				print("player="+player+"; active gesture: "+pair.Key+"; action value: "+pair.Value.action);
				
				if (controller != null) {
					controller.PerformAction(pair.Value.action);
				}
			}
		}
		
		// loops over each animated gesture and prints out if one was successfully activated.
		foreach (KeyValuePair<string, AnimatedGesture> pair in animated_gestures) {
			if (pair.Value.activated() && controller != null) {
				//print ("player "+player+" body popped the '"+pair.Key+"' animated gesture!");
				
				if (controller != null) {
					controller.PerformAction(pair.Value.action);
				}
			}
		}
		
		// aim x and y for aiming/steering the vehicle.
		Mapping aim_x;
		Mapping aim_y;
		if (controller != null && mappings.TryGetValue("aim_x", out aim_x) && mappings.TryGetValue("aim_y", out aim_y)) {
			controller.UpdateAimSteer(aim_x.get_value(), aim_y.get_value());
//			print (controller.IsDriver()+" - "+aim_x.get_value());
		}
	}
	
	// updates joints
	void update_joint(KinectSkeleton.Joints joint_type, GameObject joint_obj) {
		Quaternion spine_rotation;
		Vector3 direction = game_joint_pos[(int)joint_type];
		
		switch (joint_type) {
		case Joints.HEAD:
			skShoulderCenter.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction)
				* Quaternion.FromToRotation(Vector3.up, new Vector3(0.0f, 1.0f, -0.2f));
			break;
		case Joints.SHOULDER_CENTER:
			skSpine.transform.localRotation = Quaternion.FromToRotation(model_joint_pos[(int)Joints.SHOULDER_CENTER], direction);
			break;
		case Joints.ELBOW_LEFT:
			spine_rotation = Quaternion.FromToRotation(model_joint_pos[(int)Joints.SPINE], game_joint_pos[(int)Joints.SPINE]);
			spine_rotation.w /= 0.5f;
			spine_rotation *= Quaternion.FromToRotation(Vector3.up, new Vector3(0.0f, 1.0f, 0.2f));
			
			skLeftShoulder.transform.localRotation = Quaternion.Inverse(spine_rotation)
				* Quaternion.Inverse(Quaternion.FromToRotation(model_joint_pos[(int)Joints.SHOULDER_CENTER], game_joint_pos[(int)Joints.SHOULDER_CENTER]))
				* Quaternion.FromToRotation(model_joint_pos[(int)Joints.ELBOW_LEFT], direction);
			
			break;
		case Joints.WRIST_LEFT:
			skLeftElbow.transform.localRotation = Quaternion.FromToRotation(game_joint_pos[(int)Joints.ELBOW_LEFT], direction);
			break;
		case Joints.HAND_LEFT:
			skLeftWrist.transform.localRotation = Quaternion.FromToRotation(game_joint_pos[(int)Joints.WRIST_LEFT], direction);
			break;
		case Joints.ELBOW_RIGHT:
			spine_rotation = Quaternion.FromToRotation(model_joint_pos[(int)Joints.SPINE], game_joint_pos[(int)Joints.SPINE]);
			spine_rotation.w /= 0.5f;
			spine_rotation *= Quaternion.FromToRotation(Vector3.up, new Vector3(0.0f, 1.0f, 0.2f));
			
			skRightShoulder.transform.localRotation = Quaternion.Inverse(spine_rotation)
				* Quaternion.Inverse(Quaternion.FromToRotation(model_joint_pos[(int)Joints.SHOULDER_CENTER], game_joint_pos[(int)Joints.SHOULDER_CENTER]))
				* Quaternion.FromToRotation(model_joint_pos[(int)Joints.ELBOW_RIGHT], direction);
			
			
			break;
		case Joints.WRIST_RIGHT:
			skRightElbow.transform.localRotation = Quaternion.FromToRotation(game_joint_pos[(int)Joints.ELBOW_RIGHT], direction);
			break;
		case Joints.HAND_RIGHT:
			skRightWrist.transform.localRotation = Quaternion.FromToRotation(game_joint_pos[(int)Joints.WRIST_RIGHT], direction);
			break;
		case Joints.SPINE:
			Quaternion base_rotation = Quaternion.FromToRotation(model_joint_pos[(int)Joints.SPINE], direction);
			base_rotation.w /= 0.5f;
			skHipCenter.transform.localRotation = base_rotation
				* Quaternion.FromToRotation(Vector3.up, new Vector3(0.0f, 1.0f, 0.9f));
			break;
		default:
			break;
		}
	}
	
	// main thread function for updating the joint position data
	void run_joint_data_update() {
		while (is_running) {
			// fetches the udp player data
			string raw_joint_data = client.read_data(player, 1);
			
			// wait for a lock on the joint data mutex to update the data
			joint_data_mutex.WaitOne();
			if (raw_joint_data != "no player" && raw_joint_data != "silent") {
				parse_joint_data(raw_joint_data);
				tracking_player = true;
			} else {
				tracking_player = false;
			}
			joint_data_mutex.ReleaseMutex();
		}
	}
	
	// called when the application quits. this is mainly so we can tell the kinect skeleton server to quit.
	void OnApplicationQuit() {
		is_running = false;
		
		if (thr_joint_data_update != null)
			thr_joint_data_update.Join();
	}
	
	// parses the joint data from the udp broadcast from the kinect skeleton server.
	void parse_joint_data(string raw_data) {
		float x, y, z;
		string[] joint_vals = raw_data.Split(';');
		
		for (int i = 0; i < (int)Joints.COUNT; i++) {
			string[] str_floats = joint_vals[i].Split(',');
			x = float.Parse(str_floats[0]) * scale_factor;
			y = float.Parse(str_floats[1]) * scale_factor;
			z = float.Parse(str_floats[2]) * scale_factor;
			raw_joint_pos[i] = new Vector3(x, y, z);
		}
	}
	
	
	public Vector3 parse_vector3_string(string str) {
//		print (str);
//		float x, y, z;
//		string[] parts = str.Split(',');
//		x = float.Parse(parts[0]);
//		y = float.Parse(parts[1]);
//		z = float.Parse(parts[2]);
//		
//		return new Vector3(x, y, z);
		return new Vector3(0.0f, 0.0f, 0.0f);
	}
	
	public Vector3 get_raw_joint(Joints joint) {
		return raw_joint_pos[(int)joint];
	}
	
	// JSON related functions
//	public void serialize_object_to_file( System.Object object ) {
//		
//	}
	
	public void write_string_to_file(string path, string str) {
		StreamWriter sw = new StreamWriter(path);
		sw.Write(str);
		sw.Close();
	}
	
	public string read_file_as_string(string path) {
		try {
            using (StreamReader sr = new StreamReader(path)) {
                string contents = sr.ReadToEnd();
                return contents;
            }
        } catch (Exception e) {
            print ("The file could not be read:"+e.Message);
			return "";
        }
	}
	
	// joints enumeration. it is important that these are kept in alphabetical order
	// to ensure that they align correctly with the joint ordering in the Kinect
	// skeleton server.
	public enum Joints {
        ANKLE_LEFT = 0,
        ANKLE_RIGHT,
        ELBOW_LEFT,
        ELBOW_RIGHT,
        FOOT_LEFT,
        FOOT_RIGHT,
        HAND_LEFT,
        HAND_RIGHT,
        HEAD,
        HIP_CENTER,
        HIP_LEFT,
        HIP_RIGHT,
        KNEE_LEFT,
        KNEE_RIGHT,
        SHOULDER_CENTER,
        SHOULDER_LEFT,
        SHOULDER_RIGHT,
        SPINE,
        WRIST_LEFT,
        WRIST_RIGHT,
        COUNT
    };
	
}
