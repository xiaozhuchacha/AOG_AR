// provides call back to edit the pose of end node

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditAction : MonoBehaviour {
	private Button button;
	private Text actionName;
	public GameObject manager;
	private GripperNewPoseControl gripperNewPoseControl;

	// Use this for initialization
	void Start () {
		button = GetComponent<Button>();
		if (button == null){
			Debug.Log("ERROR: missing button script on new node");
		}
		actionName = transform.Find("Text").gameObject.GetComponent<Text>();
		gripperNewPoseControl = manager.GetComponent<GripperNewPoseControl>();
		button.onClick.AddListener(editNode);
	}

	void editNode(){
		gripperNewPoseControl.toggleLeftGripperStateVisual(actionName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
