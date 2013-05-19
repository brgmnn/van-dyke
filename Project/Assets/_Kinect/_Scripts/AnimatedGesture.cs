using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedGesture : Serialize.ReadableSerialize  {
	
	public string name;
	public int action;
	private List<Gesture> gestures = new List<Gesture>();
	private List<float> time_windows = new List<float>();
	private List<float> time_offsets = new List<float>();
	
	private int index = 0;
	public Gesture current_gesture = null;
	public float time_offset_remaining = 0.0f;
	public float time_window_remaining = 0.0f;
	
	// constructors
	public AnimatedGesture( string name, int action ) {
		this.name = name;
		this.action = action;
	}
	
	// add a new gesture keyframe to the end of this animation series.
	public void add_keyframe( Gesture gesture, float window, float offset ) {
		gestures.Add(gesture);
		time_windows.Add(window);
		time_offsets.Add(offset);
		
		if (gestures.Count == 0) {
			current_gesture = gesture;
			time_offset_remaining = offset;
			time_window_remaining = window;
		}
	}
	
	// move to this keyframe and begin recognising it immediately
	private void move_to_keyframe( int index ) {
		move_to_keyframe(index, false);
	}
	
	private void move_to_keyframe( int index, bool with_offset ) {
		if (index >= 0 && index < gestures.Count) {
			this.index = index;
			current_gesture = gestures[index];
			time_offset_remaining = with_offset ? time_offsets[index] : 0.0f;
			time_window_remaining = time_windows[index];
		}
	}
	
	// check if this animated gesture was activated.
	public bool activated() {
		// if we still have time to activate the gesture
		if (time_window_remaining > 0.0f) {
			// if we are in the active position
			if (gestures[index].is_active()) {
				// if it was the last gesture in the sequence
				if (index+1 == gestures.Count) {
					move_to_keyframe(0, true);
					return true; // congrats! you just did an animated gesture!
				}
				// guess there are more moves to do...
				move_to_keyframe(++index, true);
				return false;
			}
			// we aren't in the active position. just keep ticking the clock
			time_window_remaining -= Time.deltaTime;
		} else if (time_offset_remaining > 0.0f) {
			// if we are in the offset window where we cannot trigger the next gesture
			time_offset_remaining -= Time.deltaTime;
		} else {
			// we timed out. reset the animated gesture.
			move_to_keyframe(0, true);
		}
		return false;
	}
	
	//	Serialization
	// serialize the object to a string
	public override string serialize () {
		return "";
	}
	
	internal override string readable_serialize (int indentation_level) {
		StringBuilder buffer = new StringBuilder();
		buffer.Append("{\n");
		
		string sa, sb, sd, sf, sh, si;
		sa = "\"name\": \""+name+"\",\n";
		buffer.Append(sa.PadLeft(sa.Length+indentation_level+4, ' '));
		
		sb = "\"gestures\": [";
		buffer.Append(sb.PadLeft(sb.Length+indentation_level+4, ' '));
		
		for (int i = 0; i < gestures.Count; i++) {
			string sc;
			if (i < gestures.Count-1) {
				sc = gestures[i].readable_serialize(indentation_level+4)+", ";
			} else {
				sc = gestures[i].readable_serialize(indentation_level+4);
			}
			buffer.Append(sc);
		}
		
		buffer.Append("],\n");
		
		sd = "\"time_windows\": [\n";
		buffer.Append(sd.PadLeft(sd.Length+indentation_level+4, ' '));
		
		for (int i = 0; i < time_windows.Count; i++) {
			string se;
			if (i < time_windows.Count-1) {
				se = time_windows[i]+", \n";
			} else {
				se = time_windows[i]+"\n";
			}
			buffer.Append(se.PadLeft(se.Length+indentation_level+8, ' '));
		}
		
		buffer.Append("],\n".PadLeft(indentation_level+7, ' '));
		
		
		sf = "\"time_offsets\": [\n";
		buffer.Append(sf.PadLeft(sf.Length+indentation_level+4, ' '));
		
		for (int i = 0; i < time_offsets.Count; i++) {
			string sg;
			if (i < time_offsets.Count-1) {
				sg = time_offsets[i]+", \n";
			} else {
				sg = time_offsets[i]+"\n";
			}
			buffer.Append(sg.PadLeft(sg.Length+indentation_level+8, ' '));
		}
		
		buffer.Append("],\n".PadLeft(indentation_level+7, ' '));
		
		sh = "\"index\": "+index+",\n";
		buffer.Append(sh.PadLeft(sh.Length+indentation_level+4, ' '));
		
		si = "\"time_window_remaining\": "+time_window_remaining+",\n";
		buffer.Append(si.PadLeft(si.Length+indentation_level+4, ' '));
		
		string sj = "\"time_offset_remaining\": "+time_offset_remaining+",\n";
		buffer.Append(sj.PadLeft(sj.Length+indentation_level+4, ' '));
		
		buffer.Append("}".PadLeft(indentation_level+1, ' '));
		return buffer.ToString();
	}
	
	public override void deserialize (string str) {
	}
}
