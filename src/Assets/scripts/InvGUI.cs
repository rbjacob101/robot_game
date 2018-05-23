using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvGUI : MonoBehaviour {

	#region const init
	const float DragIconLerpVal = 1f;
	const float DoublePlayerInvPercentageFill = 0.6f;
	#endregion

	#region static init
	public static InvGUI GUI;
	public static bool GUI_OPEN = false;
	public static Item USE_ITEM;
	#endregion

	#region static methods
	/* Static method to update inventory
	 * Meant only for InvGUI singles format */
	public static void UpdateGUI() {
		if (InvGUI.GUI.isSingle) {
			InvGUI.GUI.UpdateInventory (InvGUI.GUI.myInv);
		}
	}
	#endregion

	#region public init
	public bool isOpen = false;
	public bool isSingle;
	public GameObject invUI;
	public float fadeTime;
	public float slotLerpTime;
	public float slotHighlightedAlpha;
	public float slotAlertLerpTime;
	public GameObject SingleViewSlotsParent;
	public GameObject DoubleViewSlotsParent;
	public SlotScript[] slotsSingle;
	public SlotScript[] slotsDoublePlayer;
	public SlotScript[] slotsDoubleObject;
	public Inventory myInv;
	public Inventory currentObjectInv;
	public Sprite PlaceHolder;
	public float CatchTime;
	public bool isTooltipLerping = false;
	public float tooltipMaxAlpha;
	public float doubleClickRadius;
	public GridLayoutGroup[] gridLayouts;
	public Text[] inventoryLables;
	public bool isDragIconLerping = false;
	#endregion

	#region private init
	private CanvasGroup SingleCanvas;
	private CanvasGroup DoubleCanvas;
	private RectTransform WrapperPanel;
	private RectTransform DoubleWrapperPanel;
	private GameObject invAssets;
	private GameObject DoublePlayerInv;
	private GameObject DoubleObjectInv;
	private Image DragIcon;
	private Text DragIconQuantity;
	private bool onHomeSlot = false;
	private HoverData nullSlot;
	private HoverData hoverData;
	private HoverData downSlot;
	private float timePressedButton0;
	private CanvasGroup tooltipAlpha;
	private Text tooltipText;
	private RectTransform tooltipRect;
	private string tooltipString;
	private bool tooltipAnimating;
	private Vector3 clickMousePosition;
	private Text ObjectInventoryName;
	private CustomScrollRect SingleScrollRect;
	#endregion

	void Awake() {
		if (GUI == null)
			GUI = this;
		else if (GUI != this)
			Destroy(gameObject);
	}

	// Use this for initialization
	void Start () {

		invAssets = transform.Find ("InvAssets").gameObject;
		invAssets.SetActive (true);
		isSingle = true;
		SingleCanvas = transform.Find ("InvAssets/Canvas/WrapperPanel").gameObject.GetComponent<CanvasGroup> ();
		DoubleCanvas = transform.Find ("InvAssets/Canvas/WrapperPanelDouble").gameObject.GetComponent<CanvasGroup> ();
		WrapperPanel = transform.Find ("InvAssets/Canvas/WrapperPanel").gameObject.GetComponent<RectTransform> ();
		DoubleWrapperPanel = transform.Find ("InvAssets/Canvas/WrapperPanelDouble").gameObject.GetComponent<RectTransform> ();
		DoublePlayerInv = transform.Find ("InvAssets/Canvas/WrapperPanelDouble/PlayerTextView").gameObject;
		DoubleObjectInv = transform.Find ("InvAssets/Canvas/WrapperPanelDouble/ObjectTextView").gameObject;
		DragIcon = transform.Find ("InvAssets/Canvas/Icon").gameObject.GetComponent<Image> ();
		DragIconQuantity = transform.Find ("InvAssets/Canvas/Icon/QuantityText").gameObject.GetComponent<Text> ();
		SingleScrollRect = transform.Find ("InvAssets/Canvas/WrapperPanel/SlotWrapper").gameObject.GetComponent<CustomScrollRect> ();
		tooltipAlpha = transform.Find ("InvAssets/Canvas/Tooltip").gameObject.GetComponent<CanvasGroup> ();
		tooltipText = transform.Find ("InvAssets/Canvas/Tooltip/Text").gameObject.GetComponent<Text> ();
		tooltipRect = transform.Find ("InvAssets/Canvas/Tooltip").gameObject.GetComponent<RectTransform> ();
		ObjectInventoryName = transform.Find ("InvAssets/Canvas/WrapperPanelDouble/ObjectTextView/Text").gameObject.GetComponent<Text>();

		ResizeDisplay ();

		slotsSingle = SingleViewSlotsParent.GetComponentsInChildren<SlotScript> ();
		slotsDoubleObject = DoubleObjectInv.transform.Find ("ObjectWrapper/SlotWrapper/SlotGrid").gameObject.GetComponentsInChildren<SlotScript> ();
		slotsDoublePlayer = DoublePlayerInv.transform.Find ("PlayerWrapper/SlotWrapper/SlotGrid").gameObject.GetComponentsInChildren<SlotScript> ();
		myInv = GetComponent<Inventory> ();

		SetDragIcon (PlaceHolder, 0);
		SingleCanvas.alpha = 0f;
		DoubleCanvas.alpha = 0f;
		transform.Find ("InvAssets/Canvas/Tooltip").gameObject.GetComponent<Image> ().color = new Color (1f, 1f, 1f, tooltipMaxAlpha);

		Inventory randomInv = new Inventory ();
		nullSlot = new HoverData(false, -1, randomInv); //what does not being on any slot look like?

	}

	void ResizeDisplay () {
		float slotLength = (90f / 1920f) * Screen.width;
		float slotSpacing = (10f / 1920f) * Screen.width;

		//perhaps add this to Update later in case of changes in screen width on the fly (fullscreen, etc)
		WrapperPanel.offsetMin = new Vector2(0.443802f * Screen.width, 0.217785f * Screen.height); 
		WrapperPanel.offsetMax = new Vector2 (-0.258698f * Screen.width, -0.217785f * Screen.height);

		DoubleWrapperPanel.offsetMin = new Vector2(0.443802f * Screen.width, 0.146785f * Screen.height); 
		DoubleWrapperPanel.offsetMax = new Vector2 (-0.258698f * Screen.width, -0.217785f * Screen.height);

		DoubleObjectInv.GetComponent<LayoutElement>().minHeight = (slotLength * 2 + 10 * 3); //2 90px slots tall, with 3 10px spaces between
		DoublePlayerInv.GetComponent<LayoutElement>().minHeight = (slotLength * 3 + 10 * 4) + 5; //3 90px slots tall, with 4 10px spaces between 

		foreach (GridLayoutGroup gridLayout in gridLayouts) {
			gridLayout.cellSize = new Vector2 (slotLength, slotLength);
			gridLayout.spacing = new Vector2 (slotSpacing, slotSpacing);
		}
		foreach (Text text in inventoryLables) {
			text.fontSize = (int) Mathf.Floor(((25f / 1920f) * Screen.width)+0.5f); //rounded down
		}
		transform.Find("InvAssets/Canvas/Icon").gameObject.transform.localScale = new Vector3 (Screen.width * (1f / 1920f), Screen.width * (1f / 1920f), 1);

	}

	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (isSingle) { 
				UpdateInventory (myInv);
			} else if (!isSingle && isOpen) {
				UpdateInventory (myInv);
				isSingle = true;
			} else if (!isSingle && !isOpen) {
				//just in case inv is closed and still in double format
				isSingle = true;
				UpdateInventory (myInv);
			}
			isOpen = !isOpen;
			GUI_OPEN = !GUI_OPEN;
		}
		if (isOpen) {
			if (isSingle) {
				SingleCanvas.alpha = Mathf.Lerp (SingleCanvas.alpha, 1f, fadeTime * Time.deltaTime); //if single inv, open single inv
				SingleCanvas.blocksRaycasts = true;
				DoubleCanvas.blocksRaycasts = false;
			}
			else {
				DoubleCanvas.alpha = Mathf.Lerp (DoubleCanvas.alpha, 1f, fadeTime * Time.deltaTime); // and if double inv, open double inv
				DoubleCanvas.blocksRaycasts = true;
				SingleCanvas.blocksRaycasts = false;
			}
		} else {
			SingleCanvas.alpha = Mathf.Lerp (SingleCanvas.alpha, 0f, fadeTime * Time.deltaTime); //if closed, fade both inventories
			DoubleCanvas.alpha = Mathf.Lerp (DoubleCanvas.alpha, 0f, fadeTime * Time.deltaTime);
			DoubleCanvas.blocksRaycasts = false;
			SingleCanvas.blocksRaycasts = false;
		}

		ItemDraggingStep (isSingle);

	}

	//please remember that this function is purely for the InvGUI
	void UpdateInventory(Inventory inv, bool single=true, Inventory inv2 = null) {

		if (single) {
			Inventory.SlotData[] invArray = inv.RetrieveInventoryArray ();

			for (int i = 0; i < invArray.Length; i++) {
				if (invArray [i].ThisItem != null) {
					if (invArray [i].Quantity == 1) {
						slotsSingle [i].UpdateIcon (invArray [i].ThisItem.icon);
					} else {
						slotsSingle [i].UpdateIcon (invArray [i].ThisItem.icon, invArray [i].Quantity);
					}
				} else {
					slotsSingle [i].UpdateIcon (PlaceHolder, 0);
				}
			}
		} else {
			Inventory.SlotData[] inv1_array = inv.RetrieveInventoryArray ();
			Inventory.SlotData[] inv2_array = inv2.RetrieveInventoryArray ();

			for (int i = 0; i < inv1_array.Length; i++) {
				if (inv1_array [i].ThisItem != null) {
					if (inv1_array [i].Quantity == 1) {
						slotsDoublePlayer [i].UpdateIcon (inv1_array [i].ThisItem.icon);
					} else {
						slotsDoublePlayer [i].UpdateIcon (inv1_array [i].ThisItem.icon, inv1_array [i].Quantity);
					}
				} else {
					slotsDoublePlayer [i].UpdateIcon (PlaceHolder, 0);
				}
			}

			for (int i = 0; i < inv2_array.Length; i++) {
				if (inv2_array [i].ThisItem != null) {
					if (inv2_array [i].Quantity == 1) {
						slotsDoubleObject [i].UpdateIcon (inv2_array [i].ThisItem.icon);
					} else {
						slotsDoubleObject [i].UpdateIcon (inv2_array [i].ThisItem.icon, inv2_array [i].Quantity);
					}
				} else {
					slotsDoubleObject [i].UpdateIcon (PlaceHolder, 0);
				}
			}

		}
	}

	[System.Serializable]
	public struct HoverData {
		public bool IsHover;
		public int SlotNumber;
		public Inventory Inv;

		public HoverData(bool isHover, int slotNumber, Inventory inv) {
			this.IsHover = isHover;
			this.SlotNumber = slotNumber;
			this.Inv = inv;
		}
	}

	HoverData ReturnHoverSlot () {
		if (isSingle) {
			for (int i = 0; i < slotsSingle.Length; i++) {
				if (slotsSingle [i].isMouseOver (slotsSingle [i].button)) {
					return new HoverData (true, i, myInv);
				}
			}
			return nullSlot;
		} else {
			if (isMouseOver (DoubleObjectInv.GetComponent<RectTransform> (), DoubleObjectInv.GetComponent<RectTransform>())) {
				//return hover slot of the object inventory
				for (int i = 0; i < slotsDoubleObject.Length; i++) {
					if (slotsDoubleObject [i].isMouseOver (slotsDoubleObject [i].button)) {
						return new HoverData (true, i, currentObjectInv);
					}
				}
				return nullSlot;
			} else if (isMouseOver (DoublePlayerInv.GetComponent<RectTransform> (), DoublePlayerInv.GetComponent<RectTransform>())) {
				//return hover slot of the player's double inventory
				for (int i = 0; i < slotsDoublePlayer.Length; i++) {
					if (slotsDoublePlayer[i].isMouseOver(slotsDoublePlayer[i].button)) {
						return new HoverData (true, i, myInv);
					}
				}
				return nullSlot;
			} else return nullSlot;
		}
	}

	public void ItemDraggingStep(bool single = true) {
		//inv movement data
		//TODO: MIGHT revise this

		DragIcon.color = new Color (1f, 1f, 1f, isDragIconLerping ? DragIconLerpVal : 0f);
		DragIconQuantity.color = new Color (1f, 1f, 1f, isDragIconLerping ? DragIconLerpVal : 0f);
		DragIcon.gameObject.GetComponent<RectTransform> ().transform.position = Input.mousePosition;

		float a = tooltipAlpha.alpha;
		tooltipAlpha.alpha = Mathf.Lerp (a, isTooltipLerping ? 1f : 0f, fadeTime);

		if (isOpen) { // the inv must be open in order to use it! :)
			if (single) {
				hoverData = ReturnHoverSlot ();

				if (Input.GetMouseButton (0) && !Input.GetMouseButtonDown (0) && !Input.GetMouseButtonUp (0)) {
					//the player did not just release or press, but is holding mouse button

					if (Time.time - timePressedButton0 > CatchTime || Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) > doubleClickRadius) { //make sure that its NOT a double click
						onHomeSlot = (hoverData.Equals (downSlot)) ? true : false; //is the player currently on home slot?
						isTooltipLerping = false;
					
						slotsSingle [downSlot.SlotNumber].iconAlphaLerp = false; //if they are hold-hovering over anything else than the one they came, hide the downSlot item
						slotsSingle [downSlot.SlotNumber].quantityLerp = false;
						if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) { // to show the dragIcon, the downSlot must have an icon
							isDragIconLerping = true; //if they are hold-hovering over anything else, show the dragIcon
						} else {
							isDragIconLerping = false;
						}
					} else if (Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) < doubleClickRadius) { //without this, clicking randomly on an item will delete it
						onHomeSlot = (hoverData.Equals (downSlot)) ? true : false;
					}

				} else if (!Input.GetMouseButton (0) && !Input.GetMouseButtonDown (0) && !Input.GetMouseButtonUp (0)) {
					//the player is not using mouse button at all
					onHomeSlot = false;
					isDragIconLerping = false; //the player can't be dragging an item if the button isn't pressed

					if (hoverData.IsHover && myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem && !isTooltipLerping && isMouseOver (WrapperPanel)) { //if player is hovering over a slot and the tooltip is not already being displayed
						//do slot script animation
						isTooltipLerping = true;

						Inventory.SlotData highlightedItem = myInv.RetrieveInventoryArray () [hoverData.SlotNumber];
						//generate text that will be animated
						tooltipText.text = "";
						tooltipString = "" + highlightedItem.ThisItem.name.ToLower () + (highlightedItem.Quantity > 1 ? " (" + highlightedItem.Quantity + ") \n/" : " \n/") + highlightedItem.ThisItem.tooltip + "/";
						StopAllCoroutines ();
						tooltipAnimating = false;
						StartCoroutine (AnimateTooltip (tooltipString));
					}

					if (!hoverData.IsHover || !myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem) {
						isTooltipLerping = false;
						StopAllCoroutines (); //don't want any mixed up messages, only one coroutine at a time
					}
					if (isTooltipLerping) {
						tooltipRect.position = Input.mousePosition;
					}

				} else if (Input.GetMouseButtonDown (0)) {
					//the player just pressed the mouse button

					//they double clicked on item and are using the item they clicked on
					if (Time.time - timePressedButton0 < CatchTime && Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) < doubleClickRadius) {
						if (myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem) {
							#region USE ITEM
							USE_ITEM = myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem;

							if (!USE_ITEM.inventoryOnly) {
								isOpen = !isOpen;
								GUI_OPEN = !GUI_OPEN;
							} else {
								USE_ITEM.use();
								Inventory.Player.RemoveItemAtSlot(hoverData.SlotNumber);
								InvGUI.UpdateGUI();
								USE_ITEM = null;
							}
								

							#endregion
						}
					} else {
						downSlot = hoverData;
						timePressedButton0 = Time.time;
						//only set dragIcon to slot's item icon if the slot has an item icon
						if (downSlot.SlotNumber != -1 && myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem != null) { 
							SetDragIcon (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.icon, (int)myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity);
						} else {
							SetDragIcon (PlaceHolder, 0);
						}
					}

					timePressedButton0 = Time.time; //record last double click time
					clickMousePosition = Input.mousePosition;

				} else if (Input.GetMouseButtonUp (0)) {
					//the player just released the mouse button
					if (!onHomeSlot && hoverData.IsHover && isMouseOver (WrapperPanel)) { 							//they were on a slot which was not the one they came from
						myInv.MoveItem (myInv, myInv, downSlot.SlotNumber, hoverData.SlotNumber);
						UpdateInventory (myInv);
						if (!tooltipAnimating && onHomeSlot) {
							StopAllCoroutines ();
						}
					} else {																						//they released on the slot they came from or on no slot at all
						if ((!hoverData.IsHover || hoverData.SlotNumber == downSlot.SlotNumber) && myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) {
							slotsSingle [downSlot.SlotNumber].iconAlphaLerp = true;
							if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity != 1) {
								slotsSingle [downSlot.SlotNumber].quantityLerp = true;
							}
						}
					}
				}
			} else {
				//the inventory is in double format
				hoverData = ReturnHoverSlot ();
				if (Input.GetMouseButton (0) && !Input.GetMouseButtonDown (0) && !Input.GetMouseButtonUp (0)) {
					//player did not press or release, but is holding mouse button

					if (Time.time - timePressedButton0 > CatchTime || Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) > doubleClickRadius) { //make sure that its NOT a double click
						onHomeSlot = (hoverData.Equals (downSlot)) ? true : false; //is the player currently on home slot?
						isTooltipLerping = false;

						if (downSlot.IsHover) {
							if (downSlot.Inv.Equals (myInv)) {
								slotsDoublePlayer [downSlot.SlotNumber].iconAlphaLerp = false;
								slotsDoublePlayer [downSlot.SlotNumber].quantityLerp = false;

								if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) { // to show the dragIcon, the downSlot must have an icon
									isDragIconLerping = true; //if they are hold-hovering over anything else, show the dragIcon
								} else {
									isDragIconLerping = false;
								}

							} else if (downSlot.Inv.Equals (currentObjectInv)) {
								slotsDoubleObject [downSlot.SlotNumber].iconAlphaLerp = false;
								slotsDoubleObject [downSlot.SlotNumber].quantityLerp = false;

								if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) { // to show the dragIcon, the downSlot must have an icon
									isDragIconLerping = true; //if they are hold-hovering over anything else, show the dragIcon
								} else {
									isDragIconLerping = false;
								}
							}
						}
					} else if (Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) < doubleClickRadius) {
						onHomeSlot = (hoverData.Equals (downSlot)) ? true : false;
					}
						

				} else if (!Input.GetMouseButton (0) && !Input.GetMouseButtonDown (0) && !Input.GetMouseButtonUp (0)) {
					//the player is not using mouse button at all
					onHomeSlot = false;
					isDragIconLerping = false; //the player can't be dragging an item if the button isn't pressed

					if (hoverData.IsHover) {
						if (hoverData.Inv.Equals (myInv)) {
							if (myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem && !isTooltipLerping && isMouseOver (DoublePlayerInv.GetComponent<RectTransform> ())) { //if player is hovering over a slot and the tooltip is not already being displayed
								//do slot script animation
								isTooltipLerping = true;

								Inventory.SlotData highlightedItem = myInv.RetrieveInventoryArray () [hoverData.SlotNumber];
								//generate text that will be animated
								tooltipText.text = "";
								tooltipString = "" + highlightedItem.ThisItem.name.ToLower () + (highlightedItem.Quantity > 1 ? " (" + highlightedItem.Quantity + ") \n/" : " \n/") + highlightedItem.ThisItem.tooltip + "/";
								StopAllCoroutines ();
								tooltipAnimating = false;
								StartCoroutine (AnimateTooltip (tooltipString));
							} else if (!myInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem) {
								isTooltipLerping = false;
								StopAllCoroutines ();
							}

							if (isTooltipLerping) {
								tooltipRect.position = Input.mousePosition;
							}

						} else if (hoverData.Inv.Equals (currentObjectInv)) {
							if (currentObjectInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem && !isTooltipLerping && isMouseOver (DoubleObjectInv.GetComponent<RectTransform> ())) { //if player is hovering over a slot and the tooltip is not already being displayed
								//do slot script animation
								isTooltipLerping = true;

								Inventory.SlotData highlightedItem = currentObjectInv.RetrieveInventoryArray () [hoverData.SlotNumber];
								//generate text that will be animated
								tooltipText.text = "";
								tooltipString = "" + highlightedItem.ThisItem.name.ToLower () + (highlightedItem.Quantity > 1 ? " (" + highlightedItem.Quantity + ") \n/" : " \n/") + highlightedItem.ThisItem.tooltip + "/";
								StopAllCoroutines ();
								tooltipAnimating = false;
								StartCoroutine (AnimateTooltip (tooltipString));
							} else if (!currentObjectInv.RetrieveInventoryArray () [hoverData.SlotNumber].ThisItem) {
								isTooltipLerping = false;
								StopAllCoroutines ();
							}

							if (isTooltipLerping) {
								tooltipRect.position = Input.mousePosition;
							}

						}
					} else { //mouse not over a slot
						isTooltipLerping = false;
						StopAllCoroutines ();
					}

				} else if (Input.GetMouseButtonDown (0)) {
					//the player just pressed the mouse button

					if (Time.time - timePressedButton0 < CatchTime && Mathf.Abs((Input.mousePosition - clickMousePosition).magnitude) < doubleClickRadius) {
						//double clicking on an item
						//TODO: double click to quick move item from one inventory to another
						downSlot = hoverData;

						if (hoverData.IsHover) {
							onHomeSlot = true;
							//player is on a slot

							if (downSlot.Inv.Equals (myInv) && myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) { //there is an item and player doub-clicked on their inv
								if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.isEssential) {
									slotsDoublePlayer [downSlot.SlotNumber].Alert ();
								} else {
									currentObjectInv.AddItem (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem, myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity);
									myInv.RemoveItemAtSlot (downSlot.SlotNumber, 1, true);
									UpdateInventory (myInv, false, currentObjectInv);
								}
							} else {
								if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) { //they doub-clicked on a slot with an item in it in object inv
									myInv.AddItem (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem, currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity);
									currentObjectInv.RemoveItemAtSlot (downSlot.SlotNumber, 1, true);
									UpdateInventory (myInv, false, currentObjectInv);
								}
							}
						}


					} else {
						//not double clicking on an item
						downSlot = hoverData;
						timePressedButton0 = Time.time;

						if (hoverData.IsHover) {
							onHomeSlot = true;
							if (downSlot.Inv.Equals (myInv)) {
								//the player clicked down on their own inventory
								if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem != null) { //only set dragIcon to slot's item icon if the slot has an item icon
									SetDragIcon (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.icon, (int)myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity);
								} else {
									SetDragIcon (PlaceHolder, 0);
								}

							} else {
								//the player clicked down on the object inventory
								if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem != null) { //only set dragIcon to slot's item icon if the slot has an item icon
									SetDragIcon (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.icon, (int)currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity);
								} else {
									SetDragIcon (PlaceHolder, 0);
								}
							}
						}
					}

					timePressedButton0 = Time.time; //record last double click
					clickMousePosition = Input.mousePosition;

				} else if (Input.GetMouseButtonUp (0)) {
					//the player just released the mouse button
					if (hoverData.Inv.Equals (myInv)) {
						//player released on their own inventory
						if (!onHomeSlot && hoverData.IsHover) { 							
							//they were on a slot which was not the one they came from
							myInv.MoveItem (downSlot.Inv, hoverData.Inv, downSlot.SlotNumber, hoverData.SlotNumber);
							UpdateInventory (myInv, false, currentObjectInv);
							if (!tooltipAnimating && onHomeSlot) {
								StopAllCoroutines ();
							}
						}

					} else if (hoverData.Inv.Equals (currentObjectInv)) {
						//player released on current object inventory
						if (!onHomeSlot && hoverData.IsHover) { 							
							//they were on a slot which was not the one they came from
							if (!(downSlot.Inv.Equals (myInv) && myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.isEssential)) {
								myInv.MoveItem (downSlot.Inv, hoverData.Inv, downSlot.SlotNumber, hoverData.SlotNumber);
								UpdateInventory (myInv, false, currentObjectInv);
								if (!tooltipAnimating && onHomeSlot) {
									StopAllCoroutines ();
								}
							} else if (downSlot.Inv.Equals (myInv) && myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem.isEssential) {
								slotsDoublePlayer [downSlot.SlotNumber].Alert ();
								//do normal else statement code as the following else would do
								if (downSlot.Inv.Equals (myInv)) {
									if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) {
										slotsDoublePlayer [downSlot.SlotNumber].iconAlphaLerp = true;
										if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity != 1) {
											slotsDoublePlayer [downSlot.SlotNumber].quantityLerp = true;
										}
									}
								} else if (downSlot.Inv.Equals (currentObjectInv)) {
									if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) {
										slotsDoubleObject [downSlot.SlotNumber].iconAlphaLerp = true;
										if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity != 1) {
											slotsDoubleObject [downSlot.SlotNumber].quantityLerp = true;
										}
									}
								}
							}
						}
					} else {
						//the player released on a non-slot location or home slot
						if (downSlot.Inv && downSlot.Inv.Equals (myInv)) {
							if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) {
								slotsDoublePlayer [downSlot.SlotNumber].iconAlphaLerp = true;
								if (myInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity != 1) {
									slotsDoublePlayer [downSlot.SlotNumber].quantityLerp = true;
								}
							}
						} else if (downSlot.Inv.Equals (currentObjectInv)) {
							if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].ThisItem) {
								slotsDoubleObject [downSlot.SlotNumber].iconAlphaLerp = true;
								if (currentObjectInv.RetrieveInventoryArray () [downSlot.SlotNumber].Quantity != 1) {
									slotsDoubleObject [downSlot.SlotNumber].quantityLerp = true;
								}
							}
						}
					}

				}
			}
		} else {
			//inv is not open
			isTooltipLerping = false;
		}
	}

	void SetDragIcon(Sprite sprite, int Quantity) {
		DragIcon.sprite = sprite;

		if (Quantity <= 1) {
			DragIconQuantity.text = "";
		} else {
			DragIconQuantity.text = "" + Quantity;
		}
	}

	IEnumerator AnimateTooltip (string tooltip) {
		if (!tooltipAnimating) {

			bool ignore = false;
			bool bold = false;
			bool italics = false;

			tooltipAnimating = true;
			foreach (char nextLetter in tooltip.ToCharArray()) {
				switch (nextLetter) {
				case '¬':
					ignore = true; //ignore the styling character
					bold = !bold; //toggle boldness
					break;
				case '/':
					ignore = true; //ignore styling character
					italics = !italics; //toggle italics
					break;
				}

				string letter = nextLetter.ToString ();

				if (!ignore) {
					if (bold) {
						letter = "<b>" + letter + "</b>";
					}
					if (italics) {
						letter = "<i>" + letter + "</i>";
					}

					tooltipText.text += letter; //add stylized letter to the 
				}

				ignore = false; //don't ignore next letter

				yield return new WaitForSeconds (1f / tooltip.Length / 2);
				//wait time between character types is inversely proportional to length of text being displayed
				//i.e. length goes up, wait time gets smaller

			}
			tooltipAnimating = false;
			StopAllCoroutines ();
		}
		yield return null;
	}
		
	bool isMouseOver(RectTransform rect, RectTransform viewportRect) {
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

	bool isMouseOver (RectTransform rect) {
		//method overload for single rect

		Vector3[] worldCorners = new Vector3[4];
		rect.GetWorldCorners(worldCorners);

		if(Input.mousePosition.x >= worldCorners[0].x && Input.mousePosition.x < worldCorners[2].x 
			&& Input.mousePosition.y >= worldCorners[0].y && Input.mousePosition.y < worldCorners[2].y) {
			return true;
		}    
		return false;
	}

	public void LoadInventory(Inventory inv) {
		isSingle = false;
		currentObjectInv = inv;
		ObjectInventoryName.text = inv.name;
		isOpen = true; //open inventory with inv as loaded inv
		GUI_OPEN = true;
		UpdateInventory (myInv, false, currentObjectInv);
	}
}
