using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMedkit : BaseInteractable
{
    public override InteractableType InteractableType => InteractableType.ItemPickup;
    public override InteractableSubType InteractableSubType => InteractableSubType.ItemMedkit;

    protected override int Count => 1;
    protected override int Flags => 0;

    public override void Export(List<byte> array)
    {
        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));


    }
}
