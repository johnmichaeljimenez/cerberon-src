using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TriggerObject : BaseObject
{
    [SerializeField] private BaseInteractable TargetObject;
    [SerializeField] private bool OneShot;
    [SerializeField] private float Cooldown;

    public override void Export(List<byte> array)
    {
        var colliders = GetComponentsInChildren<BoxCollider2D>().Where(p => p.GetComponent<SpriteRenderer>() != null).ToArray();
        var target = (TargetObject == null ? "" : TargetObject.gameObject.name);

        array.AddRange(Encoding.ASCII.GetBytes(target.ToFixedLength(32)));
        array.AddRange(BitConverter.GetBytes(OneShot));
        array.AddRange(BitConverter.GetBytes(Cooldown));
        array.AddRange(BitConverter.GetBytes(colliders.Length));

        foreach (var i in colliders)
        {
            var sprite = i.GetComponent<SpriteRenderer>();

            array.AddRange(BitConverter.GetBytes(i.transform.position.x * MAP_SCALE));
            array.AddRange(BitConverter.GetBytes(i.transform.position.y * MAP_SCALE_Y));
            array.AddRange(BitConverter.GetBytes(i.transform.eulerAngles.z));
            array.AddRange(BitConverter.GetBytes(sprite.size.x * MAP_SCALE));
            array.AddRange(BitConverter.GetBytes(sprite.size.y * MAP_SCALE));
        }
    }

    private void OnDrawGizmos()
    {
        if (TargetObject != null)
        {
            Gizmos.color = Color.gray;
            Utils.DrawArrow(transform.position, TargetObject.transform.position);
            Gizmos.color = Color.white;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (TargetObject != null)
        {
            Gizmos.color = Color.green;
            Utils.DrawArrow(transform.position, TargetObject.transform.position);
            Gizmos.color = Color.white;
        }
    }
}
