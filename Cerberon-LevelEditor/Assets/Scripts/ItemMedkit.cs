using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMedkit : BaseItem
{
    public override InteractableType InteractableType => InteractableType.ItemPickup;
    public override InteractableSubType InteractableSubType => InteractableSubType.ItemMedkit;

    protected override int Count => 1;
    protected override int Flags => 0;

    public override void Export(List<byte> array)
    {
        base.Export(array);
    }
}
