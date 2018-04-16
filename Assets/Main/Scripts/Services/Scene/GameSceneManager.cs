using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using WaveField.Scene;
using Object = UnityEngine.Object;


namespace WaveField.Services
{

	public class GameSceneManager: ServiceBase
	{
		public SceneManager<TransitionData> sceneManager
		{
			set;
			get;
		}

		public GameSceneManager()
		{
			Init();
		}	

	    // Use this for initialization
	    public override void Init() {
		    
	    }
	
	    // Update is called once per frame
	    public override void Update() {
		
	    }

	    public override void Destroy()
	    {
	    }
		

    }
}
