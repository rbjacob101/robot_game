using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class InteractablePickup : Interactable
{
    //item(s) that will be added to the player's inventory upon pickup
    public Inventory.SlotData[] items;
    public int itemsLength;

    protected override void OnDefaultUse(bool usingItem = false, Item item = null)
    {
        if (usingItem)
        {
            AlertTextAnimationThread = StartCoroutine(Alert("It does nothing.", new Color(1, 1, 1, 1), 100, HoverYOffsetRelative));
        } else
        {
            for (int i = 0; i < items.Length; i++)
            {
                Inventory.Player.AddItem(items[i].ThisItem, items[i].Quantity);
            }

            //destroy the pickup
            //TODO: might make this a fade out animation later
            TARGET = null;
            HOVERING = false;
            DestroyHoverText();
            Destroy(gameObject);
        }
    }
}

[CustomEditor(typeof(InteractablePickup), true)]
class InteractablePickupEditor : InteractableEditor
{
    public override void OnInspectorGUI()
    {
        InteractablePickup script = (InteractablePickup)target;

        script.isInteractable = EditorGUILayout.Toggle("Interactable?", script.isInteractable);
        if (script.isInteractable)
        {
            script.isHighlightable = EditorGUILayout.Toggle("Highlightable?", script.isHighlightable);
        }
        else
        {
            script.isHighlightable = false;
        }

        //hover text input
        script.HasHoverText = EditorGUILayout.Toggle("Has Hover Text? ", script.HasHoverText);
        if (script.HasHoverText)
        {
            script.HoverText = EditorGUILayout.TextArea(script.HoverText, GUILayout.MaxHeight(75));
        }

        //interact and click radii
        script.InteractRadius = EditorGUILayout.Slider("Interact Radius", script.InteractRadius, 0f, 10f);
        script.ClickRadius = EditorGUILayout.Slider("Click Radius", script.ClickRadius, 0f, 200f);

        //items
        if (script.isInteractable)
        {
            int lastLength = script.items.Length;
            script.itemsLength = EditorGUILayout.IntSlider("Size:", script.itemsLength, 1, 5, null);

            //length has changed, reevaluate items array size
            if (lastLength != script.itemsLength)
            {
                var items_copy = new Inventory.SlotData[script.items.Length];
                for (int i = 0; i < script.items.Length; i++)
                    items_copy[i] = script.items[i];
                script.items = new Inventory.SlotData[script.itemsLength];
                if (script.items != null && script.items.Length != 0)
                {
                    for (int i = 0; i < items_copy.Length; i++)
                    {
                        if (i >= script.items.Length)
                        {break;} else {script.items[i] = items_copy[i];}
                    }
                }
            }
            if (script != null && !(script.actions == null || script.actions.Length == 0)) {
                for (int i = 0; i < script.items.Length; i++)
                {
                    EditorGUIUtility.labelWidth = 50f;
                    GUILayout.BeginHorizontal(GUILayout.MaxHeight(20f));
                    GUILayout.BeginVertical(GUILayout.MaxWidth(166.66f));
                    script.items[i].ThisItem = (Item)EditorGUILayout.ObjectField("   " + (i + 1) + ".", script.items[i].ThisItem, typeof(Item), false, GUILayout.MaxHeight(20f));
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.MaxWidth(166.66f));
                    script.items[i].Quantity = EditorGUILayout.IntField("Quantity:", script.items[i].Quantity, GUILayout.MaxHeight(20f));
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
