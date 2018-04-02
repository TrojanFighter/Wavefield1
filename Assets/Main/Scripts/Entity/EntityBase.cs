
using UnityEngine;
using WaveField.Services;

namespace WaveField.Entity
{
	public class EntityBase : MonoBehaviour
	{
		public int entityId = -1;

		private bool inited = false;

		protected virtual void Start()
		{
			Init();
		}


		public virtual bool Init()
		{
			if (inited)
			{
				return false;
			}

			int id= Services.ServiceList.GameEntityManager.RegisterEntity(this);
			if (id != -1)
			{
				entityId = id;
				inited = true;
				return true;
			}
			else
			{
				Debug.Log("EntityCreationFailed: "+gameObject.name);
				return false;
			}
		}

		public virtual void SelfDestroy()
		{
			ServiceList.GameEntityManager.UnRegisterEntity(entityId);
			Destroy(gameObject);
		}
	}
}
