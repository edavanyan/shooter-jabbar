using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera camera;

    [SerializeField] private float _smoothSpeed = 002f;
    [SerializeField] private Vector3 _offset;
    private Vector3 destination;

    public Vector2 WorldToScreenPosition(Vector3 position)
    {
        var viewportPosition = camera.WorldToScreenPoint(position);
        return viewportPosition;
    }

    public void Move(Vector3 position)
    {
        destination = position + _offset;
        var smoothPosition = Vector3.Lerp(transform.position, destination, _smoothSpeed);
        if (smoothPosition.z < -14)
        {
            smoothPosition.z = -14;
        }
        transform.position = smoothPosition;
    }
}
