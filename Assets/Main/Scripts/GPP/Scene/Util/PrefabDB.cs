using UnityEngine;

[CreateAssetMenu (menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject
{

    [SerializeField] private GameObject _ball;
    public GameObject Ball
    {
        get { return _ball; }
    }

    [SerializeField] private GameObject[] _levels;
    public GameObject[] Levels
    {
        get { return _levels; }
    }

}