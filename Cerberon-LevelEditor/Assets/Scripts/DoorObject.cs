using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DoorObject : BaseInteractable
{
    public override InteractableType InteractableType => InteractableType.Door;

    public override void Export(List<byte> array)
    {
        base.Export(array);
        //TODO: add key id
    }
}
