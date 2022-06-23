using System;
using System.Collections;
using UnityEngine;

namespace UnityUtilitiesCode.GameSystems
{
    public class JoystickController : MonoBehaviour
    {
        private Coroutine inputGetCo_;
        private Vector3 inputDirection_;
        private bool gameStarted_;
        
        /// <summary>
        /// A normalized 0 to 1 value that is calculated after applying 'deadzone' and 'maxInputVectorLength'.
        /// Final 0 to 1 value that is fed into 'ArrowController'
        /// </summary>
        private float remappedInput_;
        private bool isTouching_;
        
        [SerializeField] private Camera joystickCamera;
        [SerializeField, Tooltip("Joystick object")]
        private Transform joystick;
        [SerializeField, Tooltip("Joystick's moving object")]
        private Transform joystickMovingObject;
        [SerializeField, Tooltip("Joystick's distance from joystick camera")]
        private float joystickDistanceFromCam;

        // Both below variables work based on magnitude value of 'inputDirection_'
        [SerializeField, Tooltip("Minimum amount of input needed to register input")]
        private float deadzone;
        [SerializeField, Tooltip("Maximum amount of input needed for maximum input register")]
        private float maxInputVectorLength;

        public Vector3 InputVector => inputDirection_.normalized * remappedInput_;

        public event Action OnGrabArrow;
        public event Action OnReleaseArrow;

        //private void Start() => LevelManager.Instance.OnStartGame += StartGame;

        private void StartGame() => gameStarted_ = true;
        
        private void Update()
        {
            if (!gameStarted_) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                OnGrabArrow?.Invoke();
                isTouching_ = true;
                inputGetCo_ = StartCoroutine(GetInputRoutine());
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnReleaseArrow?.Invoke();
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
            SetJoystickStartPosition(GetMousePosition());
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
            inputDirection_ = Vector3.zero;
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
    }
}