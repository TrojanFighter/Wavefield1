using System.Collections;
using UnityEngine;

public class MonoBehaviourEX<T> : MonoBehaviour  where T : MonoBehaviour
{
	private static T _Instance;

	public static T Instance 
	{
get
	{
    #if UNITY_EDITOR
		if (_Instance == null)
		{
			T instanceInScene = GameObject.FindObjectOfType<T>();
			if (instanceInScene != null)
			{
				_Instance = instanceInScene;
			}
		}
	#endif
		return _Instance;
	}
}

	// Use this for initialization
    public virtual void Awake () {
	    if (_Instance == null)
	    {
		    _Instance = this as T;
	    }
	    else if(!_Instance.Equals(this))
	    {
		    if (Application.isPlaying)
		    {
			    Destroy(this);
		    }
		    else
		    {
			    DestroyImmediate(this);
		    }
	    }
    }

}
