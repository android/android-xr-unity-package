using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace XrRigResource.Demo
{
    [RequireComponent(typeof(VstControllerLedControl))]
    public class ControllerLedTest : MonoBehaviour
    {
        [SerializeField] private bool enableTest;
        [SerializeField] private InputAction changeColorRandomAction;
        [SerializeField] private InputAction changeBlinkAction;
        [SerializeField] private InputAction resetAction;
        
        private VstControllerLedControl _controller;


        private void Awake()
        {
            _controller = GetComponent<VstControllerLedControl>();
        }


        private void OnEnable()
        {
            changeColorRandomAction.Enable();
            changeBlinkAction.Enable();
            resetAction.Enable();
            changeColorRandomAction.performed += OnChangeColorRandomHandler;
            changeBlinkAction.performed += OnChangeBlinkHandler;
            resetAction.performed += OnResetHandler;
        }
        
        
        private void OnDisable()
        {
            changeColorRandomAction.Disable();
            changeBlinkAction.Disable();
            resetAction.Disable();
            changeColorRandomAction.performed -= OnChangeColorRandomHandler;
            changeBlinkAction.performed -= OnChangeBlinkHandler;
            resetAction.performed -= OnResetHandler;
        }

        
        private void OnChangeColorRandomHandler(InputAction.CallbackContext obj)
        {
            if(enableTest == false) return;
            
            Color targetColor = Random.ColorHSV();
            targetColor.a = 1f;
            _controller.SetStaticColor(targetColor, 0.5f);
        }
        

        private void OnChangeBlinkHandler(InputAction.CallbackContext obj)
        {
            if(enableTest == false) return;
            
            _controller.SetBlinkColor(Color.red, Color.blue, 0.5f);
        }
        
        
        private void OnResetHandler(InputAction.CallbackContext obj)
        {
            if(enableTest == false) return;
            
            _controller.Reset();
        }
    }
}
