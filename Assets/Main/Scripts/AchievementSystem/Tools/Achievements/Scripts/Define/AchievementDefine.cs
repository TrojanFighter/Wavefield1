namespace WaveField.Achievement
{
	/// <summary>
	/// A list of the possible base events
	/// </summary>
	public enum AchievementEventTypes
	{
		LevelStart,
		LevelComplete,
		LevelEnd,
		Pause,
		UnPause,
		PlayerDeath,
		Respawn,
		KillAnyEnemy,
		Kill3Enemy,
		ShootBullet
	}

	/// <summary>
	/// A type of events used to signal level start and end (for now)
	/// </summary>
	public struct AchievementEvent
	{
		public AchievementEventTypes EventType;

		/// <summary>
		/// Initializes a new instance of the Achievement struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>
		public AchievementEvent(AchievementEventTypes eventType)
		{
			EventType = eventType;
		}
	}
	
	
}