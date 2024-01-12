using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : BaseInteractable
{
    public override InteractableType InteractableType => InteractableType.ItemPickup;

    protected override int Count => 1;
    protected override int Flags => 0;
}
