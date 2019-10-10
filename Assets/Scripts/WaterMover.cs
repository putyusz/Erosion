using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMover : MonoBehaviour {

    public GameObject player;
	void Start () {
		
	}	

	void Update () {
        transform.position.Set(player.transform.position.x, transform.position.y, player.transform.position.z);
	}
}
