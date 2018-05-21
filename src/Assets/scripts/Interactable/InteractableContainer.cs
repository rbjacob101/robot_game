using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class InteractableContainer : Interactable {
	//inventory that the container uses
	public Inventory inventory;



	void Reset() {
		//set the default inventory
		inventory = gameObject.GetComponent<Inventory>() ? gameObject.GetComponent<Inventory>() : null;
	}
}
	
[CustomEditor(typeof(InteractableContainer), true)]
class InteractableContainerEditor : InteractableEditor {
	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		InteractableContainer interactableScript = (InteractableContainer) target;

		interactableScript.inventory = (Inventory) EditorGUILayout.ObjectField ("Inventory", interactableScript.inventory, typeof(Inventory), false);
	}
}
