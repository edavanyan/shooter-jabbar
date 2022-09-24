using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour, IInputManager
{
    public event Action<Vector3> OnMouseClick;
    [SerializeField]
    private Joystick joystick;
    
    [SerializeField] private LayerMask layerMask;

    [DllImport("__Internal")]
    private static extern bool IsMobile();
    private void Awake()
    {
        if (IsRunOnMobile())
        {
            joystick.gameObject.SetActive(true);
        }
    }

    private bool IsRunOnMobile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return InputManager.IsMobile();
#else
        return false;
#endif
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left) || Input.touchCount > 1)
        {
            bool joyStickTouched = false;
            if (joystick.gameObject.activeSelf)
            {
                if (joystick.JoystickBounds.x > Input.mousePosition.x && joystick.JoystickBounds.y > Input.mousePosition.y)
                {
                    joystick.Pressed();
                    joyStickTouched = true;
                }
            }

            var touchPosition = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                touchPosition = Input.touches[Input.touchCount - 1].position;
            }

            if (!joyStickTouched || Input.touchCount > 1)
            {
                var ray = GameManager.Instance.Camera.camera.ScreenPointToRay(touchPosition);
                if (Physics.Raycast(ray, out var hit, layerMask))
                {
                    OnMouseClick?.Invoke(hit.point);
                }
            }
        }
    }
}
