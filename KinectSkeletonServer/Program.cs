using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Net;
using System.Net.Sockets;

using Microsoft.Kinect;

namespace KinectSkeletonServer {
	class Program {
		private StreamWriter log;

		private KinectSensor sensor;
		private Skeleton[] skeletons;

		private List<int> player_skids;
		private bool untracked_player = true;

		private long player_timeout = 1000;
		private List<long> player_last_seen;

		private int no_players_to_track = 2;
		private bool swap_ports = false;

		// ports for the udp broadcast
		public static int port = 31000;

		// sockets and endpoints
		private Socket sock_pj1;
		private Socket sock_pj2;
		private Socket sock_po1;
		private Socket sock_po2;
		private IPEndPoint addr_pj1;
		private IPEndPoint addr_pj2;
		private IPEndPoint addr_po1;
		private IPEndPoint addr_po2;

		/*******************************************************************************************
		 *		Constructor
		 ******************************************************************************************/
		public Program() {
			log = new StreamWriter("kinect-server.log");
			string options = read_file_as_string("server.properties");

			if (options == "<failed>")
				Environment.FailFast("Failed to open server.properties file.");

			//Console.WriteLine(options);
			string target_ip = options.Split('\n')[0].Trim();
			swap_ports = options.Split('\n')[1].Trim() == "true" ? true : false;
			no_players_to_track = Convert.ToInt32(options.Split('\n')[2].Trim());

			//Console.WriteLine(swap_ports);

			player_skids = new List<int>(2);
			player_skids.Insert(0,-1);
			player_skids.Insert(1,-1);

			player_last_seen = new List<long>(2);
			player_last_seen.Insert(0,0);
			player_last_seen.Insert(1,0);

			// my ip address = 169.254.28.165
			// beas address  = 169.254.19.42

			sock_pj1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sock_pj2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sock_po1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sock_po2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			IPAddress target = IPAddress.Parse(target_ip);

			// we want the ports swapped just if we are sending from the remote server over the lan
			if (swap_ports) {
				addr_pj1 = new IPEndPoint(target, port+2);
				addr_po1 = new IPEndPoint(target, port+4);

				addr_pj2 = new IPEndPoint(target, port+1);
				addr_po2 = new IPEndPoint(target, port+3);
			} else {
				addr_pj1 = new IPEndPoint(target, port+1);
				addr_po1 = new IPEndPoint(target, port+3);

				addr_pj2 = new IPEndPoint(target, port+2);
				addr_po2 = new IPEndPoint(target, port+4);
			}
		}

		/*******************************************************************************************
		 *		Starting and stopping the sensor
		 ******************************************************************************************/
		// connects the kinect sensor to the program.
		private void connect_sensor() {
			foreach (var potentialSensor in KinectSensor.KinectSensors) {
				if (potentialSensor.Status == KinectStatus.Connected) {
					this.sensor = potentialSensor;
					break;
				}
			}

			if (this.sensor != null) {
				// Turn on the skeleton stream to receive skeleton frames
				this.sensor.SkeletonStream.Enable();

				// Add an event handler to be called whenever there is new color frame data
				this.sensor.SkeletonFrameReady += this.cb_sensor_skeleton_frame_ready;

				// Start the sensor!
				try {
					this.sensor.Start();
					Console.WriteLine("ready.");
					log.WriteLine(DateTime.Now+":> Sensor ready.");
					log.Flush();
				} catch (IOException) {
					this.sensor = null;
				}
			} 

			if (this.sensor == null) {
				Console.WriteLine("no sensor.");
				log.WriteLine(DateTime.Now+":> No free sensor." );
				log.Flush();
			}
		}

		/*******************************************************************************************
		 *		Skeleton frame callback
		 ******************************************************************************************/
		private void cb_sensor_skeleton_frame_ready( object sender, SkeletonFrameReadyEventArgs e ) {
			//Skeleton[] skeletons = new Skeleton[0];
			//Console.WriteLine("received frame.");

			using (SkeletonFrame skeleton_frame = e.OpenSkeletonFrame()) {
				if (skeleton_frame != null) {
					// copies the skeleton data to the class data holder.
					this.skeletons = new Skeleton[skeleton_frame.SkeletonArrayLength];
					skeleton_frame.CopySkeletonDataTo( this.skeletons );

					cb_refresh_player_timeouts(skeleton_frame.Timestamp);
					cb_reset_player_skid();

					//get_joint(get_player_skid(1), "WristLeft");
					udp_broadcast();
				}
			}
		}

