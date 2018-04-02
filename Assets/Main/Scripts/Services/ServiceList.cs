using WaveField.Services.Task;
using WaveField.Services.Event;
using UnityEngine;

namespace WaveField.Services
{
	public static class ServiceList
	{
		// Renderer     
		private static GameSceneManager _gameSceneManager;

		public static GameSceneManager GameSceneManager
		{
			get
			{
				Debug.Assert(_gameSceneManager != null);
				return _gameSceneManager;
			}
			set { _gameSceneManager = value; }
		}
		
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

		// etc... 
	}
}