using UnityEngine;
using UnityEngine.Assertions;
using WaveField.Scene;

namespace GPP.Scene
{
    public class Main : MonoBehaviour
    {
        private void Awake()
        {
            Assert.raiseExceptions = true;

            var aspectRatio = new CameraUtil.AspectRatio(16, 9);
            CameraUtil.SetupAspectCamera(aspectRatio);

            Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
            Services.Events = new EventManager();
            Services.Scenes = new SceneManager<TransitionData>(gameObject, Services.Prefabs.Levels);

            Services.Scenes.PushScene<SplashScene>();
        }


        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) Services.Events.Fire(new MouseDownEvent(0));
            if (Input.GetMouseButtonDown(1)) Services.Events.Fire(new MouseDownEvent(1));
        }
    }

    public class MouseDownEvent : GameEvent
    {
        public readonly int button;

        public MouseDownEvent(int button)
        {
            this.button = button;
        }
    }
}