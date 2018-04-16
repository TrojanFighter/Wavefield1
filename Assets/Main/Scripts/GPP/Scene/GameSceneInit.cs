using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaveField.Scene;

namespace WaveField.Services
{
	public class GameSceneInit : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

			ServiceList.GameSceneManager.PushScene<StartScene>();
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}