using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCulling : MonoBehaviour {

	public float includedDistance;

	private GameObject player;
	private GameObject[] occlusionLights;
	private float distanceToPlayer;
	private bool on;

	// Use this for initialization
	void Start () {
		occlusionLights = GameObject.FindGameObjectsWithTag("culledLighting");
		player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {

		foreach (GameObject light in occlusionLights) {
			distanceToPlayer = Vector3.Distance(player.transform.position, light.transform.position);

			if (distanceToPlayer < includedDistance) { 

				/*if (light.GetComponent<Light> ().enabled == false) {
					light.GetComponent<Light> ().enabled = true;
				}
				if (light.GetComponent<Light> ().enabled = true) {
					easeInOutCubic (0f, 2.5f, light.GetComponent<Light> ().intensity);
				}*/

				light.GetComponent<Light>().enabled = true;

			}
			if (distanceToPlayer > includedDistance) {

				/*if (light.GetComponent<Light> ().enabled == true) {
					easeInOutCubic (2.5f, 0f, light.GetComponent<Light> ().intensity);
				}

				if (light.GetComponent<Light> ().intensity <= 0) {
					light.GetComponent<Light> ().enabled = false;
				}*/
				light.GetComponent<Light>().enabled = false;
			}
		}
	}

	public float easeInOutCubic(float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if (value < 1) return end * 0.5f * value * value * value + start;
		value -= 2;
		return end * 0.5f * (value * value * value + 2) + start;
	}

}
