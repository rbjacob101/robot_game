using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
[System.Serializable]
public class Item : ScriptableObject {

	public int id;
	new public string name = "";
	public string tooltip = "";
	public Sprite icon = null;

	//determines if the item can be moved outside of player's inv
	public bool isEssential;

	//determines if the item is destroyed upon use
	public bool destroyOnUse = true;

	/* false if the item closes the inventory (lockpicks, keys, etc.)
	 * or true if it is purely used inside inventory (healthpacks, food, etc.) 
	*/
	public bool inventoryOnly = false;

	public virtual void use () {

		if (destroyOnUse && !inventoryOnly) {
			Inventory.Player.RemoveItemByID (this.id);
			InvGUI.UpdateGUI ();
		}

		//create new classes and code new items, inheriting from this base "Item" class
		//use "override" access modifier and don't forget to call base.use(); at end of method
	}

	public override string ToString ()
	{
		return ("" + name);
	}
}
