using System;
using UnityEngine;
using System.Collections.Generic;

namespace CerberonEditor.Main
{
	public class PropObject : EntityObject
	{
		public Dictionary<string, object> GetProperties()
		{
			var spr = GetComponent<SpriteRenderer>();
			var c = GetComponent<BoxCollider2D>();
			return new Dictionary<string, object>()
			{
				{
					"IsActive",
					gameObject.activeInHierarchy
				},
				{
					"ColliderSize", new
					{
						X = c.size.x,
						Y = c.size.y,
					}
				},
				{
					"CurrentSpriteID",
					$"env/{spr.sprite.name}"
				},
			};
		}
	}
}