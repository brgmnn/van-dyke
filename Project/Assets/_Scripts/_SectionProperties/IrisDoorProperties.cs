using UnityEngine;
using System.Collections;

public class IrisDoorProperties : SectionProperties {
	
	/* Custom behaviour: two possible powers can be activated in this
	 * section, so we must prevent the player being punished for not
	 * performing the other when they perform one
	 */
	
	bool completedSection = false;
	
	/* Mark the section as completed if either ability is used
	 */
	
	public override void Supercharge ()
	{
		completedSection = true;
		base.Supercharge ();
	}
	
	public override void Destroy ()
	{
		completedSection = true;
		base.Destroy ();
	}
	
	/* Check if the section is completed before punishing player
	 */
	
	public override void DestroyFail ()
	{
		if(completedSection) return;
		base.DestroyFail ();
	}
	
	public override void SuperchargeFail ()
	{
		if(completedSection) return;
		base.SuperchargeFail ();
	}
}
