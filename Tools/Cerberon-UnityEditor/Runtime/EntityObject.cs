using System;
using UnityEngine;

namespace CerberonEditor.Main
{
	public class EntityObject : MonoBehaviour
	{
		public string NameTag = "";
		public string EntityType = "Item";

		//TEMPORARY
		[TextArea]
		public string Properties;
	}
}