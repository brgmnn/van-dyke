/* Sam Cox - 2009 - FrictionPointStudios.com 
 */

using UnityEngine;
using System.Collections;

public class BezierMover {

	public int passedPoints = 0;
	public Vector3 currentGoal = Vector3.zero;
	public float speed = 1f;
	public float currentTime = 0;
	
	bool lookBack = true;
	Vector3[] vectorArray;
	GameObject target;
	
	public BezierMover(GameObject t){
		// Matt's dynamic instantiator
		target = t;
	}

	public void InitialMove(){
		currentGoal = SectionManager.GetWaypoint(passedPoints);
		target.transform.position = currentGoal;
		passedPoints++;
		
        Vector3 newPosition = GetPointAtTime(0.01f,passedPoints);

        target.transform.LookAt(newPosition);

	}
	
	// Useful for moving a projectile to be as far as we are ahead
	public void SkipForward(BezierMover bm){
		passedPoints = bm.passedPoints;
		currentGoal = bm.currentGoal;
		currentTime = bm.currentTime;
	}
	
	// lateralMovement provides a horizontal offset, for sideways movement
	// lookAhead provides the option of enabling / disabling rotating the object being moved
	public void Move(float lateralMovement = 0.0f, bool lookAhead = true){
		currentGoal = SectionManager.GetWaypoint(passedPoints);
		
		// Don't move if the game is running slowly
		if(Time.timeScale < 0.1) return;
		
		if(currentTime < 1.0f){
			currentTime += speed * Time.smoothDeltaTime;
	        Vector3 newPosition = GetPointAtTime(currentTime,passedPoints);
			Vector3 latMov = new Vector3(lateralMovement,0,0);
			
			latMov = target.transform.rotation * latMov;
			
			newPosition += latMov;
	
			//TODO Putting out an error in restore floor, runs fine
	        if(lookBack && lookAhead) target.transform.LookAt(newPosition);
			lookBack = true;
			
			if(target.rigidbody){
	        	target.rigidbody.MovePosition(newPosition); 
			} else {
				target.transform.position = newPosition; 
			}
		} else {
			lookBack = false;
			currentTime = Time.smoothDeltaTime * speed;
			passedPoints += 1;
		}
	}
	
	
    public Vector3 GetPointAtTime(float t, int x)
    {
        return CreateBenzierForPoint(t,x);
    }

    private Vector3 CreateBenzierForPoint(float t, int x)
    {
        // This is based off http://homepage.mac.com/nephilim/sw3ddev/bezier.html		
		Vector3 prevl = SectionManager.GetWaypoint(x);
		Vector3 thisl = SectionManager.GetWaypoint(x+1);
		Vector3 nextl = SectionManager.GetWaypoint(x+2);
		Vector3 farl  = SectionManager.GetWaypoint(x+3);
		
        Vector3 delta1 = (nextl - prevl) * .166f;
        Vector3 delta2 = (farl - thisl) * .166f;

        return DoBenzierForNPoints(t, new Vector3[] { thisl, thisl + delta1, nextl - delta2, nextl });

    }

    private int CheckWithinArray(int x, int c)
    {
        if (x >= c)
        {
            return x % c;
        }
        else
        {
            return x;
        }
    }

    private Vector3 DoBenzierForNPoints(float t, Vector3[] currentArray)
    {
        Vector3 returnVector = Vector3.zero;
        
        float oneMinusT = (1f - t);

        int n = currentArray.Length - 1;

        for (int i = 0; i <= n; i++)
        {
            returnVector += BinomialCoefficient(n, i) * Mathf.Pow(oneMinusT, n - i) * Mathf.Pow(t, i) * currentArray[i];
        }

        return returnVector;
    }

    #region Standard Maths methods

    private float BinomialCoefficient(int n, int k)
    {
        if ((k < 0) || (k > n)) return 0;
        k = (k > n / 2) ? n - k : k;
        return (float) FallingPower(n, k) / Factorial(k);
    }

    private int Factorial(int n)
    {
        if (n == 0) return 1;
        int t = n;
        while (n-- > 2) 
            t *= n;
        return t;
    }

    private int FallingPower(int n, int p)
    {
        int t = 1;
        for (int i = 0; i < p; i++) t *= n--;
        return t;
    }

    #endregion

}
