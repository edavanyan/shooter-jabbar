using System;
using Unity.VisualScripting;
using UnityEngine;

public interface IInputManager
{
    event Action<Vector3> OnMouseClick;
}
