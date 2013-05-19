using UnityEngine;
using System.Collections;

public class Skin : MonoBehaviour {
	public GUISkin mainSkin;
	public static GUISkin sMainSkin;
	
	void OnGUI () {
		sMainSkin = mainSkin;
		GUI.skin = sMainSkin;
	}

}
