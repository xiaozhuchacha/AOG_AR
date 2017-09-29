// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using UnityEngine.UI;

// modifies the HandDraggable script in HololensToolKit
// able to lock certain axis of movement
// drag along only x, y, z or any combination of those by modifying 
// lock_x, lock_y, lock_z values
// and records the delta values on each axis
namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Component that allows dragging an object with your hand on HoloLens.
    /// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class HandDraggableCustom : MonoBehaviour,
                                 IFocusable,
                                 IInputHandler,
                                 ISourceStateHandler
    {
        public GameObject originalObject = null;
        /// <summary>
        /// Event triggered when dragging starts.
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// Event triggered when dragging stops.
        /// </summary>
        public event Action StoppedDragging;

        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        [Tooltip("Scale by which hand movement in z is multipled to move the dragged object.")]
        public float DistanceScale = 2f;
        
        public enum RotationModeEnum
        {
            Default,
            LockObjectRotation,
            OrientTowardUser,
            OrientTowardUserAndKeepUpright
        }

        public RotationModeEnum RotationMode = RotationModeEnum.Default;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired position")]
        [Range(0.01f, 1.0f)]
        public float PositionLerpSpeed = 0.2f;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired rotation")]
        [Range(0.01f, 1.0f)]
        public float RotationLerpSpeed = 0.2f;

        public bool IsDraggingEnabled = true;

        private Camera mainCamera;
        private bool isDragging;
        private bool isGazed;
        private Vector3 objRefForward;
        private Vector3 objRefUp;
        private float objRefDistance;
        private Quaternion gazeAngularOffset;
        private float handRefDistance;
        private Vector3 objRefGrabPoint;

        private Vector3 draggingPosition;
        private Quaternion draggingRotation;

        private IInputSource currentInputSource = null;
        private uint currentInputSourceId;

        // stores the delta vector3
        public Vector3 delta_pos;

        private const float delta_pos_norm_limit = 0.1f;

        public GameObject newHandPoseVisualization = null;

        private Text x_text = null;
        private Text y_text = null;
        private Text z_text = null;

        private Vector3 initial_pos;

        public GameObject warningSign = null;

        private bool lock_x = false;
        private bool lock_y = false;
        private bool lock_z = false;

        private Color changeTransparency = new Color(0, 0, 0, 0.7f);

        public void toggleLock_x(){
            lock_x = !lock_x;

            if (lock_x){
                x_text.color -= changeTransparency;
            } else {
                x_text.color += changeTransparency;
            }

        }

        public void toggleLock_y(){
            lock_y = !lock_y;
            if (lock_y){
                y_text.color -= changeTransparency;
            } else {
                y_text.color += changeTransparency;
            }            
        }

        public void toggleLock_z(){
            lock_z = !lock_z;
            if (lock_z){
                z_text.color -= changeTransparency;
            } else {
                z_text.color += changeTransparency;
            }   
        }

        public void updateText(Vector3 new_val){
            x_text.text = "Delta x: " + new_val.x.ToString("#.000");
            y_text.text = "Delta y: " + new_val.z.ToString("#.000");
            z_text.text = "Delta z: " + new_val.y.ToString("#.000");
        }

        public void resetText(){
            x_text.text = "Delta x: 0";
            y_text.text = "Delta y: 0";            
            z_text.text = "Delta z: 0";
        }

        private void deActivateObject(){
            gameObject.SetActive(false);
        }

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
            x_text = newHandPoseVisualization.transform.Find("x").GetComponent<Text>();
            y_text = newHandPoseVisualization.transform.Find("y").GetComponent<Text>();
            z_text = newHandPoseVisualization.transform.Find("z").GetComponent<Text>();
        }

        private void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }

            if (isGazed)
            {
                OnFocusExit();
            }
        }

        private void Update()
        {
            if (IsDraggingEnabled && isDragging)
            {
                UpdateDragging();
            } else {
                assignTransform();
            }
        }

        private void assignTransform(){
            gameObject.transform.position = originalObject.transform.position + delta_pos;
            gameObject.transform.rotation = originalObject.transform.rotation;
            gameObject.transform.localScale = originalObject.transform.localScale;
        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        public void StartDragging()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            // Add self as a modal input handler, to get all inputs during the manipulation
            InputManager.Instance.PushModalInputHandler(gameObject);

            isDragging = true;
            //GazeCursor.Instance.SetState(GazeCursor.State.Move);
            //GazeCursor.Instance.SetTargetObject(HostTransform);

            Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            Vector3 pivotPosition = GetHandPivotPosition();

            handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);


            objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

            Vector3 objForward = HostTransform.forward;
            Vector3 objUp = HostTransform.up;

            // Store where the object was grabbed from
            objRefGrabPoint = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);

            Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);

            objForward = mainCamera.transform.InverseTransformDirection(objForward);       // in camera space
            objUp = mainCamera.transform.InverseTransformDirection(objUp);       		   // in camera space
            objDirection = mainCamera.transform.InverseTransformDirection(objDirection);   // in camera space
            handDirection = mainCamera.transform.InverseTransformDirection(handDirection); // in camera space

            objRefForward = objForward;
            objRefUp = objUp;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = gazeHitPosition;

            initial_pos = HostTransform.position - delta_pos;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private Vector3 GetHandPivotPosition()
        {
            Vector3 pivot = Camera.main.transform.position + new Vector3(0, -0.2f, 0) - Camera.main.transform.forward * 0.2f; // a bit lower and behind
            return pivot;
        }

        /// <summary>
        /// Enables or disables dragging.
        /// </summary>
        /// <param name="isEnabled">Indicates whether dragging shoudl be enabled or disabled.</param>
        public void SetDragging(bool isEnabled)
        {
            if (IsDraggingEnabled == isEnabled)
            {
                return;
            }

            IsDraggingEnabled = isEnabled;

            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            Vector3 newHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

            Vector3 pivotPosition = GetHandPivotPosition();

            Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);


            newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space


            float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);


            float distanceRatio = currenthandDistance / handRefDistance;
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;



            float targetDistance = objRefDistance+ distanceOffset;

            draggingPosition = pivotPosition + (targetDirection * targetDistance);


            if (RotationMode == RotationModeEnum.OrientTowardUser || RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright) 
            {
                draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else if (RotationMode == RotationModeEnum.LockObjectRotation)
            {
                draggingRotation = HostTransform.rotation;
            }
            else // RotationModeEnum.Default
            {
                Vector3 objForward = mainCamera.transform.TransformDirection(objRefForward); // in world space
                Vector3 objUp = mainCamera.transform.TransformDirection(objRefUp);   // in world space
                draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }

            // Apply Final Position
            Vector3 final_position = draggingPosition + mainCamera.transform.TransformDirection(objRefGrabPoint);
            if (lock_x){
                final_position.x = HostTransform.position.x;
            }
            if (lock_z) {
                final_position.y = HostTransform.position.y;
            }
            if (lock_y){
             final_position.z = HostTransform.position.z;
            }

            delta_pos = final_position - initial_pos;
            float norm_ratio = delta_pos.magnitude/ delta_pos_norm_limit;
            // cannot drag past a limit
            if (norm_ratio > 1){
                showWarning(true);
                delta_pos /= norm_ratio;
                final_position = initial_pos + delta_pos;
            } else {
                showWarning(false);
            }
            updateText(delta_pos);

            HostTransform.position = Vector3.Lerp(HostTransform.position, final_position, PositionLerpSpeed);
            // Apply Final Rotation
            HostTransform.rotation = Quaternion.Lerp(HostTransform.rotation, draggingRotation, RotationLerpSpeed);

            if (RotationMode == RotationModeEnum.OrientTowardUserAndKeepUpright)		
            {		
                Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);		
                HostTransform.rotation = upRotation * HostTransform.rotation;		
            }
        }

        private void showWarning(bool val){
            warningSign.SetActive(val);
        }

        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            showWarning(false);

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();

            isDragging = false;
            currentInputSource = null;
            StoppedDragging.RaiseEvent();
        }

        public void OnFocusEnter()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isGazed)
            {
                return;
            }

            isGazed = true;
        }

        public void OnFocusExit()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (!isGazed)
            {
                return;
            }

            isGazed = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null &&
                eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }

            currentInputSource = eventData.InputSource;
            currentInputSourceId = eventData.SourceId;
            StartDragging();
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }
    }
}
