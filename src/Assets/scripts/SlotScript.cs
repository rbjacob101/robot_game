using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour {

	Image normalButton;
	Image activeButton;
	public RectTransform button;
	float tempAlpha;
	bool selected;
	public bool iconAlphaLerp;
	public bool quantityLerp;
	InvGUI invControl;
	public Image iconImage;
	Text quantity;

	Sprite placeHolder;
	int DisplayedQuantity;

	// Use this for initialization
	void Start () {
		normalButton = transform.Find ("NormalButton").gameObject.GetComponent<Image>();
		activeButton = transform.Find ("NormalButton/SelectedButton").gameObject.GetComponent<Image>();
		button = GetComponent<RectTransform> ();
		iconImage = transform.Find ("NormalButton/SelectedButton/ItemIcon").gameObject.GetComponent<Image> ();
		quantity = transform.Find ("QuantityText").gameObject.GetComponent<Text> ();

		quantity.fontSize = (int) Mathf.Floor(((18f / 1920f) * Screen.width)+0.5f);

		normalButton.color = new Color (1f, 1f, 1f, 1f);
		activeButton.color = new Color (1f, 1f, 1f, 0f);
		tempAlpha = 1f;

		invControl = GameObject.FindGameObjectWithTag ("inventory").GetComponent<InvGUI> ();
		placeHolder = invControl.PlaceHolder;

		selected = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!selected) {
			tempAlpha = Mathf.Lerp (tempAlpha, (isMouseOver (button)) ? invControl.slotHighlightedAlpha : 1f, invControl.slotLerpTime * Time.deltaTime);
		} else {
			tempAlpha = Mathf.Lerp (tempAlpha, 0f, invControl.slotLerpTime * Time.deltaTime);
		}
			
		if (isMouseOver (button) && (Input.GetMouseButtonDown (0) || Input.GetMouseButtonUp (0))) {
			selected = true;
		} else if (!isMouseOver (button) && (Input.GetMouseButtonDown (0) || Input.GetMouseButtonUp (0))) {
			selected = false;
		}

		//if the slot currently has icon, change iconImage alpha to opaque
		//if not, change to transparent

		iconImage.color = new Color (1f, 1f, 1f, iconAlphaLerp ? 1f : 0f);
		quantity.color = new Color (1f, 1f, 1f, quantityLerp ? 1f : 0f);

		normalButton.color = new Color (1f, Mathf.Lerp(normalButton.color.g, 1f, invControl.slotAlertLerpTime * Time.deltaTime), Mathf.Lerp(normalButton.color.b, 1f, invControl.slotAlertLerpTime * Time.deltaTime), tempAlpha);
		activeButton.color = new Color (1f, Mathf.Lerp(activeButton.color.g, 1f, invControl.slotAlertLerpTime * Time.deltaTime), Mathf.Lerp(activeButton.color.b, 1f, invControl.slotAlertLerpTime * Time.deltaTime), 1 - tempAlpha);
	}
		
	public bool isMouseOver(RectTransform rect, RectTransform viewportRect) {
		//finds if the mouse is over a given RectTransform
		 
		if (isMouseOver (viewportRect)) {
			Vector3[] worldCorners = new Vector3[4];
			rect.GetWorldCorners (worldCorners);

			if (Input.mousePosition.x >= worldCorners [0].x && Input.mousePosition.x < worldCorners [2].x
			    && Input.mousePosition.y >= worldCorners [0].y && Input.mousePosition.y < worldCorners [2].y) {
				return true;
			}    
			return false;
		} else
			return false;
	}

	public bool isMouseOver (RectTransform rect) {
		//method overload for single rect

		Vector3[] worldCorners = new Vector3[4];
		rect.GetWorldCorners(worldCorners);

		if(Input.mousePosition.x >= worldCorners[0].x && Input.mousePosition.x < worldCorners[2].x 
			&& Input.mousePosition.y >= worldCorners[0].y && Input.mousePosition.y < worldCorners[2].y) {
			return true;
		}    
		return false;
	}

	public void UpdateIcon (Sprite icon = null, int? q = 0) {
		//if no sprite specified, just clear the slot of its item icon
		//give a value and it will display that number (item quantity)

		if (icon != null) {
			iconImage.sprite = icon;
			iconAlphaLerp = true;
		} else {
			iconImage.sprite = placeHolder;
			iconAlphaLerp = false;
		}

		if (q != 0) {
			quantity.text = "" + q;
			quantityLerp = true;
		} else {
			quantity.text = "";
			quantityLerp = false;
		}
		DisplayedQuantity = (int) q;
	}

	public Sprite MyIcon () {
		return iconImage.sprite;
	}

	public int MyQuantity () {
		return DisplayedQuantity;
	}

	public void Alert () {
		//make slots red for a moment
		normalButton.color = new Color (1f, 0f, 0f, tempAlpha);
		activeButton.color = new Color (1f, 0f, 0f, 1 - tempAlpha);
	}
		
}
