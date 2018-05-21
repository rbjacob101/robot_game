using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;

public class CameraFollow : MonoBehaviour
{
	private Transform target;
	public Transform player;
	public Transform focus;
	public Transform desiredAngle;
	public InvGUI inv;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;
	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;
	public float distanceMin = .5f;
	public float distanceMax = 15f;
	public float smoothTime = 2f;
	public float scrollTime = 2f;
	public float invDistanceDenom = 2f;
	public float invScrollDistanceMin = 2f;

	public Transform CameraDirectionObject;
	private Vector3 lastPosition;

	float rotationYAxis = 0.0f;
	float rotationXAxis = 0.0f;
	float velocityX = 0.0f;
	float velocityY = 0.0f;
	float scrollVelocity = 0.0f;

	public PostProcessingProfile ppProfile;
	public float vignIntensityOpen;
	public float vignChange;
	public float vignIntensity;

	float step = 0.05f;
	float zRotation = 0;

	void Start()
	{
		Vector3 angles = transform.eulerAngles;
		rotationYAxis = angles.y;
		rotationXAxis = angles.x;

		distance = distanceMax;
		target = focus;

	}
	void LateUpdate()
	{
		//only rotate if player clicks and drags or mousewhell click-drags
		if (!inv.isOpen) { //only allow if inv is not open

			if (Input.GetMouseButton (2) || (Input.GetMouseButton (0))) {
				velocityX += xSpeed * Input.GetAxis ("Mouse X") * distance * 0.04f;
				velocityY += ySpeed * Input.GetAxis ("Mouse Y") * 0.04f;
			}

			if (Input.GetAxis ("Mouse ScrollWheel") != 0f) {
				scrollVelocity += Input.GetAxis ("Mouse ScrollWheel");
			}

			focus.position = Vector3.Lerp (focus.position, player.position, 3f * Time.deltaTime);
			lastPosition = gameObject.transform.position;
			scrollVelocity = -0.03f;

			rotationXAxis = Mathf.Lerp (rotationXAxis, 45, 0.02f);

		} else {
			distanceMin = 0f;
			distance = Mathf.Lerp (distance, invScrollDistanceMin, invDistanceDenom);

			Vector2 perpendicular = perpVector (lastPosition, player.position, 1.45f);
			focus.position = Vector3.Lerp (focus.position, player.position + new Vector3(-perpendicular.x, 1f, -perpendicular.y), Time.deltaTime);

			Vector3 DesiredAngle = angleBetween (desiredAngle.position, lastPosition, focus.transform.position);
			rotationYAxis += DesiredAngle.y * Time.deltaTime * step;
			rotationXAxis -= 0.25f;
			step = Mathf.Lerp (step, 0, 0.6f);
		}

		rotationYAxis += velocityX;
		rotationXAxis -= velocityY;
		rotationXAxis = ClampAngle (rotationXAxis, yMinLimit, yMaxLimit);
		zRotation = Mathf.Lerp (zRotation, inv.isOpen ? 4 : 0, Time.deltaTime);
		Quaternion toRotation = Quaternion.Euler (rotationXAxis, rotationYAxis, zRotation);
		Quaternion rotation = toRotation;

		//scrollwheel smooth zooming in with linear interpolation
		distance = Mathf.Clamp (distance - scrollVelocity, distanceMin, distanceMax);
		scrollVelocity = Mathf.Lerp (scrollVelocity, 0, Time.deltaTime * scrollTime);

		Vector3 negDistance = new Vector3 (0.0f, 0.0f, -distance);
		Vector3 position = rotation * negDistance + target.position;

		transform.rotation = rotation;
		transform.position = position;

		//linear interpolation. make the camera decellerate after movement
		velocityX = Mathf.Lerp (velocityX, 0f, Time.deltaTime * smoothTime);
		velocityY = Mathf.Lerp (velocityY, 0f, Time.deltaTime * smoothTime);

		interpolateVignette(inv.isOpen);
			
	}

	//make sure that the angle of rotation does not go beyond -360 or 360 in degrees
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

	Vector3 angleBetween(Vector3 a, Vector3 b, Vector3 c) {
		//three vectors in global coordinates, a, b and c
		//returns euler rotation of angle c inside triangle ABC
		//rotates only using x and y axes

		Vector2 YZ1 = new Vector2 (Mathf.Abs(a.y - c.y), a.z - c.z);
		Vector2 YZ2 = new Vector2 (Mathf.Abs(b.y - c.y), b.z - c.z);

		Vector2 XZ1 = new Vector2 (a.x - c.x, a.z - c.z);
		Vector2 XZ2 = new Vector2 (b.x - c.x, b.z - c.z);

		float XAngle, YAngle;
		XAngle = Mathf.Atan2 (YZ1.y, YZ1.x + YZ2.y);
		YAngle = Mathf.Atan2 (XZ1.y, XZ1.x + XZ2.y);

		return new Vector3 (XAngle * Mathf.Rad2Deg, YAngle * Mathf.Rad2Deg, 0f);

	}

	Vector2 perpVector (Vector3 v1, Vector3 v2, float m) {
		//finds a vector2 that is perpendicular to
		//the line starting at v1 and ending at v2
		//and is -90 degrees away that has magnitude m
		//NOTE: vector extends out of origin

		Vector2 norm = new Vector2 (v1.x - v2.x, v1.z - v2.z);
		norm = new Vector2 (norm.y, norm.x);
		norm = new Vector2 (norm.x, -norm.y);

		norm *= 1000f;
		norm = Vector2.ClampMagnitude (norm, m);
		return norm;
	}

	void interpolateVignette(bool invIsOpen) {
		VignetteModel.Settings vignetteSettings = ppProfile.vignette.settings;
		if (invIsOpen) {
			vignetteSettings.intensity = Mathf.Lerp (vignetteSettings.intensity, vignIntensityOpen, vignChange);
		} else {
			vignetteSettings.intensity = Mathf.Lerp (vignetteSettings.intensity, vignIntensity, vignChange);
		}
		ppProfile.vignette.settings = vignetteSettings;
	}
}