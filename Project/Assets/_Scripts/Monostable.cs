using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monostable {
	private float timeout = 0.0f;
	private float last_set = 0.0f;
	
	public Monostable(float timeout) {
		this.timeout = timeout;
	}
	
	public void set() {
		last_set = Time.fixedTime;
	}
	
	public bool get() {
		if (last_set >= Time.fixedTime-timeout)
			return true;
		else
			return false;
	}
}
