using UnityEngine;
using System.Collections;
using Wavefield.Tools;
using UnityEditor;

namespace Wavefield.Tools
{	
	public static class MMAchievementMenu 
	{
		[MenuItem("Reset all achievements",false,21)]
		/// <summary>
		/// Adds a menu item to enable help
		/// </summary>
		private static void EnableHelpInInspectors()
		{
			MMAchievementManager.ResetAllAchievements ();
		}
	}
}