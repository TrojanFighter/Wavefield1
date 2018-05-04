
using Wavefield.Tools;
using WaveField.Achievement;
using WaveField.Enemy;

namespace WaveField.Entity
{

	public class PlayerEntity : HPEntity
	{
		public override void SelfDestroy()
		{
			MMEventManager.TriggerEvent(new AchievementEvent(AchievementEventTypes.PlayerDeath));
			EnemyGenerator.Instance.StopGame();
			base.SelfDestroy();
		}
	}
}
