using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapping {
	internal KinectSkeleton ks = null;
	private List<KinectSkeleton.Joints> joints = new List<KinectSkeleton.Joints>(2);
	private List<Vector3> jc_weights = new List<Vector3>(2);
//	private List<int> weightings = new List<int>(2);
	private List<Operators> operators = new List<Operators>(1);
	
	private bool has_upperbound = false;
	private bool has_lowerbound = false;
	private float upperbound = 0.0f;
	private float lowerbound = 0.0f;
	
//	private float scale_factor_pos = 1.0f;
//	private float scale_factor_neg = 1.0f;
	private bool normalized = false;
	
	public Mapping(KinectSkeleton ks, KinectSkeleton.Joints joint, Vector3 weights) {
		this.ks = ks;
		joints.Add(joint);
		jc_weights.Add(weights);
	}
	
	// get the mapping functions value.
	public float get_value() {
		float val = 0.0f;
		Vector3 vjoint = ks.get_raw_joint(joints[0]);
		val += vjoint.x*jc_weights[0].x + vjoint.y*jc_weights[0].y + vjoint.z*jc_weights[0].z;
		
		for (int i = 1; i < joints.Count; i++) {
			vjoint = ks.get_raw_joint(joints[i]);
			
			switch (operators[i-1]) {
			case Operators.ADD:
				val += vjoint.x*jc_weights[i].x + vjoint.y*jc_weights[i].y + vjoint.z*jc_weights[i].z;
				break;
			case Operators.SUBTRACT:
				val -= vjoint.x*jc_weights[i].x + vjoint.y*jc_weights[i].y + vjoint.z*jc_weights[i].z;
				break;
			}
		}
		
		if (has_upperbound && val > upperbound)
			val = upperbound;
		
		if (has_lowerbound && val < lowerbound)
			val = lowerbound;
		
		if (normalized && has_lowerbound && has_upperbound) {
			val = (val / (upperbound - lowerbound)) + 0.5f;
		}
		
		return val;
	}
	
	// adds a joint
	public void push_joint(KinectSkeleton.Joints joint, Vector3 weights, Operators operation) {
		joints.Add(joint);
		jc_weights.Add(weights);
		operators.Add(operation);
	}
	
	//		set the upper and lower bounds
	// sets/clears the upper bound.
	public void set_upperbound(float bound) {
		has_upperbound = true;
		upperbound = bound;
	}
	
	public void no_upperbound() {
		has_upperbound = false;
	}
	
	// sets/clears the lower bound.
	public void set_lowerbound(float bound) {
		has_lowerbound = true;
		lowerbound = bound;
	}
	
	public void no_lowerbound() {
		has_lowerbound = false;
	}
	
	// shorthand function to set the upper and lower bound in 1 function
	public void set_bound(float bound) {
		set_lowerbound(-bound);
		set_upperbound(bound);
	}
	
	// turn on normalization
	public void normalize() {
		normalized = true;
	}
	
	public enum Operators {
		ADD = 0,
		SUBTRACT
	}
}