		/*******************************************************************************************
		 *		Public get joint/boneorientation function
		 ******************************************************************************************/
		// fetches the joint given a skeleton id and string for the joint name
		public Joint get_joint( int skid, string joint_name ) {
			//if (true == false) {
			if (skid != -1) {
				foreach (Skeleton skeleton in this.skeletons) {
					if (	skeleton.TrackingState	== SkeletonTrackingState.Tracked &&
							skeleton.TrackingId		== skid ) {
						
						switch (joint_name) {
							case "AnkleLeft":
								return skeleton.Joints[JointType.AnkleLeft];
							case "AnkleRight":
								return skeleton.Joints[JointType.AnkleRight];
							case "ElbowLeft":
								return skeleton.Joints[JointType.ElbowLeft];
							case "ElbowRight":
								return skeleton.Joints[JointType.ElbowRight];
							case "FootLeft":
								return skeleton.Joints[JointType.FootLeft];
							case "FootRight":
								return skeleton.Joints[JointType.FootRight];
							case "HandLeft":
								return skeleton.Joints[JointType.HandLeft];
							case "HandRight":
								return skeleton.Joints[JointType.HandRight];
							case "Head":
								return skeleton.Joints[JointType.Head];
							case "HipCenter":
								return skeleton.Joints[JointType.HipCenter];
							case "HipLeft":
								return skeleton.Joints[JointType.HipLeft];
							case "HipRight":
								return skeleton.Joints[JointType.HipRight];
							case "KneeLeft":
								return skeleton.Joints[JointType.KneeLeft];
							case "KneeRight":
								return skeleton.Joints[JointType.KneeRight];
							case "ShoulderCenter":
								return skeleton.Joints[JointType.ShoulderCenter];
							case "ShoulderLeft":
								return skeleton.Joints[JointType.ShoulderLeft];
							case "ShoulderRight":
								return skeleton.Joints[JointType.ShoulderRight];
							case "Spine":
								return skeleton.Joints[JointType.Spine];
							case "WristLeft":
								return skeleton.Joints[JointType.WristLeft];
							case "WristRight":
								return skeleton.Joints[JointType.WristRight];
						}
					}
				}
			}

			Joint bad_joint = new Joint();
			bad_joint.TrackingState = JointTrackingState.NotTracked; 
			return bad_joint;
		}

		// fetches the bone orientation given a skeleton id and string for the joint name
		public BoneOrientation get_bone( int skid, string joint_name ) {
			if (skid != -1) {
				foreach (Skeleton skeleton in this.skeletons) {
					if (	skeleton.TrackingState	== SkeletonTrackingState.Tracked &&
							skeleton.TrackingId		== skid ) {
						
						switch (joint_name) {
							case "AnkleLeft":
								return skeleton.BoneOrientations[JointType.AnkleLeft];
							case "AnkleRight":
								return skeleton.BoneOrientations[JointType.AnkleRight];
							case "ElbowLeft":
								return skeleton.BoneOrientations[JointType.ElbowLeft];
							case "ElbowRight":
								return skeleton.BoneOrientations[JointType.ElbowRight];
							case "FootLeft":
								return skeleton.BoneOrientations[JointType.FootLeft];
							case "FootRight":
								return skeleton.BoneOrientations[JointType.FootRight];
							case "HandLeft":
								return skeleton.BoneOrientations[JointType.HandLeft];
							case "HandRight":
								return skeleton.BoneOrientations[JointType.HandRight];
							case "Head":
								return skeleton.BoneOrientations[JointType.Head];
							case "HipCenter":
								return skeleton.BoneOrientations[JointType.HipCenter];
							case "HipLeft":
								return skeleton.BoneOrientations[JointType.HipLeft];
							case "HipRight":
								return skeleton.BoneOrientations[JointType.HipRight];
							case "KneeLeft":
								return skeleton.BoneOrientations[JointType.KneeLeft];
							case "KneeRight":
								return skeleton.BoneOrientations[JointType.KneeRight];
							case "ShoulderCenter":
								return skeleton.BoneOrientations[JointType.ShoulderCenter];
							case "ShoulderLeft":
								return skeleton.BoneOrientations[JointType.ShoulderLeft];
							case "ShoulderRight":
								return skeleton.BoneOrientations[JointType.ShoulderRight];
							case "Spine":
								return skeleton.BoneOrientations[JointType.Spine];
							case "WristLeft":
								return skeleton.BoneOrientations[JointType.WristLeft];
							case "WristRight":
								return skeleton.BoneOrientations[JointType.WristRight];
						}
					}
				}
			}

			BoneOrientation bad_bone = new BoneOrientation(JointType.HipCenter);
			return bad_bone;
		}		

