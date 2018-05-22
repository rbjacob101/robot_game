using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {

	public static GameObject thePlayer;

	//initialize player's Animator and NavMesh
	Animator anim;
	public NavMeshPath navMeshPath;
	public NavMeshAgent navAgent;

	public Transform navTarget;
	public float walkingDistance;
	public float walkSpeed;
	public float runSpeed;
	public float decelAmt;
	public Collider[] eventZones;
	public Vector3 vel;

	private InvGUI inv;
	private float speed;
	new private Camera camera;
	private Vector3 mouseToScreen;
	private float clickTime;

	void Awake() {
		if (thePlayer == null)
			thePlayer = gameObject;
		else if (thePlayer != gameObject)
			Destroy(gameObject);
	}

	// Use this for initialization
	void Start () {
		inv = InvGUI.GUI;

		speed = walkSpeed;
		navMeshPath = new NavMeshPath();
		navAgent = GetComponent<NavMeshAgent> ();

		//assign anim to the component in the inspector
		anim = GetComponent<Animator> ();
	}

	bool stopped() {
		if (!navAgent.pathPending)
		{
			if (navAgent.remainingDistance <= navAgent.stoppingDistance)
			{
				if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	bool simple_stopped() {
		if (!navAgent.pathPending && (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f))
		{
			return true;
		}
		return false;
	}

	public bool stoppedAndFacing {
		get {
			if (stopped() && Vector3.Angle(gameObject.transform.forward, Interactable.TARGET.transform.position - gameObject.transform.position) < 20f) {
				return true;
			}
			return false;
		}
	}
	
	// update is called once per frame
	void Update () {

		vel = navAgent.velocity;

		LayerMask ignoreRays = (1 << 2) | (1 << 8);
		ignoreRays = ~ignoreRays;

		if (!inv.isOpen) {
			if (Input.GetMouseButtonDown (0)) {
				clickTime = Time.time;
			}
			
			if (Input.GetMouseButtonUp (0)) {
				if ((Time.time - clickTime) < 0.2) { //the player made a short click

                    if (Interactable.TARGET && !Interactable.HOVERING)
                    {
                        Interactable.TARGET = null;
                    }
                    if (Interactable.USE_ITEM && !Interactable.HOVERING)
                    {
                        Interactable.USE_ITEM = null;
                    }

                    RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

					if (Physics.Raycast (ray, out hit, 20, ignoreRays, QueryTriggerInteraction.Ignore)) {
						if (!Interactable.HOVERING) {
							navTarget.position = hit.point;
						} else {
                            navTarget.position = Interactable.TARGET.destination;
						}
					}
					navAgent.destination = navTarget.position;
				}

			}
		}

		if (Interactable.TARGET && simple_stopped()) {
			transform.rotation = Quaternion.Slerp(transform.rotation, Interactable.TARGET.transform.rotation, 5f * Time.deltaTime);
		}

		float SpeedPercent = navAgent.velocity.magnitude / navAgent.speed;
		anim.SetFloat ("SpeedPercent", SpeedPercent, 0.001f, Time.deltaTime);

	}
}
