using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
[System.Serializable]

public class SlotPanel : MonoBehaviour {
    public List<Slot> itemList;
    public Transform contentPannel;
    //public Text myNodes;

    // Use this for initialization
    public void AddButtons()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Slot slot = itemList[i];
            GameObject newslot = slot.item;
            newslot.transform.SetParent(contentPannel);

            GameObject item = newslot.GetComponent<GameObject>();
            //slot.Setup(item, this);
            if (item)
            {
                slot.arrow.SetActive(true);
            }
            else
            {
                slot.arrow.SetActive(false);
            }
        }
    }
}
