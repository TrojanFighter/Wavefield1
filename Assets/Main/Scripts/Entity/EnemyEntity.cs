
using Wavefield.Tools;
using WaveField.Achievement;
using WaveField.Enemy;

namespace WaveField.Entity
{

	public class EnemyEntity : HPEntity
	{
		public override void SelfDestroy()
		{
			MMEventManager.TriggerEvent (new AchievementEvent(AchievementEventTypes.Kill3Enemy));
			MMEventManager.TriggerEvent (new AchievementEvent(AchievementEventTypes.KillAnyEnemy));
			EnemyGenerator.Instance.AddKill();
			base.SelfDestroy();
		}
	}
}
