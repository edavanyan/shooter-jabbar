using UnityEngine.InputSystem;

public class PlayerEvent : Event
{
    public PlayerInput PlayerInput { get; private set; }
    public void Set(PlayerInput playerInput)
    {
        PlayerInput = playerInput;
    }
}
