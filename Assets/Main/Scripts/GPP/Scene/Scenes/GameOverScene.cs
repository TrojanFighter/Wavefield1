
using UnityEngine;

namespace GPP.Scene
{
    public class GameOverScene : Scene<TransitionData>
    {
        [SerializeField] private TextMesh _scoreLabel;

        protected override void OnEnter(TransitionData data)
        {
            _scoreLabel.text = data.score.ToString();
            Services.Events.AddHandler<MouseDownEvent>(OnClick);
        }

        protected override void OnExit()
        {
            Services.Events.RemoveHandler<MouseDownEvent>(OnClick);
        }

        private void OnClick(MouseDownEvent evt)
        {
            Services.Scenes.PopScene();
        }

    }
}