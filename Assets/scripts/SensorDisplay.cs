// read data from buffers of TCPManager
// displays visualization(force, TF, gripper, UI buttons etc.) at appropriate locations

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR && UNITY_WSA
using System.Threading;
#endif

public class SensorDisplay : MonoBehaviour {
	private const int AXIS_X = 0;
	private const int AXIS_Y = 1;
	private const int AXIS_Z = 2;
	private const int AXIS_TOTAL = 3;

	public GameObject cameraToggleButton = null;
	public GameObject tfToggleButton = null;
	public GameObject plannerToggleButton = null;

	public GameObject left_gripper_target = null;
	public GameObject left_gripper_force_target = null;

	public GameObject tf_coordindates_target = null;

	public GameObject plannerCanvas = null;

	public GameObject forceBarGraph = null;
	private Slider sliderX = null;
	private Slider sliderY = null;
	private Slider sliderZ = null;
	private bool isShowingForceBar = false;


	private GameObject[] tf_coordinate_gameobject_list = new GameObject[TCPManager.TCPPackageConstants.TOTAL_NUM_TF-1];

	private GameObject left_gripper_force_target_x = null;
	private GameObject left_gripper_force_target_y = null;
	private GameObject left_gripper_force_target_z = null;
	private GameObject left_gripper_force_target_total = null;

	// components of the force arrows
	private GameObject force_x_arrow = null;
	private GameObject force_x_cylinder = null;
	private GameObject force_y_arrow = null;
	private GameObject force_y_cylinder = null;
	private GameObject force_z_arrow = null;
	private GameObject force_z_cylinder = null;
	private GameObject force_total_arrow = null;
	private GameObject force_total_cylinder = null;


	public GameObject gripperButton = null;

	public GameObject actionListCanvas = null;

    //gets latest gripper info
    private TCPManager.GripperInfo _left_gripper_info = new TCPManager.GripperInfo();

    // keeps reference to the TCPManager to get access to data sent from hololens
    private TCPManager tcpManager = null;

    // keeps reference to the marker
    // use GetMarkerTransformationMatrix() to get the latest matrix
    private ARUWPMarker aruwpMarker = null;

    // stores marker transform matrix with respect to hololens
    private Matrix4x4 latestMarkerTransMatrix = new Matrix4x4();

    const float force_thresh = 5f;

    // force delta above this will fill the whole arrow bar
    const float force_delta_max = 15f;

	const float torque_thresh = 0.5f;

	// convert from right hand to left hand coordinate
	private Matrix4x4 robotCoordinateOrientationToUnityMatrix = new Matrix4x4();

	// stores the base orientation
	private Quaternion _forceVisualOrientation;

	// align visualization x,y,z direction, a hack
	private Quaternion toVisualizeForce = Quaternion.Euler(0, 0, 90);

    /// <summary>
    /// GameObject that is temporarily used to hold transformation. [internal use]
    /// </summary>
    private GameObject dummyGameObject;

    private GameObject holoLensCamera;

    public bool isShowingForceVisual = false;
    public bool isShowingTFCoordinates = false;
    public bool isShowingLeftGripper = true;
    public bool isShowingCameraView = false;
    public bool isShowingPlannerCanvas = false;


    private TCPManager.tfData[] _tfDataArr = new TCPManager.tfData[TCPManager.TCPPackageConstants.TOTAL_NUM_TF];

    private Matrix4x4[] robotPoseToHololensMatrices = new Matrix4x4[TCPManager.TCPPackageConstants.TOTAL_NUM_TF];

    // shows video feed from robot camera
    public GameObject previewPlane = null;

    // set the position of the warning to be near left gripper
    public GameObject warningSign = null;

    // Unity Material to hold the Unity Texture of camera preview image
    private Material mediaMaterial = null;


    //holds the image data from robot
    private Texture2D mediaTexture = null;

    public static int image_height = 540;
    public static int image_width = 960;

    private byte[] _image_buffer = new byte[image_height * image_width * 4];

    private Vector4 globalTrackingOffset = new Vector4(0,0,0,0);



    public void toggleAlwaysPush(){
    	Inventory.toggleChildChoice();
    }

