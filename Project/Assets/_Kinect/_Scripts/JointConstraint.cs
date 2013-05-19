using UnityEngine;
using System;
using System.Collections;
using System.Text;

public class JointConstraint : Serialize.ReadableSerialize {
	private KinectSkeleton ks = null;
	
	public KinectSkeleton.Joints joint_a;
	public KinectSkeleton.Joints joint_b;
	public Vector3 val;
	public Relations relation;
	public Operators operation;
	
	// constructors
	public JointConstraint( KinectSkeleton ks ) {
		this.ks = ks;
	}
	
	public JointConstraint( KinectSkeleton ks, KinectSkeleton.Joints joint_a, KinectSkeleton.Joints joint_b ) {
		this.ks = ks;
		this.joint_a = joint_a;
		this.joint_b = joint_b;
	}
	
	public JointConstraint( KinectSkeleton ks,
							KinectSkeleton.Joints joint_a,
							KinectSkeleton.Joints joint_b,
							Relations relation,
							Operators operation,
							Vector3 val ) {
		this.ks = ks;
		this.joint_a = joint_a;
		this.joint_b = joint_b;
		this.relation = relation;
		this.operation = operation;
		this.val = val;
	}
	
	// joint constraint check
	public bool check() {
		Vector3 ja, jb;
		
		switch (relation) {
		case JointConstraint.Relations.DISTANCE:
			switch (operation) {
			case JointConstraint.Operators.GREATER_THAN:
				return Vector3.Distance(ks.raw_joint_pos[(int)joint_a], ks.raw_joint_pos[(int)joint_b]) > val.x;
			case JointConstraint.Operators.LESS_THAN:
				return Vector3.Distance(ks.raw_joint_pos[(int)joint_a], ks.raw_joint_pos[(int)joint_b]) < val.x;
			default:
				return false;
			}
		case JointConstraint.Relations.COMPONENT_DISTANCE:
			ja = ks.raw_joint_pos[(int)joint_a];
			jb = ks.raw_joint_pos[(int)joint_b];
			
			switch (operation) {
			case JointConstraint.Operators.GREATER_THAN:
				return (ja.x - jb.x) > val.x && (ja.y - jb.y) > val.y && (ja.z - jb.z) > val.z;
			case JointConstraint.Operators.LESS_THAN:
				return (ja.x - jb.x) < val.x && (ja.y - jb.y) < val.y && (ja.z - jb.z) < val.z;
			default:
				return false;
			}
		case JointConstraint.Relations.ABS_COMPONENT_DISTANCE:
			ja = ks.raw_joint_pos[(int)joint_a];
			jb = ks.raw_joint_pos[(int)joint_b];
			
			switch (operation) {
			case JointConstraint.Operators.GREATER_THAN:
				return Math.Abs(ja.x - jb.x) > val.x && Math.Abs(ja.y - jb.y) > val.y && Math.Abs(ja.z - jb.z) > val.z;
			case JointConstraint.Operators.LESS_THAN:
				return Math.Abs(ja.x - jb.x) < val.x && Math.Abs(ja.y - jb.y) < val.y && Math.Abs(ja.z - jb.z) < val.z;
			default:
				return false;
			}
		default:
			return false;
		}
	}
	
	// Needed enumerations for this class.
	// firstly the constraint relation between the two joints. these are always relative to joint a.
	// when comparing to a value, if only one value is needed, it is stored in the x component. if two
	// are needed then they are stored in the x and y component and if 3 are needed then in the x, y and z
	// component.
	public enum Relations {
		DISTANCE,
		COMPONENT_DISTANCE,
		// bro do you even lift?
		ABS_COMPONENT_DISTANCE
	}
	
	// the operators we can use to test the constraint. these are always relative to val. so it is:
	// a-b = val, a-b != val, a-b > val, a-b < val, a-b >= val, a-b <= val.
	public enum Operators {
		EQUAL = 0,
		NOT_EQUAL,
		GREATER_THAN,
		LESS_THAN,
		GREATER_THAN_OR_EQUAL,
		LESS_THAN_OR_EQUAL
	};
	
	//		serialization methods
	// serialize
	public override string serialize() {
		StringBuilder buffer = new StringBuilder();
		buffer.Append("{");
		
		buffer.Append("\"joint_a\": "+joint_a+",");
		buffer.Append("\"joint_b\": "+joint_b+",");
		buffer.Append("\"operation\": "+operation+",");
		buffer.Append("\"relation\": "+relation+",");
		buffer.Append("\"val\": "+Serialize.Struct.Vector3.serialize(val));
		
		buffer.Append("}");
		return buffer.ToString();
	}
	
	internal override string readable_serialize(int indentation_level) {
		StringBuilder buffer = new StringBuilder();
		buffer.Append("{\n");
		
		string sa, sb, sc, sd, se;
		sa = "\"joint_a\": \""+joint_a+"\",\n";
		sb = "\"joint_b\": \""+joint_b+"\",\n";
		sc = "\"operation\": \""+operation+"\",\n";
		sd = "\"relation\": \""+relation+"\",\n";
		se = "\"val\": "+Serialize.Struct.Vector3.readable_serialize(val, 0)+"\n";
		
		buffer.Append(sa.PadLeft(sa.Length+indentation_level+4, ' '));
		buffer.Append(sb.PadLeft(sb.Length+indentation_level+4, ' '));
		buffer.Append(sc.PadLeft(sc.Length+indentation_level+4, ' '));
		buffer.Append(sd.PadLeft(sd.Length+indentation_level+4, ' '));
		buffer.Append(se.PadLeft(se.Length+indentation_level+4, ' '));
		
		buffer.Append("}".PadLeft(indentation_level+1, ' '));
		return buffer.ToString();
	}
	
	// deserialize
	public override void deserialize(string str) {
	}
}
