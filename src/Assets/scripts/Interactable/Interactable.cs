using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

public class Interactable : MonoBehaviour {

	public static Item USE_ITEM = null;
	public static bool HOVERING = false;
	public static Interactable TARGET = null;
	GameObject player_reference;
	InvGUI inventory_GUI;

	int HoverYOffsetRelative = 30; //number of pixels relative to 1080x1920 that hoverText is offset from renderer.bounds.center of interactable
	Material outline;
	MaterialObject[] materials;

	public bool isInteractable = true; //if false, object cannot clicked on
	public bool isHighlightable = true; //if false, object is not highlighted when hovered over
	public bool HasHoverText = true;
	public string HoverText = null; //text that hovers over item
	public float InteractRadius = 0.5f; //distance from object that player must be under to interact with the object
	public float ClickRadius = 75f; //distance from object 

	public Interaction[] actions; //list of items that when used on interactable cause an object to run a method
	public bool hasInteractions = true; //if true, object can be interacted with items
	public int actionsLength = 1; //number of interactions

	new private Renderer renderer;
	private bool HoverThreadCalled = false;
	private bool ExitThreadCalled = false;
    private Coroutine HoverTextAnimationThread;
    private Coroutine AlertTextAnimationThread;

	GameObject HoverObjectPrefab;
	GameObject HoverObjectInstance;

	public Vector3 destination {
		get {
			return transform.Find ("Dest").position;
		}
	}

	protected struct MaterialObject {
		public Material material;
		public GameObject Object;

		public MaterialObject(Material _material, GameObject _object) {
			this.material = _material;
			this.Object = _object;
		}
	}

	[System.Serializable]
	public struct Interaction {
		public Item item;
		public string method;

		public Interaction(Item _item, string _method) {
			this.item = _item;
			this.method = _method;
		}
	}

	private MaterialObject[] GetMaterials(GameObject Object) {

		Renderer[] renderers = Object.GetComponentsInChildren<Renderer> ();
		if (Object.transform.childCount > 0 && (renderers == null || renderers.Length == 0)) {
			
			MaterialObject[] material_list = new MaterialObject[renderers.Length + 1];
			for (int i = 0; i <= material_list.Length; i++) {
				if (i != material_list.Length) {
					material_list [i].material = renderers [i].material;
					material_list [i].Object = renderers [i].gameObject;
				}
			}
			material_list [renderers.Length].material = gameObject.GetComponent<Renderer> ().material;
			material_list [renderers.Length].Object = gameObject;
			return material_list;
		} else {
			MaterialObject[] material_list = new MaterialObject[1];
			material_list [0].material = Object.GetComponent<Renderer> ().material;
			material_list [0].Object = Object;
			return material_list;
		}
	}

	private void SetMaterials(MaterialObject[] Materials, Material Outline = null) {
		if (Outline) {
			for (int i = 0; i < Materials.Length; i++) {
				Materials [i].Object.GetComponent<Renderer> ().material = outline;
			}
		} else {
			for (int i = 0; i < Materials.Length; i++) {
				Materials [i].Object.GetComponent<Renderer> ().material = Materials [i].material;
			}
		}
	}

	// Use this for initialization
	protected virtual void Start () {
		inventory_GUI = InvGUI.GUI;
		player_reference = PlayerController.thePlayer;
		HoverYOffsetRelative = (int) ((float) Screen.width / 1080 * HoverYOffsetRelative); //make HoverYOffsetRelative become fixed (non-relative)
		renderer = gameObject.GetComponent<Renderer> ();

		HoverObjectPrefab = Resources.Load ("HoverObject") as GameObject;
		outline = Resources.Load ("outlineComplete") as Material;
		materials = GetMaterials (gameObject);
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		if (InvGUI.USE_ITEM && HOVERING && Input.GetMouseButtonDown (0)) {
			USE_ITEM = InvGUI.USE_ITEM;
		}

		//is the player clicking on the object
		if (!InvGUI.GUI_OPEN) {
			if (IsHovering (renderer, ClickRadius)) {
				ExitThreadCalled = false;

				if (Input.GetMouseButtonDown (0) && isInteractable) {
					OnClick ();
				} else if (!HoverThreadCalled) {
					HoverThreadCalled = true;
					OnHover ();
					print ("hovered");
				} else {
					Hover ();
				}

			} else {
				HoverThreadCalled = false;
				if (!ExitThreadCalled) {
					ExitThreadCalled = true;
					OnExit ();
					print ("left");
				}
			}
		} else {
			DestroyHoverText ();
		}

		if (TARGET && TARGET == this) {
			if ((destination - player_reference.transform.position).magnitude < InteractRadius && player_reference.GetComponent<PlayerController>().stoppedAndFacing) {
				OnUse ();
				TARGET = null;
			}
		}
	}

