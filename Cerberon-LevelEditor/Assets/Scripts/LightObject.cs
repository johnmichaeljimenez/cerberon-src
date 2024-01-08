using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightObject : BaseObject
{
    [Range(0f, 1f)] [SerializeField] private float intensity = 1;
    [SerializeField] private bool castShadows;

    public override void Export(List<byte> array)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var scale = Mathf.Max(transform.localScale.x, transform.localScale.y);
        scale = scale * MAP_SCALE;

        array.AddRange(BitConverter.GetBytes(transform.position.x * MAP_SCALE));
        array.AddRange(BitConverter.GetBytes(transform.position.y * MAP_SCALE_Y));
        array.AddRange(BitConverter.GetBytes(transform.eulerAngles.z));
        array.AddRange(BitConverter.GetBytes(scale));

        array.AddRange(BitConverter.GetBytes(castShadows));
        array.AddRange(BitConverter.GetBytes(spriteRenderer.color.r));
        array.AddRange(BitConverter.GetBytes(spriteRenderer.color.g));
        array.AddRange(BitConverter.GetBytes(spriteRenderer.color.b));
        array.AddRange(BitConverter.GetBytes(intensity));
    }

    private void OnDrawGizmosSelected()
    {
        var scale = Mathf.Max(transform.localScale.x, transform.localScale.y) / 2;

        Gizmos.DrawWireSphere(transform.position, scale);
    }
}