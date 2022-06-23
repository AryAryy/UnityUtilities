using System;
using System.Collections;
using UnityEngine;

namespace Utilities.GameSystems
{
    public class JoystickController : MonoBehaviour
    {
        private static JoystickController instance_;
        public static JoystickController Instance => instance_;

        private Camera mainCam_;
        private Coroutine inputGetCo_;
        private Vector2 inputDirection_;
        private bool gameStarted_;
        
        /// <summary>
        /// A normalized 0 to 1 value that is calculated after applying 'deadzone' and 'maxInputVectorLength'.
        /// Final 0 to 1 value that is fed into 'ArrowController'
        /// </summary>
        private float remappedInput_;
        private bool isTouching_;
        private bool active_;

        [SerializeField] private bool fixedPosition;
        [SerializeField] private Camera joystickCamera;
        [SerializeField, Tooltip("Joystick object")]
        private Transform joystick;
        [SerializeField, Tooltip("Joystick's moving object")]
        private Transform joystickMovingObject;
        [SerializeField, Tooltip("Joystick's distance from joystick camera")]
        private float joystickDistanceFromCam;

        [SerializeField] private float minDistanceToRegisterInput;
        // Both below variables work based on magnitude value of 'inputDirection_'
        [SerializeField, Tooltip("Minimum amount of input needed to register input")]
        private float deadzone;
        [SerializeField, Tooltip("Maximum amount of input needed for maximum input register")]
        private float maxInputVectorLength;
        [SerializeField] private LayerMask itemsLayer;

        public Vector2 RawInputVector => inputDirection_.normalized;
        public Vector2 InputVector => inputDirection_.normalized * remappedInput_;

        public event Action OnTouchStart;
        public event Action OnTouchEnd;

        private void Awake()
        {
            instance_ = this;
            mainCam_ = Camera.main;
            joystickMovingObject.gameObject.SetActive(false);
            //active_ = true;
        }

        /// <summary>
        /// This function activates joystick
        /// </summary>
        private void ActivateJoystick() => ToggleActivationStatus(true);

        /// <summary>
        /// This function deactivates joystick
        /// </summary>
        private void DeactivateJoystick() => ToggleActivationStatus(false);
        
        private void Update()
        {
            if (!gameStarted_ || !active_) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                isTouching_ = true;
                if (inputGetCo_ != null) StopCoroutine(inputGetCo_);
                inputGetCo_ = StartCoroutine(GetInputRoutine());
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnTouchEnd?.Invoke();
                isTouching_ = false;
            }
        }
        
        /// <summary>
        /// This function moves 'joystick' to touch start position, moves 'joystickMovingPart' wherever we have touch,
        /// calculated joystick direction, applies 'deadzone' and 'maxInputVectorLength' and calculates a
        /// normalized 0 to 1 value that can be used to determine input magnitude
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetInputRoutine()
        {
            if (!fixedPosition) SetJoystickStartPosition(GetMousePosition());
            float distance = Vector3.Distance(GetMousePosition(), joystick.position);
            if (distance > minDistanceToRegisterInput)
            {
                isTouching_ = false;
                yield break;
            }

            OnTouchStart?.Invoke();
            while (isTouching_)
            {
                MoveJoystick(GetMousePosition());
                
                inputDirection_ = joystickMovingObject.position - joystick.position;
                if (inputDirection_.magnitude >= deadzone)
                {
                    float inputMagnitude = Mathf.Clamp(inputDirection_.magnitude / maxInputVectorLength, deadzone,
                        maxInputVectorLength);
                    remappedInput_ = (inputMagnitude - deadzone) / (maxInputVectorLength - deadzone);
                }
                else remappedInput_ = 0;
                
                yield return null;
            }

            remappedInput_ = 0;
            inputDirection_ = Vector2.zero;
            inputGetCo_ = null;
        }

        /// <summary>
        /// Get current mouse position
        /// </summary>
        /// <returns></returns>
        private Vector3 GetMousePosition()
        {
            Ray ray = joystickCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 pos = joystickCamera.transform.position + ray.direction * joystickDistanceFromCam;
            return pos;
        }

        /// <summary>
        /// his function should get called once the touch input starts, it sets the initial position of joystick 
        /// </summary>
        /// <param name="startPosition"> Initial position of joystick </param>
        private void SetJoystickStartPosition(Vector3 startPosition) => joystick.position = startPosition;

        /// <summary>
        /// Moves joystick to target position
        /// </summary>
        /// <param name="pos"> Target position </param>
        private void MoveJoystick(Vector3 pos) => joystickMovingObject.position = pos;

        /// <summary>
        /// This function activates/deactivated joystick
        /// </summary>
        /// <param name="status"> Activation status </param>
        private void ToggleActivationStatus(bool status)
        {
            active_ = status;
            isTouching_ = status;
            joystickMovingObject.gameObject.SetActive(status);
            OnTouchEnd?.Invoke();
        }
    }
}