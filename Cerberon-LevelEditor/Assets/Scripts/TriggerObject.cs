using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TriggerObject : BaseObject
{
    [SerializeField] private BaseInteractable TargetObject;
    [SerializeField] private bool OneShot;
    [SerializeField] private float Cooldown;

    public override void Export(List<byte> array)
    {
        var colliders = GetComponentsInChildren<BoxCollider2D>();
        var target = (TargetObject == null ? "" : TargetObject.gameObject.name);

        array.AddRange(Encoding.ASCII.GetBytes(target.ToFixedLength(32)));
        array.AddRange(BitConverter.GetBytes(OneShot));
        array.AddRange(BitConverter.GetBytes(Cooldown));
        array.AddRange(BitConverter.GetBytes(colliders.Length));

        foreach (var i in colliders)
        {
            array.AddRange(BitConverter.GetBytes(i.transform.position.x * MAP_SCALE));
            array.AddRange(BitConverter.GetBytes(i.transform.position.y * MAP_SCALE_Y));
            array.AddRange(BitConverter.GetBytes(-transform.eulerAngles.z));
            array.AddRange(BitConverter.GetBytes(i.size.x));
            array.AddRange(BitConverter.GetBytes(i.size.y));
        }
    }
}