	protected virtual void OnClick () {
		//OnClick is called when object is clicked on within raidus
		Debug.Log("clicked on object");
		TARGET = this;
	}

    /* Diverges the Interactable's logic upon player interaction.
     * Either will launch code for use of an item on object or
     * use of Interatable without item. Not to be called directly
     * or overriden in child classes. */
	protected void OnUse () {
        if (USE_ITEM != null)
            UseItemOn(USE_ITEM);
        else
            OnDefaultUse();
            
	}

    /* The default use of the object, feel free to override in
     * derived classes. usingItem is true iff the player uses an
     * invalid item on the Interactable, false if they use no item
     * whatsoever. Not to be called directly child classes (let OnUse()
     * and UseItemOn(Item item) call it */
    public virtual void OnDefaultUse(bool usingItem = false)
    {
        if (usingItem)
        {
            //the player has used an invalid item on this
        } else
        {
            //the player is not using an item on the this
            AlertTextAnimationThread = (StartCoroutine(Alert("testing testing testing", new Color(1, 0, 0, 1), 100, 2, HoverYOffsetRelative)));
        }
    }

    /* Checks an Item against the list of Item objects in actions to see
     * if said item has a predefined method to run if the item is such
     * and runs the method. Not to be called directly or overriden
     * in child classes. */
    protected void UseItemOn(Item item)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (item.id == actions[i].item.id)
            {

                Action myAction = () => { };
                System.Type thisType = this.GetType();
                MethodInfo mi = thisType.GetMethod(actions[i].method.Substring(0, actions[i].method.Length), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (mi != null)
                {
                    myAction = (Action)Delegate.CreateDelegate(typeof(Action), this, mi);
                }
                myAction();
                break;
            }
        }
        OnDefaultUse(true);

    }

	protected virtual void OnHover () {
		//OnHover is called when object is hovered over within radius
		if (isInteractable) {
			HOVERING = true;
		}
		if (HasHoverText) {
			HoverObjectInstance = Instantiate (HoverObjectPrefab, GameObject.Find ("PlayerGUI/Canvas").transform);
			HoverObjectInstance.transform.Find ("Text").GetComponent<Text> ().text = "";
            RectTransform HoverRect = HoverObjectInstance.GetComponent<RectTransform>();
            Vector3 RenderedPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position) + new Vector3(0, HoverYOffsetRelative);
            if (InScreenBounds(RenderedPoint))
            {
                HoverRect.position = RenderedPoint;
            }
            else
            {
                HoverRect.position = RenderedPoint - new Vector3(0, 2 * HoverYOffsetRelative + HoverRect.rect.height);
            }
            HoverTextAnimationThread = StartCoroutine (AnimateText (HoverText, HoverObjectInstance.transform.Find ("Text").GetComponent<Text> ()));
		}
		if (isHighlightable) {
			SetMaterials (materials, outline);
		}
	}
		
	protected virtual void OnExit () {
		//OnExit is called when the player stops hovering on an object
		if (isInteractable) {
			HOVERING = false;
		}
		if (HasHoverText) {
			DestroyHoverText ();
		}
		if (isHighlightable) {
			SetMaterials (materials);
		}
	}

	protected virtual void Hover() {
		RectTransform HoverRect = HoverObjectInstance.GetComponent<RectTransform> ();
		Vector3 RenderedPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position) + new Vector3 (0, HoverYOffsetRelative);

		if (HasHoverText) {
			if (InScreenBounds (RenderedPoint)) {
				HoverRect.position = RenderedPoint;
			} else {
				HoverRect.position = RenderedPoint - new Vector3 (0, 2 * HoverYOffsetRelative + HoverRect.rect.height);
			}
		}
	}

	private void DestroyHoverText() {
		StopCoroutine (HoverTextAnimationThread);
		Destroy (HoverObjectInstance);
	}

	private void OnDrawGizmos () {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.Find ("Dest").position, InteractRadius);
	}

	private bool IsVisible (Renderer Object) {

		/* largely copied from:
		 * https://answers.unity.com/questions/8003/how-can-i-know-if-a-gameobject-is-seen-by-a-partic.html
		 */

		LayerMask IgnoredLayers = (1 << 2) | (1 << 8);
		IgnoredLayers = ~IgnoredLayers;
		
		Vector3 ScreenPoint = Camera.main.WorldToScreenPoint (Object.bounds.center);

		//is in front of screenspace (Z-positive)
		if (ScreenPoint.z < 0) {
			return false;
		}

		//is in FOV
		if (ScreenPoint.x < 0 || ScreenPoint.x > Screen.width || ScreenPoint.y < 0 || ScreenPoint.y > Screen.height) {
			return false;
		}

		RaycastHit hit;
		if (Physics.Linecast (Camera.main.gameObject.transform.position, Object.bounds.center, out hit, IgnoredLayers, QueryTriggerInteraction.Ignore)) {
			if (hit.transform.name != Object.gameObject.name) {
				//in other words, object that got hit was not the same as target object
				return false;
			}
		}

		return true;

	}

	private bool InScreenBounds (Vector3 vector3) {

		if (vector3.x < 0 || vector3.x > Screen.width || vector3.y < 0 || vector3.y > Screen.height) {
			return false;
		}
		return true;

	}

	private bool IsHovering (Renderer Object, float Radius) {
		if (IsVisible(Object)) {
			if ((Input.mousePosition - new Vector3(Camera.main.WorldToScreenPoint(Object.bounds.center).x, Camera.main.WorldToScreenPoint(Object.bounds.center).y, 0f)).magnitude <= Radius) {
				return true;
			}
		}
		return false;
	}

	private bool IsRendered (Renderer Object) {
		return Object.isVisible ? true : false;
	}

	//transcluded from InvGUI.cs
	private IEnumerator AnimateText (string text, Text text_component) {

		bool ignore = false;
		bool bold = false;
		bool italics = false;

		foreach (char nextLetter in text.ToCharArray()) {
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
					
				if (text_component) {
					text_component.text += letter; //add stylized letter to the text
				}
			}

			ignore = false; //don't ignore next letter

			yield return new WaitForSeconds (1/15f);
		}
		StopCoroutine(HoverTextAnimationThread);
	}

    /* Calls an Alert which is text rising above the object that fades out over time.
     * Meant to inform the player. Messages could be like "It is locked" or "Cannot be opened" */
    private IEnumerator Alert(string text, Color color, float height, float time, float offset)
    {
        //initialize a border length relative to the screen's width
        float BORDER_LENGTH = (float)Screen.width / 1920f * 100f;
        //current running time
        var t = 0f;

        GameObject AlertText = Instantiate(HoverObjectPrefab, GameObject.Find("PlayerGUI/Canvas").transform);
        Text AlertTextComponent = AlertText.transform.Find("Text").GetComponent<Text>();
        AlertTextComponent.text = text;
        AlertTextComponent.color = color;
        RectTransform AlertRect = AlertText.GetComponent<RectTransform>();
        Vector3 start = Camera.main.WorldToScreenPoint(gameObject.transform.position) + new Vector3(0, offset);
        Vector3 end = start + new Vector3(0, height);
        if (end.y > Screen.height - BORDER_LENGTH)
            end = new Vector3(end.x, Screen.height - BORDER_LENGTH);
        if (end.x > Screen.width - BORDER_LENGTH)
            end = new Vector3(Screen.width - BORDER_LENGTH, end.y);
        else if (end.x < BORDER_LENGTH)
            end = new Vector3(BORDER_LENGTH, end.y);

        while (t < 1)
        {
            t += Time.deltaTime / time;

            start = Camera.main.WorldToScreenPoint(gameObject.transform.position) + new Vector3(0, offset);
            end = start + new Vector3(0, height);
            if (end.y > Screen.height - BORDER_LENGTH)
                end = new Vector3(end.x, Screen.height - BORDER_LENGTH);
            if (end.x > Screen.width - BORDER_LENGTH)
                end = new Vector3(Screen.width - BORDER_LENGTH, end.y);
            else if (end.x < BORDER_LENGTH)
                end = new Vector3(BORDER_LENGTH, end.y);

            AlertRect.position = Vector3.Lerp(start, end, t);
            AlertTextComponent.color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0), t);
            yield return null;
        }

        Destroy(AlertText);
    }

}

