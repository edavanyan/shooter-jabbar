using UnityEngine.InputSystem;

public class PlayerEvent : Event
{
    public PlayerController Player { get; private set; }
    public virtual void Set(PlayerController player)
    {
        Player = player;
    }
}
