using UnityEngine;

public class TestController : MonoBehaviour 
{

	public GameObject weightPrefab;

	public float z = 0;
	
	void Update () 
	{
		if ( Input.GetMouseButtonDown(0) ) 
		{
			var pt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pt.z = z;
			Instantiate(weightPrefab,pt,Quaternion.identity);
		}
	}
	
}