[CustomEditor(typeof(Interactable), true)]
public class InteractableEditor : Editor {

	public override void OnInspectorGUI()
	{
		Interactable script = (Interactable) target;

		script.isInteractable = EditorGUILayout.Toggle ("Interactable?", script.isInteractable);
		if (script.isInteractable) {
			script.isHighlightable = EditorGUILayout.Toggle ("Highlightable?", script.isHighlightable);
		} else {
			script.isHighlightable = false;
		}

		//hover text input
		script.HasHoverText = EditorGUILayout.Toggle ("Has Hover Text? ", script.HasHoverText);
		if (script.HasHoverText) {
			script.HoverText = EditorGUILayout.TextArea (script.HoverText, GUILayout.MaxHeight(75));
		}

		//interact and click radii
		script.InteractRadius = EditorGUILayout.Slider("Interact Radius", script.InteractRadius, 0f, 10f);
		script.ClickRadius = EditorGUILayout.Slider("Click Radius", script.ClickRadius, 0f, 200f);

		//interactions
		int lastLength = script.actionsLength;
		EditorGUIUtility.labelWidth = 115f;
		GUILayout.BeginHorizontal(GUILayout.MaxHeight(20f));
		EditorGUIUtility.labelWidth = 110f;
		script.hasInteractions = EditorGUILayout.Toggle ("Has Interactions?", script.hasInteractions, GUILayout.MaxHeight(20f));
		if (script.hasInteractions) {
			GUILayout.BeginVertical (GUILayout.MaxWidth (300f));
			EditorGUIUtility.labelWidth = 40f;
			EditorGUIUtility.fieldWidth = 20f;
			script.actionsLength = EditorGUILayout.IntSlider ("Size:", script.actionsLength, 1, 5, GUILayout.MaxHeight (20f));
			GUILayout.EndVertical ();
		}
		if (script.actionsLength != lastLength) {
			var actions_copy = script.actions;
			script.actions = new Interactable.Interaction[script.actionsLength];
			if (script.actions != null && script.actions.Length != 0) {
				for (int i = 0; i < actions_copy.Length; i++) {
					if (i >= script.actions.Length) {
						break;
					} else {
						script.actions [i] = actions_copy [i];
					}
				}
			}
		}
		GUILayout.EndHorizontal ();
		if (script != null && script.hasInteractions && !(script.actions == null || script.actions.Length == 0))
		{
			for (int i = 0; i < script.actions.Length; i++) {
				EditorGUIUtility.labelWidth = 50f;
				GUILayout.BeginHorizontal(GUILayout.MaxHeight(20f));

				GUILayout.BeginVertical (GUILayout.MaxWidth(166.66f));
				script.actions[i].item = (Item) EditorGUILayout.ObjectField ("   " + (i+1) + ".", script.actions[i].item, typeof(Item), false, GUILayout.MaxHeight (20f));
				GUILayout.EndVertical ();

				GUILayout.BeginVertical (GUILayout.MaxWidth(166.66f));
				script.actions [i].method = EditorGUILayout.TextField("Method:", script.actions[i].method, GUILayout.MaxHeight(20f));
				GUILayout.EndVertical ();

				GUILayout.EndHorizontal();
			}
		}

	}
}
