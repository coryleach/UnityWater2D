using UnityEngine;
using System.Collections;

public class KillScript : MonoBehaviour {

	public float time = 2f;

	// Update is called once per frame
	void Update () {
	
		time -= Time.deltaTime;
		if ( time <= 0 ) {
			Destroy(gameObject);
		}
	
	}
}
