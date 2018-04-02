
using WaveField.Enemy;

namespace WaveField.Entity
{

	public class EnemyEntity : HPEntity
	{
		public override void SelfDestroy()
		{
			EnemyGenerator.Instance.AddKill();
			base.SelfDestroy();
		}
	}
}
