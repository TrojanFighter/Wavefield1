using System.Collections;
using System.Collections.Generic;
using WaveField.Entity;


namespace WaveField.Services
{

	public class GameEntityManager : ServiceBase
	{
		private Dictionary<int, EntityBase> m_entityDic;

		public int nextEnitityId = 0;

		public GameEntityManager()
		{
			Init();
		}

		// Use this for initialization
		public override void Init() {
			m_entityDic=new Dictionary<int, EntityBase>();
		}
	
		// Update is called once per frame
		public override void Update() {
		
		}

		public override void Destroy()
		{
		}

		public int RegisterEntity(EntityBase entity)
		{
			if (!m_entityDic.ContainsValue(entity))
			{
				nextEnitityId++;
				m_entityDic.Add(nextEnitityId,entity);
				return nextEnitityId;
			}
			else
			{
				return -1;
			}
		}
		
		public bool UnRegisterEntity(int entityid)
		{
			if (!m_entityDic.ContainsKey(entityid))
			{
				return false;
			}
			else
			{
				m_entityDic.Remove(entityid);
				return true;
			}
		}
	}
}