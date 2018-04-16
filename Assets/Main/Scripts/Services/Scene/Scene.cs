using UnityEngine;

// Scenes are parameterized based on the type of data
// Under this scheme all the scenes associated with a SceneManager
// have to be of the same type.
public class Scene<TTransitionData> : MonoBehaviour
{
    // The scene acts as a container for all the objects in the scene.
    // An easy way to do this in Unity is to attach all scene objects to a root
    // object or one of its children, so you can activate/deactivate them all in one
    // fell swoop
    public GameObject Root { get { return gameObject; } }

    // "Hidden" versions of the enter and exit methods that take care
    // of activating/deactivating the scenes as they transition. They take
    // care of handling that before calling the subclass's methods to avoid
    // saddling all subclasses from having to call super.OnEnter/Exit()
    internal void _OnEnter(TTransitionData data)
    {
        Root.SetActive(true);
        OnEnter(data);
    }

    internal void _OnExit()
    {
        Root.SetActive(false);
        OnExit();
    }

    protected virtual void OnEnter(TTransitionData data) { }
    protected virtual void OnExit() { }

    // NOTE: If you were doing this outside of Unity you would most likely need to
    // implement some lifecycle management methods (e.g. update, destroy, awake, etc.)
    // but why bother replicating that if Unity does it for you

}