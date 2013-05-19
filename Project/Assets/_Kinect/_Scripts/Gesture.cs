using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture : Serialize.ReadableSerialize {

	public string name;
	public int action;
	public List<JointConstraint> constraints = new List<JointConstraint>();
	
	// constructors
	public Gesture( string name, int action ) {
		this.name = name;
		this.action = action;
	}
	
	// check if the gesture is active
	public bool is_active() {
		bool active = true;
			
		// check all the joint constraints are true, fail fast on a false joint constraint.
		foreach (JointConstraint constraint in this.constraints) {
			if ( !constraint.check() ) {
				active = false;
				break;
			}
		}
		
		return active;
	}
	
	//	serialization
	// serialize the data
	public override string serialize() {
		return "";
	}
	
	// serialize the data in a human readable format
	internal override string readable_serialize (int indentation_level) {
		StringBuilder buffer = new StringBuilder();
		buffer.Append("{\n");
		
		string sa, sb;
		sa = "\"name\": \""+name+"\",\n";
		sb = "\"constraints\": [";
		
		buffer.Append(sa.PadLeft(sa.Length+indentation_level+4, ' '));
		buffer.Append(sb.PadLeft(sb.Length+indentation_level+4, ' '));
		
		for (int i = 0; i < constraints.Count; i++) {
			string sc;
			if (i < constraints.Count-1) {
				sc = constraints[i].readable_serialize(indentation_level+4)+", ";
			} else {
				sc = constraints[i].readable_serialize(indentation_level+4);
			}
			
			buffer.Append(sc);
		}
		buffer.Append ("]\n");
		buffer.Append("}".PadLeft(indentation_level+1, ' '));
		return buffer.ToString();
	}
	
	// deserialize a string
	public override void deserialize (string str) {
	}
}
