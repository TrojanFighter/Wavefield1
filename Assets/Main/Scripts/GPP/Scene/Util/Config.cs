
using System;
using UnityEngine;


[CreateAssetMenu (menuName = "Difficulty")]
[Serializable]
public class Difficulty : ScriptableObject
{
    [SerializeField] private int _pointsPerBall;
    public int PointsPerBall {get { return _pointsPerBall; }}

    [SerializeField] private float _moveInterval;
    public float MoveInterval {get { return _moveInterval; }}

    [SerializeField] private float _pulseInterval;
    public float PulseInterval {get { return _pulseInterval; }}

    [SerializeField] private int _numBalls;
    public int NumBalls {get { return _numBalls; }}

    [SerializeField] private int _duration;
    public int Duration {get { return _duration; }}

    [SerializeField] private Color _buttonColor;
    public Color ButtonColor
    {
        get { return _buttonColor; }
    }
}