using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour {

	public Texture2D cursorNormal;
	public Texture2D cursorHand;
	public CursorMode cursorMode = CursorMode.ForceSoftware;
	public Vector2 cursorHotspot = Vector2.zero;

	void Start ()
	{
		Normal ();
	}

	public void Normal()
	{
		Cursor.SetCursor(cursorNormal, cursorHotspot, cursorMode);
	}

	public void Hand()
	{
		Cursor.SetCursor(cursorHand, cursorHotspot, cursorMode);
	}
}