    public void globalAdjustX(float val){
    	globalTrackingOffset.x = val;
    }

    public void globalAdjustY(float val){
    	globalTrackingOffset.y = val;
    }

    public void globalAdjustZ(float val){
    	globalTrackingOffset.z = val;
    }
    
	public void toggleForceBarVisual(){
		if (isShowingForceBar){
			forceBarGraph.SetActive(false);
			isShowingForceBar = false;
		} else {
			forceBarGraph.SetActive(true);
			isShowingForceBar = true;
		}
	}

	public void toggleCameraVisual(){
		if (isShowingCameraView){
			previewPlane.SetActive(false);
			isShowingCameraView = false;
		} else {
			previewPlane.SetActive(true);
			isShowingCameraView = true;
		}
	}

	public void togglePlannerCanvas(){
		if (isShowingPlannerCanvas){
			plannerCanvas.SetActive(false);
			isShowingPlannerCanvas = false;
		} else {
			plannerCanvas.SetActive(true);
			isShowingPlannerCanvas = true;
		}
	}

	public void toggleForceVisual(){
		if (isShowingForceVisual){
			left_gripper_force_target.SetActive(false);
			isShowingForceVisual = false;
		} else {
			left_gripper_force_target.SetActive(true);
			isShowingForceVisual = true;
		}
	}

	public void toggleLeftGripperVisual(){
		if (isShowingLeftGripper){
			left_gripper_target.SetActive(false);
			isShowingLeftGripper = false;
		} else {
			left_gripper_target.SetActive(true);
			isShowingLeftGripper = true;
		}
	}

	public void toggleTFVisual(){
		if (isShowingTFCoordinates){
			tf_coordindates_target.SetActive(false);
			isShowingTFCoordinates = false;
		} else {
			tf_coordindates_target.SetActive(true);
			isShowingTFCoordinates = true;
		}
	}


    //find force visual game objects
	private void findForceVisualObjects(){
        left_gripper_force_target_x = left_gripper_force_target.transform.Find("axis_x").gameObject;
        left_gripper_force_target_y = left_gripper_force_target.transform.Find("axis_y").gameObject;
        left_gripper_force_target_z = left_gripper_force_target.transform.Find("axis_z").gameObject;
        left_gripper_force_target_total = left_gripper_force_target.transform.Find("total").gameObject;

        force_x_arrow = left_gripper_force_target_x.transform.Find("new_arrow").gameObject;
        force_x_cylinder = left_gripper_force_target_x.transform.Find("new_cylinder").gameObject;

        force_y_arrow = left_gripper_force_target_y.transform.Find("new_arrow").gameObject;
        force_y_cylinder = left_gripper_force_target_y.transform.Find("new_cylinder").gameObject;

        force_z_arrow = left_gripper_force_target_z.transform.Find("new_arrow").gameObject;
        force_z_cylinder = left_gripper_force_target_z.transform.Find("new_cylinder").gameObject;

        GameObject placeholder = left_gripper_force_target_total.transform.Find("placeholder").gameObject;

        force_total_arrow = placeholder.transform.Find("new_arrow").gameObject;
        force_total_cylinder = placeholder.transform.Find("new_cylinder").gameObject;
	}
	

#if !UNITY_EDITOR && UNITY_WSA
	// Use this for initialization
	void Start () {
        tcpManager = GetComponent<TCPManager>();
		aruwpMarker	= GetComponent<ARUWPMarker>();

        holoLensCamera = GameObject.Find("HoloLensCamera");

		robotCoordinateOrientationToUnityMatrix.SetRow(0, new Vector4(1, 0,  0, 0));
		robotCoordinateOrientationToUnityMatrix.SetRow(1, new Vector4(0, -1,  0, 0));
		robotCoordinateOrientationToUnityMatrix.SetRow(2, new Vector4(0, 0, 1, 0));
		robotCoordinateOrientationToUnityMatrix.SetRow(3, new Vector4(0, 0,  0, 1));

        dummyGameObject = new GameObject("Dummy");
        dummyGameObject.transform.SetParent(holoLensCamera.transform);

		findForceVisualObjects();
		findTFCoordinateVisualObjects();


        for (int i = 0; i < TCPManager.TCPPackageConstants.TOTAL_NUM_TF; i++) {
            _tfDataArr[i] = new TCPManager.tfData();
            robotPoseToHololensMatrices[i] = new Matrix4x4();
        }

        mediaMaterial = previewPlane.GetComponent<MeshRenderer>().material;

        // params: width, height, 
        mediaTexture = new Texture2D(image_width, image_height, TextureFormat.BGRA32, false);
        mediaMaterial.mainTexture = mediaTexture;

		sliderX = forceBarGraph.transform.Find("BarX").GetComponent<Slider>();
		sliderY = forceBarGraph.transform.Find("BarY").GetComponent<Slider>();
		sliderZ = forceBarGraph.transform.Find("BarZ").GetComponent<Slider>();

		left_gripper_force_target.SetActive(isShowingForceVisual);
		left_gripper_target.SetActive(isShowingLeftGripper);
		tf_coordindates_target.SetActive(isShowingTFCoordinates);
		previewPlane.SetActive(isShowingCameraView);
		warningSign.SetActive(false);
		plannerCanvas.SetActive(false);
		left_gripper_target.SetActive(false);
		actionListCanvas.SetActive(false);
		forceBarGraph.SetActive(false);

	}

