using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour, IInputManager
{
    public event Action<Vector3> OnMouseClick;
    
    [SerializeField] private LayerMask layerMask;
    
    private void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            var touchPosition = Input.mousePosition;
            var ray = GameManager.Instance.Camera.camera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out var hit, layerMask))
            {

                OnMouseClick?.Invoke(hit.point);
            }
        }
    }
}
