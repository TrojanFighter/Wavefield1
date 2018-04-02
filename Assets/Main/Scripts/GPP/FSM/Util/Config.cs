
using UnityEngine;

[CreateAssetMenu (menuName = "Config")]
public class Config : ScriptableObject
{
    [SerializeField] private float _foodGenerateInterval;
    public float FoodGenerateInterval { get { return _foodGenerateInterval; } }

    [SerializeField] private uint _maxFood;
    public float MaxFood { get { return _maxFood; } }

    [SerializeField] private GameObject _foodPrefab;
    public GameObject FoodPrefab { get { return _foodPrefab; } }

    [SerializeField] private float _searchDistance;
    public float SearchDistance { get { return _searchDistance; } }

    [SerializeField] private float _searchSpeed;
    public float SearchSpeed { get { return _searchSpeed; } }

    [SerializeField] private float _trackingSpeed;
    public float TrackingSpeed { get { return _trackingSpeed; } }

    [SerializeField] private float _destinationThreshold;
    public float DestinationThreshold { get { return _destinationThreshold; } }

    [SerializeField] private float _eatingDuration;
    public float EatingDuration { get { return _eatingDuration; } }

    [SerializeField] private Color _eatingColor;
    public Color EatingColor { get { return _eatingColor; } }

    [SerializeField] private float _sleepingDuration;
    public float SleepingDuration { get { return _sleepingDuration; } }

    [SerializeField] private Color _sleepingColor;
    public Color SleepingColor { get { return _sleepingColor; } }
    
    [SerializeField] private Color _searchingColor;
    public Color SearchingColor { get { return _searchingColor; } }
    
    [SerializeField] private Color _trackingColor;
    public Color TrackingColor { get { return _trackingColor; } }

}
