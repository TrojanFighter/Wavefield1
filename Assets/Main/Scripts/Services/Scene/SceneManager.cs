using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace WaveField.Scene
{



// The scene manager is parameterized on the type of data that scenes pass back and forth
// when entering and exiting. This is mostly there to help with type safety and ensure that
// you only use scenes of a compatible type in the same manager.
    public class SceneManager<TTransitionData>
    {
        // The scene manager has two basic functions:
        // 1. Managing the stack of scenes
        // 2. Forwarding events to the current scene

        // We need somewhere where we can add or remove scenes to the
        // hierarchy, and whoever creates a scene manager is responsible
        // for letting the SceneManager know which GameObject to use
        internal GameObject SceneRoot { get; private set; }

        // Along with the root we need to let the manager know what scenes it will
        // be managing. I'm using prefabs to store the scenes so I'll keep a
        // dictionary of them indexed by their type.
        private readonly Dictionary<Type, GameObject> _scenes = new Dictionary<Type, GameObject>();

        public SceneManager(GameObject root, IEnumerable<GameObject> scenePrefabs)
        {
            SceneRoot = root;
            // Go through the list of scenes and enter them into the dictionary so we
            // can retrieve them by type later
            foreach (var prefab in scenePrefabs)
            {
                // We use the base class to reference the scene in the prefab...
                var scene = prefab.GetComponent<Scene<TTransitionData>>();
                // throw an error if the prefab is missing a scene script
                Assert.IsNotNull(scene, "Could not find scene script in prefab used to initialize SceneManager");
                // and if we're all good, we get the scene's specific type to add it to
                // our scene dictionary
                _scenes.Add(scene.GetType(), prefab);
            }
        }

        // The active scenes in the game are organized in a stack representing a history of scenes...
        private readonly Stack<Scene<TTransitionData>> _sceneStack = new Stack<Scene<TTransitionData>>();

        // With the topmost scene being the "current" one that is being interacted with by the user.
        public Scene<TTransitionData> CurrentScene
        {
            get { return _sceneStack.Count != 0 ? _sceneStack.Peek() : null; }
        }

        // Like any stack-based structure worth its salt, the scene manager allows you to
        // push and pop elements - in this case scenes
        public void PopScene(TTransitionData data = default(TTransitionData))
        {
            // Note that the scene manager needs to inform scenes about which scene is replacing,
            // and preceding them so there's a little more work to be done in the push and pop
            // methods than usual
            Scene<TTransitionData> previousScene = null;
            Scene<TTransitionData> nextScene = null;

            if (_sceneStack.Count != 0)
            {
                previousScene = _sceneStack.Peek();
                _sceneStack.Pop();
            }

            if (_sceneStack.Count != 0)
            {
                nextScene = _sceneStack.Peek();
            }

            if (nextScene != null)
            {
                nextScene._OnEnter(data);
            }

            if (previousScene != null)
            {
                Object.Destroy(previousScene.Root);
                previousScene._OnExit();
            }
        }

        // The push scene method creates the scene
        public void PushScene<T>(TTransitionData data = default(TTransitionData)) where T : Scene<TTransitionData>
        {
            var previousScene = CurrentScene;
            var nextScene = GetScene<T>();

            _sceneStack.Push(nextScene);
            nextScene._OnEnter(data);

            if (previousScene != null)
            {
                previousScene._OnExit();
                previousScene.Root.SetActive(false);
            }
        }

        // The scenemanager also has a swap method to be used when you want
        // to replace the top scene on the stack rather than try to create
        // push it down. This comes up a lot when your scenes are ephemeral
        // or require an ephemeral context so going back to them doesn't make
        // sense (e.g. going back to a loading screen when the game is done loading)
        public void Swap<T>(TTransitionData data = default(TTransitionData)) where T : Scene<TTransitionData>
        {
            Scene<TTransitionData> previousScene = null;
            if (_sceneStack.Count > 0)
            {
                previousScene = _sceneStack.Peek();
                _sceneStack.Pop();
            }

            var nextScene = GetScene<T>();
            _sceneStack.Push(nextScene);
            nextScene._OnEnter(data);

            if (previousScene != null)
            {
                previousScene._OnExit();
                Object.Destroy(previousScene.Root);
            }
        }

        // GetScene is just a helper method for creating the proper scene
        // and attaching it to the scene-graph
        private T GetScene<T>() where T : Scene<TTransitionData>
        {
            GameObject prefab;
            _scenes.TryGetValue(typeof(T), out prefab);
            Assert.IsNotNull(prefab, "Could not find scene prefab for scene type: " + typeof(T).Name);

            var sceneObject = Object.Instantiate(prefab);
            sceneObject.name = typeof(T).Name;
            sceneObject.transform.SetParent(SceneRoot.transform, false);
            return sceneObject.GetComponent<T>();
        }

    }
}