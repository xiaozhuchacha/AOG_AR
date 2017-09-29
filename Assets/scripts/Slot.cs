using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HoloToolkit.Unity.InputModule;

public class Slot : MonoBehaviour, IDropHandler {
    //public SlotPanel slotPanel;
    //public Transform slotTransform;
    public GameObject arrow;
    public GameObject item {
        get {
            if (transform.childCount>0) {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
        
    }
   
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("snap to slot");
        // if (!item)  {
        //     DragHandler.itemBeingDragged.transform.SetParent(transform);
        //     ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged ());
        // }
    }
    /*public void Setup(GameObject currentItem, SlotPanel currentPanel)
    {
        item = currentItem;
        slotPanel = currentPanel;
    }*/
}
