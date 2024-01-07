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

public class BaseInteractable : BaseObject
{
    public virtual InteractableType InteractableType => throw new NotImplementedException();

    [SerializeField] protected string Target;
    [SerializeField] protected string TargetName;
    [SerializeField] protected float Delay;
    [SerializeField] protected bool OneShot;

    protected virtual int Flags => 0;

    public override void Export(List<byte> array)
    {
        array.AddRange(BitConverter.GetBytes((int)InteractableType));
        array.AddRange(BitConverter.GetBytes(Flags));
        array.AddRange(BitConverter.GetBytes(transform.position.x));
        array.AddRange(BitConverter.GetBytes(transform.position.y));
        array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));
        array.AddRange(Encoding.ASCII.GetBytes(Target.ToFixedLength(31)));
        array.AddRange(Encoding.ASCII.GetBytes(TargetName.ToFixedLength(31)));
    }
}
