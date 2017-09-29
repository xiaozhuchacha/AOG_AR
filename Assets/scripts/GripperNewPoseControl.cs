// controls the canvas for editing end node information
// use OnScreenKeyboard to change end node name
// enable/disable dragging visual 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;


public class GripperNewPoseControl : MonoBehaviour {
	public GameObject left_gripper_original = null;
	public GameObject left_gripper_drag_visual = null;
	public GameObject left_gripper_state_display_target = null;

	private BoxCollider leftGripperOriginalCollider = null;


    private bool isShowingGripperState = false;
    private HandDraggableCustom lefGripperDraggable = null;

    private Text actionNameText;
    private Button actionNameButton;

    // uses the Microsoft OnScreenKeyboard to type
    // to use this function, the build type for the unity project
    // needs to be XAML
    private OnScreenKeyboard onScreenKeyboard;


    // function is called when an end node on the AOG is clicked
    // function enables / disables editing of the end node
	public void toggleLeftGripperStateVisual(Text callingNodeName){
		if (isShowingGripperState){
			left_gripper_state_display_target.SetActive(false);
			isShowingGripperState = false;
			lefGripperDraggable.IsDraggingEnabled = false;
			leftGripperOriginalCollider.enabled = true;

			// decide what to do with delta_y
			lefGripperDraggable.delta_pos = Vector3.zero;
			lefGripperDraggable.resetText();

			// stores the new string name to the action
			callingNodeName.text = actionNameText.text;
			actionNameText.text = "";

		} else {
			left_gripper_state_display_target.SetActive(true);
			isShowingGripperState = true;
			lefGripperDraggable.IsDraggingEnabled = true;
			leftGripperOriginalCollider.enabled = false;

			actionNameText.text = callingNodeName.text;
		}
	}


	// uses windows OnScreenKeyboard to type and modify the action name
	private void changeActionName(){
		onScreenKeyboard.openKeyboard(actionNameText);
	}


	// Use this for initialization
	void Start () {
        lefGripperDraggable = left_gripper_drag_visual.GetComponent<HandDraggableCustom>();
        GameObject ActionNameObject = left_gripper_state_display_target.transform.Find("ActionName").gameObject;
		actionNameText = ActionNameObject.GetComponent<Text>();
		actionNameButton = ActionNameObject.GetComponent<Button>();
		onScreenKeyboard = GetComponent<OnScreenKeyboard>();
		// plannerInventory = plannerCanvas.GetComponent<Inventory>();

		actionNameButton.onClick.AddListener(changeActionName);

        if (lefGripperDraggable == null){
        	Debug.Log("cannot find draggable");
        }
        lefGripperDraggable.IsDraggingEnabled = false;
		left_gripper_state_display_target.SetActive(false);

		leftGripperOriginalCollider = left_gripper_original.transform.Find("gripper_open").GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		// 
        left_gripper_drag_visual.SetActive(left_gripper_original.activeSelf);

	}
}
