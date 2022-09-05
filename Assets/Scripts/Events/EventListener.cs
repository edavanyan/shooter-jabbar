using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface EventListener
{
}

[AttributeUsage(AttributeTargets.Method)]
public class EventHandler : Attribute
{
    
}
