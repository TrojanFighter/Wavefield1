using UnityEngine;

namespace GPP.Scene
{

	public class DifficultyButton : MonoBehaviour
	{
		[SerializeField] private Difficulty _difficulty;

		private void Awake()
		{
			GetComponent<Renderer>().material.color = _difficulty.ButtonColor;
		}

		private void OnMouseDown()
		{
			Services.Scenes.PushScene<GameScene>(new TransitionData(_difficulty));
		}
	}
}