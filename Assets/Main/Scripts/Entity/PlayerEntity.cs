
using WaveField.Enemy;

namespace WaveField.Entity
{

	public class PlayerEntity : HPEntity
	{
		public override void SelfDestroy()
		{
			EnemyGenerator.Instance.StopGame();
			base.SelfDestroy();
		}
	}
}