		/*******************************************************************************************
		 *		Public to string functions.
		 ******************************************************************************************/
		public string get_bone_orientation( BoneOrientation bo ) {
			//return bo.HierarchicalRotation.Quaternion.W+","+
			//		bo.HierarchicalRotation.Quaternion.X+","+
			//		bo.HierarchicalRotation.Quaternion.Y+","+
			//		bo.HierarchicalRotation.Quaternion.Z;

			return bo.AbsoluteRotation.Quaternion.W+","+
					bo.AbsoluteRotation.Quaternion.X+","+
					bo.AbsoluteRotation.Quaternion.Y+","+
					bo.AbsoluteRotation.Quaternion.Z;
		}

		public string get_joint_position( Joint jnt ) {
			return jnt.Position.X+","+jnt.Position.Y+","+jnt.Position.Z;
		}

		// returns a string containing all the joint position information for a player
		public string get_skeleton_joint_positions(int skid) {
			if (skid != -1) {
				foreach (Skeleton skeleton in this.skeletons) {
					if (	skeleton.TrackingState	== SkeletonTrackingState.Tracked &&
							skeleton.TrackingId		== skid ) {

						StringBuilder data = new StringBuilder();

						data.Append(get_joint_position(skeleton.Joints[JointType.AnkleLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.AnkleRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.ElbowLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.ElbowRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.FootLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.FootRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.HandLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.HandRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.Head])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.HipCenter])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.HipLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.HipRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.KneeLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.KneeRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.ShoulderCenter])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.ShoulderLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.ShoulderRight])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.Spine])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.WristLeft])+";");
						data.Append(get_joint_position(skeleton.Joints[JointType.WristRight])+";");

						return data.ToString();
					}
				}
			}
			return "no player";
		}

		// returns a string containing all the joint position information for a player
		public string get_skeleton_bone_orientations(int skid) {
			if (skid != -1) {
				foreach (Skeleton skeleton in this.skeletons) {
					if (	skeleton.TrackingState	== SkeletonTrackingState.Tracked &&
							skeleton.TrackingId		== skid ) {

						StringBuilder data = new StringBuilder();

						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.AnkleLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.AnkleRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.ElbowLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.ElbowRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.FootLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.FootRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.HandLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.HandRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.Head])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.HipCenter])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.HipLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.HipRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.KneeLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.KneeRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.ShoulderCenter])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.ShoulderLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.ShoulderRight])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.Spine])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.WristLeft])+";");
						data.Append(get_bone_orientation(skeleton.BoneOrientations[JointType.WristRight])+";");

						return data.ToString();
					}
				}
			}
			return "no player";
		}

		/*******************************************************************************************
		 *		Player skids
		 ******************************************************************************************/
		// fetches the players skid. note that the input argument starts at 1, not 0 like the
		// list indexing for the list datastructure. this is taken care off in the function
		public int get_player_skid( int player ) {
			if (player >= 1 && player <= 2) {
				return this.player_skids[player-1];
			} else {
				return -1;
			}
		}

		// resets a players id to be any of the currently active skeletons in the scene. it can
		// be used even if the second player is still in the scene. best not to try and recognise
		// two players in the scene at the same time.
		public void reset_player_skid( int player ) {
			if (player >= 1 && player <= 2) {

				int other_player = player % 2;
				log.WriteLine(DateTime.Now+":> player="+(player-1)+"  other_player="+other_player);
				log.Flush();

				player_skids[player-1] = -1;
			}
		}

		// callback function to reset the players skeleton id.
		public void cb_reset_player_skid() {
			if (untracked_player) {
				foreach (Skeleton skeleton in this.skeletons) {
					if (skeleton.TrackingState == SkeletonTrackingState.Tracked) {
						for (int i = 0; i < player_skids.Count; i++) {
							if (player_skids[i] == -1 && !player_skids.Contains(skeleton.TrackingId)) {
								player_skids[i] = skeleton.TrackingId;
								log.WriteLine(DateTime.Now+":> Tracking player: "+(i+1)+" => "+player_skids[i]);
								log.Flush();
							}
						}
					}
				}

				if (!player_skids.Contains(-1)) {
					untracked_player = false;
				}
			}
		}

		// refreshes the player last seen array and if nessecary clears the associated skid with
		// that player (if the last time that skid was seen was longer than the player timeout)
		public void cb_refresh_player_timeouts( long time ) {
			for (int i = 0; i < player_last_seen.Count; i++) {
				if (player_skids[i] != -1) {
					foreach (Skeleton skeleton in this.skeletons) {
						if (player_skids[i] == skeleton.TrackingId) {
							player_last_seen[i] = time;
						}
					}

					if (time - player_last_seen[i] > player_timeout) {
						player_skids[i] = -1;
						untracked_player = true;
						log.WriteLine(DateTime.Now+":> player "+(i+1)+" timed out.");
						log.Flush();
					}
				}
			}
		}

		/*******************************************************************************************
		 *		Socket functions
		 ******************************************************************************************/
		private void udp_broadcast() {
			byte[] buffer1 = Encoding.ASCII.GetBytes(get_skeleton_joint_positions(get_player_skid(1)));
			byte[] buffer3 = Encoding.ASCII.GetBytes(get_skeleton_bone_orientations(get_player_skid(1)));
			byte[] buffer2 = Encoding.ASCII.GetBytes("");
			byte[] buffer4 = Encoding.ASCII.GetBytes("");

			if (no_players_to_track > 1) {
				buffer2 = Encoding.ASCII.GetBytes(get_skeleton_joint_positions(get_player_skid(2)));
				buffer4 = Encoding.ASCII.GetBytes(get_skeleton_bone_orientations(get_player_skid(2)));
			}
			
			try {
				sock_pj1.SendTo(buffer1, addr_pj1);
				sock_po1.SendTo(buffer3, addr_po1);
				
				if (no_players_to_track > 1) {
					sock_pj2.SendTo(buffer2, addr_pj2);
					sock_po2.SendTo(buffer4, addr_po2);
				}
			} catch (Exception) {
			}
		}

		/*******************************************************************************************
		 *		Main program run function
		 ******************************************************************************************/
		// loops over query commands sent to the program. can be tested from the command line as this just uses the standard
		// input and output streams and all the commands are in string format.
		private void run() {
			connect_sensor();

			while (true) {
				string cmd = Console.ReadLine();

				if (cmd.Contains("exit")) {
					break;
				} else if (cmd.Contains("p1")) {
					Console.WriteLine(get_skeleton_joint_positions(get_player_skid(1)));
				} else if (cmd.Contains("p2")) {
					Console.WriteLine(get_skeleton_joint_positions(get_player_skid(2)));
				} else if (cmd.Contains("po1")) {
					Console.WriteLine(get_skeleton_bone_orientations(get_player_skid(1)));
				} else if (cmd.Contains("po2")) {
					Console.WriteLine(get_skeleton_bone_orientations(get_player_skid(2)));
				}
				System.Threading.Thread.Sleep(50);
			}

			log.Close();
		}

		public static bool str_starts_with(string str, string substr) {
			if (str.Length >= substr.Length) {
				if (str.Substring(0, substr.Length) == substr) {
					return true;
				}
			}
			return false;
		}

		// read a file as a string
		public string read_file_as_string(string path) {
			try {
				using (StreamReader sr = new StreamReader(path)) {
					string contents = sr.ReadToEnd();
					return contents;
				}
			} catch (Exception e) {
				Console.WriteLine("The file could not be read: "+e.Message);
				return "<failed>";
			}
		}

		/*******************************************************************************************
		 *		Program main
		 ******************************************************************************************/
		// the program main. this just creates an instance of this program class and runs it.
		static void Main( string[] args ) {
			Program server = new Program();
			server.run();
		}
	}
}
