using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum InteractableType
{
    Door,
    ItemPickup
}

public enum InteractableSubType
{
    Door,
    ItemMedkit,
    ItemFlashlight,
    ItemCash
}

public class BaseInteractable : BaseObject
{
    public virtual InteractableType InteractableType => throw new NotImplementedException();
    public virtual InteractableSubType InteractableSubType => throw new NotImplementedException();

    [SerializeField] protected BaseInteractable TargetObject;
    [SerializeField] protected float Delay;
    [SerializeField] protected bool OneShot;

    protected virtual int Count => 1;
    protected virtual int Flags => 0;

    public override void Export(List<byte> array)
    {
        var target = gameObject.name;
        var targetName = (TargetObject == null? "" : TargetObject.gameObject.name);

        var n = (int)InteractableType;
        array.AddRange(BitConverter.GetBytes(n));
        n = (int)InteractableSubType;
        array.AddRange(BitConverter.GetBytes(n));
        array.AddRange(BitConverter.GetBytes(Count));
        array.AddRange(BitConverter.GetBytes(Flags));
        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));
        array.AddRange(Encoding.ASCII.GetBytes(target.ToFixedLength(32)));
        array.AddRange(Encoding.ASCII.GetBytes(targetName.ToFixedLength(32)));
    }
}
