using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCoinEvent : Event
{
    public Aid Coin { get; private set; }

    public void Set(Aid coin)
    {
        Coin = coin;
    }
}
