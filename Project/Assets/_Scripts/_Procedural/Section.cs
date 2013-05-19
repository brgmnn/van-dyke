using UnityEngine;
using System.Collections;

public class Section {
	public Vector3 position;
	public GameObject obj;
	public Vector3[] waypoints;
	public int waypointCount;
	SectionProperties sp;
	
	//Constructor
	public Section(GameObject obj_in, string name, Vector3 pos, Quaternion rot){
		this.obj = (GameObject) GameObject.Instantiate(obj_in, pos, rot);
		this.obj.name = name;
		this.sp = obj.GetComponent<SectionProperties>();
		this.waypointCount = this.sp.wayPointsObj.transform.childCount;
		InitializeWaypoints();
	}
	
	
	void InitializeWaypoints(){
		waypoints = new Vector3[this.waypointCount];
		for(int i=1; i<=this.waypointCount;i++){
			Vector3 l = this.sp.wayPointsObj.transform.Find("L"+i).position;
			waypoints[i-1] = l;
		}
	}
	
	public Quaternion nextSectionRotation(){
		Transform endPoint = this.sp.endPointObj.transform;
		if(endPoint == null){
			return Quaternion.identity;
		}
		Quaternion angle = endPoint.rotation;
		return angle;
	}
	
	public Vector3 nextSectionPosition(Vector3 offset){
		Transform endPoint = this.sp.endPointObj.transform;
		return endPoint.position;
	}
	
}
