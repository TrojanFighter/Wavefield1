using UnityEngine;
using System.Collections;
using Wavefield.Tools;

namespace WaveField.Achievement
{
	/// <summary>
	/// This class describes how the achievements are triggered.
	/// It extends the base class MMAchievementRules
	/// It listens for different event types
	/// </summary>
	public class AchievementRules : MMAchievementRules, 
									MMEventListener<MMGameEvent>, 
									MMEventListener<AchievementEvent>//,

	{

		public override void OnMMEvent(MMGameEvent gameEvent)
		{

			base.OnMMEvent (gameEvent);

		}



		public virtual void OnMMEvent(AchievementEvent achievementEvent)
		{
			switch (achievementEvent.EventType)
			{

				case AchievementEventTypes.PlayerDeath:
					MMAchievementManager.UnlockAchievement ("DeathIsOnlyTheBeginning");
					break;
				case AchievementEventTypes.KillAnyEnemy:
					MMAchievementManager.UnlockAchievement ("Kill Anyone");
					break;
				case AchievementEventTypes.Kill3Enemy:
					MMAchievementManager.AddProgress("Kill 3 Enemies",1);
					break;
				case AchievementEventTypes.ShootBullet:
					MMAchievementManager.UnlockAchievement ("Shoot a bullet");
					break;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable ();
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<AchievementEvent>();

		}


		protected override void OnDisable()
		{
			base.OnDisable ();

			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<AchievementEvent>();

		}

	}
}