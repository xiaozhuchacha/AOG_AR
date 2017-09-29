using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeChange : MonoBehaviour {
    public Button button;
    public Sprite or;
	// Use this for initialization
	void Start () {
        button.onClick.AddListener(changeNode);
	}
	
	public void changeNode()
    {
        
        if (transform.GetChild(0).name == "OrNode")
        {
            transform.GetChild(0).GetComponent<Image>().sprite = or;
        //transform.GetChild(0).GetComponent<Text>().text = "yes";
        }
    }
}
