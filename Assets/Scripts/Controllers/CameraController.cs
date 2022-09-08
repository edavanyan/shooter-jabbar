using UnityEngine;

public class CameraController : MonoBehaviour, EventListener
{
    private bool _isInGame = false;//to avoid expensive null check in update 

    [SerializeField] private float _smoothSpeed = 002f;
    [SerializeField] private Vector3 _offset;

    void Start()
    {
        GameManager.Instance.Events.RegisterObserver(this);
    }

    void LateUpdate()
    {
        if (_isInGame)
        {
            var destination = GameManager.Instance.Player.transform.position + _offset;
            var smoothPosition = Vector3.Lerp(transform.position, destination, _smoothSpeed);
            if (smoothPosition.z < -14)
            {
                smoothPosition.z = -14;
            }
            transform.position = smoothPosition;
        }
    }

    [EventHandler]
    void OnPlayerJoined(PlayerJoinedEvent playerJoinedEvent)
    {
        _isInGame = _isInGame || playerJoinedEvent.IsMe;
    }
}
