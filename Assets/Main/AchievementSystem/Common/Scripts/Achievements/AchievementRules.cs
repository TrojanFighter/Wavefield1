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
									//MMEventListener<MMCharacterEvent>, 
									MMEventListener<AchievementEvent>//,
									//MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>,
									//MMEventListener<MMStateChangeEvent<CharacterStates.CharacterConditions>>,
									//MMEventListener<PickableItemEvent>
	{
		/// <summary>
		/// When we catch an MMGameEvent, we do stuff based on its name
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public override void OnMMEvent(MMGameEvent gameEvent)
		{

			base.OnMMEvent (gameEvent);

		}

		/*
		public virtual void OnMMEvent(MMCharacterEvent characterEvent)
		{
			if (characterEvent.TargetCharacter.CharacterType == Character.CharacterTypes.Player)
			{
				switch (characterEvent.EventType)
				{
					case MMCharacterEventTypes.Jump:
						MMAchievementManager.AddProgress ("JumpAround", 1);
						break;
				}	
			}
		}*/

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

		/*
		public virtual void OnMMEvent(PickableItemEvent pickableItemEvent)
		{
			if (pickableItemEvent.PickedItem != null)
			{
				if (pickableItemEvent.PickedItem.GetComponent<Coin>() != null)
				{
					MMAchievementManager.AddProgress ("MoneyMoneyMoney", 1);
				}
				if (pickableItemEvent.PickedItem.GetComponent<Stimpack>() != null)
				{
					MMAchievementManager.UnlockAchievement ("Medic");
				}
			}
		}

		public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> movementEvent)
		{
			//switch (movementEvent.NewState)
			{

			}
		}

		public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.CharacterConditions> conditionEvent)
		{
			//switch (conditionEvent.NewState)
			{

			}
		}*/

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable ();
			//this.MMEventStartListening<MMCharacterEvent>();
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<AchievementEvent>();
			//this.MMEventStartListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			//this.MMEventStartListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
			//this.MMEventStartListening<PickableItemEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			//this.MMEventStopListening<MMCharacterEvent>();
			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<AchievementEvent>();
			//this.MMEventStopListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			//this.MMEventStopListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
			//this.MMEventStopListening<PickableItemEvent>();
		}

	}
}