	private void findTFCoordinateVisualObjects(){
		// excluding the left gripper
		for (int i=0; i < TCPManager.TCPPackageConstants.TOTAL_NUM_TF - 1; i++){
			tf_coordinate_gameobject_list[i] = tf_coordindates_target.transform.Find(i.ToString()).gameObject;
		}
	}


	// Update is called once per frame
	void Update () {
        if (aruwpMarker.tracking_updated) {
			processMarkerTrackingInfo();
			aruwpMarker.tracking_updated = false;

        }

        if (tcpManager.tf_data_updated) {
        	updateTFArray();

			for (int i=0; i < TCPManager.TCPPackageConstants.TOTAL_NUM_TF; i++){
				calculatePosefromTF(i);
			}

        	visualizeTFCoordinates();

        	updateUILocations();

			visualizeGripper();
			
        	tcpManager.tf_data_updated = false;
        }

        // only update visualization if there is new data
		if (tcpManager.left_gripper_force_torque_updated){
			updateLeftGripperInfo();
	      	calculateForceVisualOrientation();
        	visualizeForce();
        	visualizaForceBar();
        	tcpManager.left_gripper_force_torque_updated = false;

		}

		if (tcpManager.image_data_updated){
			updateImageByteArray();
			updateImagePreview();
			tcpManager.image_data_updated = false;
		}

	}

