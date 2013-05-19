using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * SectionManager
 * 
 * Creates and removes sections.
 * Counts waypoints.
 * Returns the n'th waypoint, no matter what n is.
 * 
 * Creation and destruction is completely automated.
 * Simply ask for a waypoint near the end of the array,
 * and SectionManager will add another section after it.
 * 
 */

public class SectionManager : MonoBehaviour {
	static List<GameObject> labSections = new List<GameObject>();
	static List<GameObject> lhcSections = new List<GameObject>();
	static Section[] instantiatedSections;
	static Vector3 sectionOffset = Vector3.zero;
	static Quaternion sectionRotation = Quaternion.identity;
	// Counts instantiated waypoints
	static int totalPoints = 0;
	// Counts de-instantiated waypoints
	static int removedPoints = 0;
	static int totalSections = 0;
	static int removedSections = 0;
	public static bool initialised = false;
	static GameObject sectionHolder;
	
	static GameObject previousSection;
	
	// Useful for SectionProperties to have a place to find Vehicle
	public static GameObject vehicle;
	
	// Records whether we're in Lab or LHC level right now
	static sectionState currentState;
	enum sectionState {
		lab,
		lhc
	}	
	
	// Number of sections that will be instantiated at one time
	static int maximumSections = 50;
	// Number of waypoints to have in front before adding new sections
	static int lookAhead = 10 * (maximumSections/2); 
	static int[] preventSection;

	// Add the given sections from the sections list
	static void AddSection(GameObject section, bool removeLast = true){
		if(removeLast) RemoveLastSection();
		Section newSection = new Section(section, ("S"+(totalSections+1)), sectionOffset, sectionRotation);
		newSection.obj.gameObject.transform.parent = sectionHolder.transform;
		totalPoints += newSection.waypointCount;
		instantiatedSections[totalSections%maximumSections] = newSection;
		sectionOffset = newSection.nextSectionPosition(sectionOffset);
		sectionRotation = Quaternion.identity * newSection.nextSectionRotation();

		totalSections++;
	}
	
	// De-instantiates the last section, removing it from Unity's heirarchy
	static void RemoveLastSection(){
		removedPoints += instantiatedSections[totalSections%maximumSections].waypointCount;
		removedSections++;
		GameObject.Destroy(instantiatedSections[totalSections%maximumSections].obj);
	}
	
	// Retrieve section that n'th waypoint is in
	public static Section GetSection(int wayPoint){
		int remainingPoints = wayPoint - removedPoints;		
		for(var i = 0; i < maximumSections; i++){
			int points = instantiatedSections[(i + removedSections) % maximumSections].waypointCount;
			remainingPoints -= points; 
			Section sec;
			if(remainingPoints < 0){
				sec = instantiatedSections[(i + removedSections) % maximumSections];
				return sec;
			}
		}
		
		return null;
	}
	
	// Retrieve the location of the n'th waypoint in space
	public static Vector3 GetWaypoint(int n){
		if(!initialised) InitialiseSections();
		int remainingPoints = n - removedPoints;

		if(n > totalPoints - lookAhead){
			AddSection(RandomSection());
		}
		
		for(var i = 0; i < maximumSections; i++){
			int points = instantiatedSections[(i + removedSections) % maximumSections].waypointCount;
			remainingPoints -= points; 
			Section sec;
			if(remainingPoints < 0){
				sec = instantiatedSections[(i + removedSections) % maximumSections];
				return sec.waypoints[points + remainingPoints];
			}
		}
		return Vector3.zero;
	}
	
	// Used to reset SectionManager's state on game restart
	public static void Restart(){
		labSections.Clear();
		lhcSections.Clear();
		GameObject sh = GameObject.Find("SectionHolder");
		if(sh){
			GameObject.Destroy(sh);
		}
		 sectionOffset = Vector3.zero;
		 sectionRotation = Quaternion.identity;
		 totalPoints = 0;
		 removedPoints = 0;
		 totalSections = 0;
		 removedSections = 0;
		 initialised = false;
		 maximumSections = 50;
		 lookAhead = 10 * (maximumSections/2); 
		
	}
	
	// Initialises maximumSection number of sections
	public static void InitialiseSections(){
		// Make parent for sections
		sectionHolder = new GameObject("SectionHolder");
		vehicle = GameObject.Find("Vehicle");
		
		// Must start with lab section
		currentState = sectionState.lab;
		
		// Pull in all the Sections in the scene
		GameObject[] availableSections = UnityEngine.GameObject.FindGameObjectsWithTag("Section");
		if(availableSections.Length > 0){
			initialised = true;
		} else {
			initialised = false;
			return;
		}
		foreach(GameObject section in availableSections){
			SectionProperties sp = section.GetComponent<SectionProperties>();
			switch(sp.sectionType){
			case 0:
			case 2:
				labSections.Add(section);
				break;
			case 1:
			case 3:
				lhcSections.Add(section);
				break;
			default:
				Debug.LogError("Unrecognised section type");
				break;
			}
		}
		instantiatedSections = new Section[maximumSections];
		
		// Always start with start section
		
		GameObject startSection = GameObject.FindGameObjectWithTag("Start Section");
		AddSection(startSection,false);
		
		for(int i = 1; i < maximumSections; i++){
			// Don't try and remove previous sections during initialisation
			AddSection(RandomSection(),false);	
		}
	}
	
	// Chooses a random section that is allowed
	static GameObject RandomSection(){
		bool cont = true;
		GameObject prospectiveSection;
		for (int i = 0;i < 10 && cont;i++){
			int r = Random.Range(0,1000);
			// Pick a section from the appropriate list
			if(currentState == sectionState.lab){
				prospectiveSection = labSections[r % labSections.Count];
			} else if(currentState == sectionState.lhc){
				prospectiveSection = lhcSections[r % lhcSections.Count];
			} else {
				Debug.LogError("INVALID STATE");
				return null;	
			}
			
			SectionProperties sp = prospectiveSection.GetComponent<SectionProperties>();
			
			// Don't use the same section twice in a row
			if(prospectiveSection == previousSection){
				continue;
			}
			
			// Make sure we aren't going more than 90 degrees left or right
			Transform endPoint = sp.endPointObj.transform;
			Vector3 prospectiveDirection;
			prospectiveDirection = (endPoint.rotation * sectionRotation).eulerAngles;
			if(prospectiveDirection.y < 269.9 && prospectiveDirection.y > 89.9) { 
				continue;
			}
			// If this was a level change section, change level!
			if(sp.sectionType == 2){
				currentState = sectionState.lhc;
			} else if(sp.sectionType == 3){
				currentState = sectionState.lab;	
			}
			
			previousSection = prospectiveSection;
			return prospectiveSection;
		}
		Debug.LogError("NO VALID SECTIONS FOUND. YOU MUST INCLUDE START SECTION, AND AT LEAST TWO OTHER SECTIONS");
		return null;
	}
	
}
