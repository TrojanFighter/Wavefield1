
using System.Collections;
using UnityEngine;

namespace GPP.Scene
{
    public class GameScene : Scene<TransitionData>
    {
        private Difficulty _difficulty;
        private int _score;
        private int _numActiveBalls;
        [SerializeField] private TextMesh _timeLabel;
        [SerializeField] private TextMesh _scoreLabel;

        protected override void OnEnter(TransitionData data)
        {
            _difficulty = data.difficulty;
            Services.Events.AddHandler<Ball.Clicked>(OnBallClicked);
        }

        protected override void OnExit()
        {
            Services.Events.RemoveHandler<Ball.Clicked>(OnBallClicked);
        }

        public void Start()
        {
            while (_numActiveBalls < _difficulty.NumBalls)
            {
                CreateBall();
            }

            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            for (var i = _difficulty.Duration; i > 0; i--)
            {
                _timeLabel.text = i.ToString();
                yield return new WaitForSeconds(1);
            }

            Services.Scenes.Swap<GameOverScene>(new TransitionData(null, _score));
        }

        private void CreateBall()
        {
            var pos = CameraUtil.RandomPositionInView(transform.position.z);
            pos.z = transform.position.z;
            var ball = Instantiate(Services.Prefabs.Ball);
            ball.transform.SetParent(transform, false);
            ball.transform.position = pos;
            ball.GetComponent<Ball>().Init(_difficulty.MoveInterval, _difficulty.PulseInterval);
            _numActiveBalls++;
        }

        private void OnBallClicked(Ball.Clicked evt)
        {
            _score += _difficulty.PointsPerBall;
            _scoreLabel.text = _score.ToString();
            _numActiveBalls--;
            if (_numActiveBalls < _difficulty.NumBalls)
            {
                CreateBall();
            }
        }

    }
}