	private void updateUILocations(){
		Vector3 headPos = ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.HEAD]);
		Vector3 buttonPosBase = headPos + new Vector3(0.2f, 0.2f, 0);

		// put plannerToggleButton at buttonPosBase
		plannerToggleButton.transform.position = buttonPosBase;


		// showing cameraToggleButton to the right
		cameraToggleButton.transform.position = buttonPosBase + new Vector3(0.1f, 0, 0);

		// showing tfToggleButton to the right of camera button + x
		tfToggleButton.transform.position = buttonPosBase + new Vector3(0.2f, 0, 0);

		// showing the images from robot camera above head (y+)
		if (!isShowingCameraView){
			previewPlane.transform.parent.position = headPos + new Vector3(0, 0.5f, 0);
		}

		// put the warning sign to be to the left of gripper (x-)
		Vector3 leftHandPos = ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.LEFT_HAND]);
		warningSign.transform.position = leftHandPos + new Vector3(-0.15f, -0.1f, 0);

		// put gripper button to be to the right of the left gripper +x, and higher +y
		gripperButton.transform.position = leftHandPos + new Vector3(0.25f, 0.2f, 0);

		//put the planner canvas in front of the robot -z (default position)
		// if the planner canvas is opened, stop setting this location (allow canvas to be dragged)
		if (!isShowingPlannerCanvas){
			plannerCanvas.transform.position = headPos + new Vector3(0, 0, -0.3f);
		}

		if (!isShowingForceBar){
			forceBarGraph.transform.position = leftHandPos + new Vector3(-0.1f, 0, 0);
		}
	}

	private void updateImageByteArray(){
		Interlocked.Exchange(ref _image_buffer, tcpManager.image_buffer);
	}

	private void updateImagePreview(){
		// load byte array
		mediaTexture.LoadRawTextureData(_image_buffer);
        mediaTexture.Apply();
        Debug.Log("texture applied");
	}

	private void updateLeftGripperInfo(){
		Interlocked.Exchange(ref _left_gripper_info, tcpManager.left_gripper_info);
    	// Debug.Log(_left_gripper_info.position);
	}

	private void visualizeTFCoordinates(){
		// not visualizing the LEFT GRIPPER frame
		for (int i=0; i < TCPManager.TCPPackageConstants.TOTAL_NUM_TF - 1; i++){
			if(tf_coordinate_gameobject_list[i] != null && isShowingTFCoordinates){
		    	ARUWPUtils.SetMatrix4x4ToGameObject(ref tf_coordinate_gameobject_list[i], robotPoseToHololensMatrices[i]);
		    	// Debug.Log("processed tf frame visual " + i +" :" + ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[i]));
			}
		}
	}

	private void calculateTFCoordinates(){
		// not calculating the left gripper

	}

	private void updateTFArray() {
		for (int i = 0; i < TCPManager.TCPPackageConstants.TOTAL_NUM_TF; i++){
			Interlocked.Exchange(ref _tfDataArr[i], tcpManager.tfDataArr[i]);
		}
	}


	// latestMarkerTransMatrix now contains the latest marker pose in static world frame
	private void processMarkerTrackingInfo(){
    	// Interlocked.Exchange(ref latestMarkerTransMatrix, aruwpMarker.GetMarkerTransformationMatrix());
		latestMarkerTransMatrix = aruwpMarker.GetMarkerTransformationMatrix();

		// convert the transformation in hololens frame
		// to transformation in the global frame
		// because hololens can move in the world, it's frame is not fixed
    	dummyGameObject.transform.localRotation = ARUWPUtils.QuaternionFromMatrix(latestMarkerTransMatrix);
    	dummyGameObject.transform.localPosition = ARUWPUtils.PositionFromMatrix(latestMarkerTransMatrix);
    	latestMarkerTransMatrix = dummyGameObject.transform.localToWorldMatrix;
    	// Debug.Log("marker to user" + ARUWPUtils.PositionFromMatrix(latestMarkerTransMatrix));
    	// Debug.Log("marker to user orientation" + ARUWPUtils.QuaternionFromMatrix(latestMarkerTransMatrix).eulerAngles);
	}


	private void calculateForceVisualOrientation(){
		_forceVisualOrientation = ARUWPUtils.QuaternionFromMatrix(robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.BASE]);
		_forceVisualOrientation = toVisualizeForce * _forceVisualOrientation;
	}


	private void calculatePosefromTF(int ID)
	{
		Matrix4x4 tempLinkToMarkerMatrix = new Matrix4x4();
        tempLinkToMarkerMatrix.SetTRS(_tfDataArr[ID].position, _tfDataArr[ID].orientation, new Vector3(1, 1, 1));

        // change to left hand coordiante
        tempLinkToMarkerMatrix = robotCoordinateOrientationToUnityMatrix * tempLinkToMarkerMatrix;

    	// Debug.Log("gripper to marker" + ARUWPUtils.PositionFromMatrix(tempLinkToMarkerMatrix));
    	// Debug.Log("gripper to marker orientation" + ARUWPUtils.QuaternionFromMatrix(tempLinkToMarkerMatrix).eulerAngles);


		tempLinkToMarkerMatrix = latestMarkerTransMatrix * tempLinkToMarkerMatrix;
		Vector4 newPosition = tempLinkToMarkerMatrix.GetColumn(3) + globalTrackingOffset;
		tempLinkToMarkerMatrix.SetColumn(3, newPosition);
        robotPoseToHololensMatrices[ID] = tempLinkToMarkerMatrix;
    	// Debug.Log("gripper to user" + ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[ID]));


	}

	private void visualizeGripper(){
		// Debug.Log("left_gripper position: " + ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.LEFT_GRIPPER]));
    	ARUWPUtils.SetMatrix4x4ToGameObject(ref left_gripper_target, robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.LEFT_GRIPPER]);
	}


	// use the new force value to update arrow length in visualization
	private void adjustForceArrowSize(float new_value, int axis) {
		float units = new_value * 2;	// scaling

		float threshold = 1.0f;

		float new_cylinder_yscale, new_arrow_ypos, new_arrow_yscale;
		if (units > 0) {
			new_arrow_yscale = 1.0f;
			if (units < threshold){
				units = threshold;
			}
		} 
		else {
			new_arrow_yscale = -1.0f;
			if (units > -threshold){
				units = -threshold;
			}
		}
		new_cylinder_yscale = (float) units * 4;
		new_arrow_ypos = (float) units * 0.01f;


		switch (axis)
		{
			case AXIS_X:
				force_x_cylinder.transform.localScale = new Vector3(1, new_cylinder_yscale, 1);
				force_x_arrow.transform.localPosition = new Vector3(0, new_arrow_ypos, 0);
				force_x_arrow.transform.localScale = new Vector3(1, new_arrow_yscale, 1);
				break;
			case AXIS_Y:
				force_y_cylinder.transform.localScale = new Vector3(1, new_cylinder_yscale, 1);
				force_y_arrow.transform.localPosition = new Vector3(0, new_arrow_ypos, 0);
				force_y_arrow.transform.localScale = new Vector3(1, new_arrow_yscale, 1);
				break;
			case AXIS_Z:
				force_z_cylinder.transform.localScale = new Vector3(1, new_cylinder_yscale, 1);
				force_z_arrow.transform.localPosition = new Vector3(0, new_arrow_ypos, 0);
				force_z_arrow.transform.localScale = new Vector3(1, new_arrow_yscale, 1);
				break;
			case AXIS_TOTAL:
				force_total_cylinder.transform.localScale = new Vector3(1, new_cylinder_yscale, 1);
				force_total_arrow.transform.localPosition = new Vector3(0, new_arrow_ypos, 0);
				force_total_arrow.transform.localScale = new Vector3(1, 1.5f * new_arrow_yscale, 1);
				break;
		}
	}


	private void visualizeForce() {
		// the robot base frame xyz direction is not what we what
		adjustForceArrowSize(_left_gripper_info.force.x, AXIS_X);
		adjustForceArrowSize(-_left_gripper_info.force.y, AXIS_Y);
		adjustForceArrowSize(_left_gripper_info.force.z, AXIS_Z);
		adjustForceArrowSize(_left_gripper_info.force.magnitude, AXIS_TOTAL);

		Vector3 dir = left_gripper_force_target_x.transform.up * _left_gripper_info.force.x +
					  left_gripper_force_target_y.transform.up * -_left_gripper_info.force.y +
					  left_gripper_force_target_z.transform.up * _left_gripper_info.force.z;
        Quaternion rot =  Quaternion.LookRotation(dir);
        // Debug.Log(rot.eulerAngles);
        left_gripper_force_target_total.transform.rotation = rot;
		// Quaternion rot = Quaternion.FromToRotation(-left_gripper_force_target_z.transform.up, dir);
		// left_gripper_force_target_total.transform.localRotation = rot;

		// prevent the force visual overlapping with the tf frame on the left hand
		// set in down -y, and to the left -x
		left_gripper_force_target.transform.localPosition = ARUWPUtils.PositionFromMatrix(robotPoseToHololensMatrices[TCPManager.TCPPackageConstants.LEFT_GRIPPER])
															+ new Vector3(0.07f, -0.1f, 0);
		left_gripper_force_target.transform.localRotation = _forceVisualOrientation;
	}

	private void visualizaForceBar(){
		sliderX.value = Mathf.Abs(_left_gripper_info.force.x);
		sliderY.value = Mathf.Abs(_left_gripper_info.force.y);
		sliderZ.value = Mathf.Abs(_left_gripper_info.force.z);
	}

#else
	void Start(){
		findForceVisualObjects();
	}


#endif

}
