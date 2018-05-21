using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class Inventory : MonoBehaviour {

	public static Inventory Player;
	new public string name;
	int MaxSize;
	public SlotData[] inventory;
	public bool isPlayerInventory;

	//TODO: make this the global class for all inventory item storage and such

	void Start () {
		if (isPlayerInventory) {
			Player = this;
			MaxSize = GameObject.Find ("WrapperPanel").transform.Find("SlotWrapper/SlotGrid").transform.GetComponentsInChildren<SlotScript> ().Length;
		}

		Validate_Inventory ();

	}

	[System.Serializable]
	public struct SlotData {
		public Item ThisItem;
		public int Quantity;

		public SlotData(Item item, int quantity) {
			this.ThisItem = item;
			this.Quantity = quantity;
		}
	}

	public void AddItem(Item item, int quantity = 1) {

		for (int i = 0; i < inventory.Length; i++) {
			if (inventory [i].ThisItem) {
				if (inventory [i].ThisItem.id == item.id) {
					int tempQuantity = inventory [i].Quantity;
					inventory [i] = new SlotData (inventory [i].ThisItem, tempQuantity + quantity);
					return;
				}
			}
		}
		//for loop did not find any existing items in inv that share ID to stack to
		//...adding its own dedicated slot

		if (GetNextOpenSlot() != null) {
			inventory [(int) GetNextOpenSlot ()] = new SlotData (item, quantity);
			return;
		}
		Debug.Log ("Not enough space in inventory to allow adding an item!");
	}

	public int? GetNextOpenSlot() {
		for (int i = 0; i < inventory.Length; i++) {
			if (!inventory [i].ThisItem) {
				return i;
			}
		}
		return null;
	}

	public void RemoveItemAtSlot (int slotNumber, int amount = 1, bool all = false) {
		if (slotNumber < inventory.Length) {
			if (all)
				inventory [slotNumber] = new SlotData (null, 0);
			else if (inventory [slotNumber].Quantity - amount <= 0)
				inventory [slotNumber] = new SlotData (null, 0);
			else {
				int tempQuantity = inventory [slotNumber].Quantity - amount;
				Item tempItem = inventory [slotNumber].ThisItem;
				inventory [slotNumber] = new SlotData (tempItem, tempQuantity);
			}
		} else
			Debug.Log ("Cannot remove item at slot position " + slotNumber);
	}

	public void RemoveItemByID (int id, int amount = 1, bool all = false) {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory [i].ThisItem) {
				if (inventory [i].ThisItem.id == id) {
					RemoveItemAtSlot (i, amount, all);
					break;
				}
			}
		}
	}

	public SlotData[] RetrieveInventoryArray() {
		return inventory;
	}

	public void StoreInventoryArray(SlotData[] slotData) {
		inventory = slotData;
	}

	public void MoveItem (Inventory fromInv, Inventory toInv, int fromSlot, int toSlot) {

		if (fromInv.Equals(toInv)) {
			//single inv setup
			SlotData[] invData = toInv.RetrieveInventoryArray();

			if (!invData [toSlot].ThisItem) {
				invData [toSlot] = new SlotData (invData [fromSlot].ThisItem, invData [fromSlot].Quantity);
				invData [fromSlot] = new SlotData (null, 0);
			} else {
				if (invData [fromSlot].ThisItem.id == invData [toSlot].ThisItem.id) {
					//ids are same so stack items instead
					invData [toSlot] = new SlotData (invData[toSlot].ThisItem, invData[fromSlot].Quantity + invData[toSlot].Quantity);
					invData [fromSlot] = new SlotData (null, 0);

				} else if (invData [fromSlot].ThisItem && invData[toSlot].ThisItem) {
					SlotData tempFromItem = invData [fromSlot];
					SlotData tempToItem = invData [toSlot];

					invData [fromSlot] = tempToItem;
					invData [toSlot] = tempFromItem;
				}
			}

			fromInv.StoreInventoryArray (invData);

		} else {

			//double inv setup
			SlotData[] fromInvData = fromInv.RetrieveInventoryArray ();
			SlotData[] toInvData = toInv.RetrieveInventoryArray ();

			if (!toInvData [toSlot].ThisItem) {

				toInvData [toSlot] = new SlotData (fromInvData [fromSlot].ThisItem, fromInvData [fromSlot].Quantity);
				fromInvData [fromSlot] = new SlotData (null, 0);

			} else  {
				if (fromInvData[fromSlot].ThisItem.id == toInvData[toSlot].ThisItem.id) {
					//ids are same so stack the items instead
					toInvData[toSlot] = new SlotData (toInvData[toSlot].ThisItem, toInvData[toSlot].Quantity + fromInvData[fromSlot].Quantity);
					fromInvData [fromSlot] = new SlotData (null, 0);

				} else if (fromInvData [fromSlot].ThisItem && fromInvData [fromSlot].ThisItem) {
					SlotData tempFromItem = fromInvData [fromSlot]; 
					SlotData tempToItem = toInvData [toSlot];

					fromInvData [fromSlot] = tempToItem;
					toInvData [toSlot] = tempFromItem;
				}
			}

			fromInv.StoreInventoryArray (fromInvData);
			toInv.StoreInventoryArray (toInvData);
		}
	}

	//ensures that only quantifiable items and non-null item types are in the inventory
	void Validate_Inventory() {
		for (int i = 0; i < inventory.Length; i++) {
			if (inventory [i].Quantity <= 0 || !inventory [i].ThisItem) {
				inventory [i] = new SlotData (null, 0);
			}
		}
	}

}