using System;
using UnityEngine;

using WaveField.ResourceManagement;

namespace WaveField.Services.Task
{
////////////////////////////////////////////////////////////////////////
// GENERAL PURPOSE TASKS
////////////////////////////////////////////////////////////////////////

// Simple action task
    public class ActionTask : Task
    {

        public Action Action { get; private set; }

        public ActionTask(Action action)
        {
            Action = action;
        }

        protected override void Init()
        {
            Action();
            SetStatus(TaskStatus.Success);
        }

    }


// A base class for tasks that track time. Use it to make things like
// Wait, ScaleUpOverTime, etc. tasks
    public abstract class TimedTask : Task
    {
        public float Duration { get; private set; }
        public float StartTime { get; private set; }

        protected TimedTask(float duration)
        {
            Debug.Assert(duration > 0, "Cannot create a timed task with duration less than 0");
            Duration = duration;
        }

        protected override void Init()
        {
            StartTime = Time.time;
        }

        internal override void Update()
        {
            var now = Time.time;
            var elapsed = now - StartTime;
            var t = Mathf.Clamp01(elapsed / Duration);
            if (t >= 1)
            {
                OnElapsed();
            }
            else
            {
                OnTick(t);
            }
        }

        // t is the normalized time for the task. E.g. if half the task's duration has elapsed then t == 0.5
        // This is where subclasses will do most of their work
        protected virtual void OnTick(float t)
        {
        }

        // Default to being successful if we get to the end of the duration
        protected virtual void OnElapsed()
        {
            SetStatus(TaskStatus.Success);
        }

    }
    
    
    public abstract class CheckTask : Task
    {
        public float Duration { get; private set; }
        public float StartTime { get; private set; }

        protected CheckTask(float duration)
        {
            Debug.Assert(duration > 0, "Cannot create a timed task with duration less than 0");
            Duration = duration;
        }

        protected override void Init()
        {
            StartTime = Time.time;
        }

        internal override void Update()
        {
            var now = Time.time;
            var elapsed = now - StartTime;
            var t = Mathf.Clamp01(elapsed / Duration);
            if (t >= 1)
            {
                OnElapsed();
            }
            else
            {
                OnTick(t);
            }
        }

        // t is the normalized time for the task. E.g. if half the task's duration has elapsed then t == 0.5
        // This is where subclasses will do most of their work
        protected virtual void OnTick(float t)
        {
        }

        // Default to being successful if we get to the end of the duration
        protected virtual void OnElapsed()
        {
            SetStatus(TaskStatus.Success);
        }

    }


// A VERY simple wait task
    public class Wait : TimedTask
    {
        public Wait(float duration) : base(duration)
        {
        }
    }


////////////////////////////////////////////////////////////////////////
// GAME OBJECT TASKS
////////////////////////////////////////////////////////////////////////

// Base classes for tasks that operate on a game object.
// Since C# doesn't allow multiple inheritance we'll make two versions - one timed and one untimed
    public abstract class GOTask : Task
    {
        protected readonly GameObject gameObject;

        protected GOTask(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }


    public abstract class TimedGOTask : TimedTask
    {
        protected readonly GameObject gameObject;

        protected TimedGOTask(GameObject gameObject, float duration) : base(duration)
        {
            this.gameObject = gameObject;
        }
    }


// A task to teleport a gameobject
    public class SetPosTask : GOTask
    {
        private readonly Vector3 _pos;

        public SetPosTask(GameObject gameObject, Vector3 pos) : base(gameObject)
        {
            _pos = pos;
        }

        protected override void Init()
        {
            gameObject.transform.position = _pos;
            SetStatus(TaskStatus.Success);
        }
    }


// A task to lerp a gameobject's position
    public class MoveTask : TimedGOTask
    {
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }

        public MoveTask(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
        {
            Start = start;
            End = end;
        }

        protected override void OnTick(float t)
        {
            gameObject.transform.position = Vector3.Lerp(Start, End, t);
        }
    }


// A task to lerp a gameobject's scale
    public class ScaleTask : TimedGOTask
    {
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }

        public ScaleTask(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
        {
            Start = start;
            End = end;
        }

        protected override void OnTick(float t)
        {
            gameObject.transform.localScale = Vector3.Lerp(Start, End, t);
        }
    }
    
    public class RotateTask : TimedGOTask
    {
        private readonly Vector3 _startRotation, _endRotation;

        public RotateTask(GameObject gameObject, Vector3 startRotation, Vector3 endRotation, float duration) : base(
            gameObject, duration)
        {
            _startRotation = startRotation;
            _endRotation = endRotation;
        }

        protected override void OnTick(float t)
        {
            gameObject.transform.rotation = Quaternion.Euler(Vector3.Lerp(_startRotation, _endRotation, t));
        }
    }
    
    /// <summary>
    /// Boss Action Sector
    /// </summary>
    
    public class SpawnTask : Task
    {
        //public Vector3 Start { get; private set; }
        //public Vector3 End { get; private set; }

        public readonly GameObject prefabToSpawn;
        public Vector3 SpawnPosition { get; private set; }
        //sprivate GameObject SpawnedGO;
        private Quaternion SpawnRotation;

        public SpawnTask(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation, out GameObject SpawnedGO) 
        {
            SpawnPosition = spawnPosition;
            prefabToSpawn = prefab;
            SpawnRotation = spawnRotation;
            //SpawnedGO=UnityEngine.GameObject.Instantiate(prefabToSpawn,)
            SpawnedGO = UnityEngine.GameObject.Instantiate(prefabToSpawn, SpawnPosition, SpawnRotation);
        }

        protected override void Init()
        {
            
            SetStatus(TaskStatus.Success);
        }
    }
    
    public class RandomFireTask : TimedGOTask
    {
        //public Vector3 Start { get; private set; }
        //public Vector3 End { get; private set; }

        public readonly GameObject bulletToFire;
        public GameObject fireSourceObject;
        
        //public Vector3 FirePosition { get; private set; }
        private int LongBurstLength;
        private float fireRate;
        private float lastFireTick=0f;
        
        public Pool BulletPool;

        public RandomFireTask(GameObject fireSource, int longBurstLength,float duration) :base(fireSource, duration)
        {
            //FirePosition = firePosition;
            fireSourceObject = fireSource;
            LongBurstLength = longBurstLength;
            fireRate = 1f / longBurstLength;
            //SpawnedGO=UnityEngine.GameObject.Instantiate(prefabToSpawn,)
        }

        protected override void Init()
        {
            Pool poolObject = GameObject.FindObjectOfType((typeof(Pool))) as Pool;
            BulletPool = poolObject;
            Fire();
            
            //SetStatus(TaskStatus.Success);
        }
        protected override void OnTick(float t)
        {
            
           //Not working not coming in dont know why
           /* if (t - lastFireTick >= fireRate)
            {
                Fire();
                lastFireTick = t;
            }*/
        }

        void Fire()
        {
                var bullet = BulletPool.nextThing; 
                bullet.transform.position = fireSourceObject.transform.position;
                bullet.transform.rotation =Quaternion.Euler(0,UnityEngine.Random.Range(0,360),0);  //fireSourceObject.transform.rotation;
                //yield return new WaitForSeconds(fireRate);
        }
    }
}

