using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCoinEvent : Event
{
    public Coin Coin { get; private set; }

    public void Set(Coin coin)
    {
        Coin = coin;
    }
}
