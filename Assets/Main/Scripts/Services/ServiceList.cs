using WaveField.Services.Task;
using WaveField.Services.Event;
using UnityEngine;
using WaveField.Services.Achievement;

namespace WaveField.Services
{
	public static class ServiceList
	{
		public static GameSceneManager<TransitionData> GameSceneManager;
		
		private static GameEntityManager _gameEntityManager;

		public static GameEntityManager GameEntityManager
		{
			get
			{
				Debug.Assert(_gameEntityManager != null);
				return _gameEntityManager;
			}
			set { _gameEntityManager = value; }
		}
		
		private static TaskManager _taskManager;

		public static TaskManager TaskManager
		{
			get
			{
				Debug.Assert(_taskManager != null);
				return _taskManager;
			}
			set { _taskManager = value; }
		}
		
		private static EventManager _eventManager;

		public static EventManager EventManager
		{
			get
			{
				Debug.Assert(_eventManager != null);
				return _eventManager;
			}
			set { _eventManager = value; }
		}
		
		private static AchievementManager _achievementManager;

		public static AchievementManager AchievementManager
		{
			get
			{
				Debug.Assert(_achievementManager != null);
				return _achievementManager;
			}
			set { _achievementManager = value; }
		}

		// etc... 
	}
}