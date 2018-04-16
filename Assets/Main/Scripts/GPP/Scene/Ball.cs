using System.Collections;
using UnityEngine;

namespace GPP.Scene
{
    public class Ball : MonoBehaviour
    {
        public class Clicked : GameEvent
        {
        }

        private float _moveInterval;
        private float _pulseInterval;

        public void Init(float moveInterval, float pulseInterval)
        {
            _moveInterval = moveInterval;
            _pulseInterval = pulseInterval;
        }

        private void Start()
        {
            StartCoroutine(Behavior());
        }

        private void OnMouseDown()
        {
            Services.Events.Fire(new Clicked());
            StopAllCoroutines();
            StartCoroutine(Die());
        }

        private IEnumerator Behavior()
        {
            while (gameObject.activeInHierarchy)
            {
                // Pulse for a sec
                var startScale = Vector3.one;
                var endScale = Vector3.one * 1.25f;
                yield return Coroutines.DoOverEasedTime(_pulseInterval / 2, Easing.BounceEaseOut,
                    t => { transform.localScale = Vector3.Lerp(startScale, endScale, t); });
                yield return Coroutines.DoOverEasedTime(_pulseInterval / 2, Easing.BounceEaseOut,
                    t => { transform.localScale = Vector3.Lerp(endScale, startScale, t); });


                // Pick a random spot
                var startPos = transform.position;
                var endPos = CameraUtil.RandomPositionInView(transform.position.z);
                // Move there
                yield return Coroutines.DoOverEasedTime(_moveInterval, Easing.CircEaseIn,
                    t => { transform.position = Vector3.Lerp(startPos, endPos, t); });
            }
        }

        private IEnumerator Die()
        {
            // Shrink
            var startScale = Vector3.one;
            var endScale = new Vector3(0.15f, 0.15f, 0.15f);
            yield return Coroutines.DoOverEasedTime(0.25f, Easing.BounceEaseOut,
                t => { transform.localScale = Vector3.Lerp(startScale, endScale, t); });
            Destroy(gameObject);
        }

    }
}