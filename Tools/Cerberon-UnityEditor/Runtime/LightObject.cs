using System;
using UnityEngine;

namespace CerberonEditor.Main
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class LightObject : MonoBehaviour
	{
		public enum VisionEffects
		{
			Light,
			VisionOnly,
		}

		public enum ShadowTypes
		{
			None,
			Static,
			Dynamic
		}

		public bool Flicker;
		public string GroupID = "";
		[Range(0f, 1f)]
		public float AmbientMultiplier;
		public ShadowTypes ShadowType;
	}
}