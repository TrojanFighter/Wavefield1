using System.Collections;
using UnityEngine;

public class Critter : MonoBehaviour
{
    private FSM<Critter> _fsm;

	private void Start() {
	    // Initialize the FSM with the context (in this case the critter whose states we're managing)
        _fsm = new FSM<Critter>(this);

        // Set the initial state. Don't forget to do this!!
	    _fsm.TransitionTo<Searching>();
	}

	private void Update() {
	    // Update the 'brain'
		_fsm.Update();
	}

    // We need a way to find out when we hit food and to let the current state know about it
    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        var food = other.GetComponent<Food>();
        if (food != null)
        {
            ((CritterState)_fsm.CurrentState).OnCollidedWithFood();
            food.Disappear();
        }
    }

    // Utility methods to be used by the states
    private void MoveTowardsPosition(Vector3 position, float speed)
    {
        var directionToDestination = (position - transform.position).normalized;
        var impulseToDestination = directionToDestination * speed;
        GetComponent<Rigidbody>().AddForce(impulseToDestination, ForceMode.Impulse);
    }

    private void ChangeColor(Color color, float transitionDuration = 0.25f)
    {
        StartCoroutine(Coroutines.DoOverEasedTime(transitionDuration, Easing.SineEaseOut, t =>
        {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.white, color, t);
        }));
    }

    // State Animations
    private IEnumerator EatingAnimation()
    {
        yield return StartCoroutine(Coroutines.DoOverEasedTime(Services.Config.EatingDuration/2, Easing.BounceEaseOut, t =>
        {
            transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.5f, 1.5f, 1.5f), t);
        }));

        yield return StartCoroutine(Coroutines.DoOverEasedTime(Services.Config.EatingDuration/2, Easing.BounceEaseOut, t =>
        {
            transform.localScale = Vector3.Lerp(new Vector3(1.5f, 1.5f, 1.5f), Vector3.one, t);
        }));

        ((CritterState)_fsm.CurrentState).EatingAnimationEnded();
    }

    private IEnumerator SleepingAnimation()
    {
        yield return StartCoroutine(Coroutines.DoOverTime(Services.Config.SleepingDuration, t =>
        {
            var s = Mathf.Abs(Mathf.Sin((1-t) * 5));
            transform.localScale = new Vector3(1, 1, s);
        }));
        transform.localScale = Vector3.one;
        ((CritterState)_fsm.CurrentState).SleepingAnimationEnded();
    }


    ///////////////////////////////////////////////////////////////////////
    // STATES
    ///////////////////////////////////////////////////////////////////////

    // I'm defining all the states as private nested classes since no one
    // besides a Critter would need access to these, but you could make
    // more generic states that are applicable to a variety of objects

    // Since the states also need to be notified of a few different events by the context
    // like colliding with food, or the end of some animations, we'll declare a base class to handle it.
    private class CritterState : FSM<Critter>.State
    {
        public void OnCollidedWithFood()
        {
            // We want all states to transition to eating when they hit food so we can just implement
            // the function here
            Parent.TransitionTo<Eating>();
        }

        // The other events are specific to particular states so those states will override the methods
        // and all the others will use the empty default implementation
        public virtual void EatingAnimationEnded() {}
        public virtual void SleepingAnimationEnded() {}


        // A couple of different states need to know if there's food in range so
        // we'll add the method for finding out in the base state class
        protected GameObject GetFoodInRange()
        {
            var nearestFood = Services.FoodManager.GetNearestFood(Context.transform.position);
            // Early out if there's no food anywhere
            if (nearestFood == null) return null;

            // if there is food make sure it's within detection range
            var position = Context.transform.position;
            var distanceToNearest = Vector3.Distance(nearestFood.transform.position, position);
            if (distanceToNearest <= Services.Config.SearchDistance)
            {
                return nearestFood;
            }
            else
            {
                return null;
            }
        }

    }

    // SEARCHING
    // Pick a random destination and see if you find food along the way...
    private class Searching : CritterState
    {
        // Since only the searching state needs to know about the destination
        // we define it inside the state so as not to clutter up the Critter class
        private Vector3 _destination;

        public override void OnEnter()
        {
            Context.ChangeColor(Services.Config.SearchingColor);
            // pick a random destination within the bounds of the screen
            _destination = Util.RandomPositionInView();
            _destination.z = Context.transform.position.z;
        }

        public override void Update()
        {
            // check to see if there's any food in range...
            var food = GetFoodInRange();

            // if there is transition to Tracking and exit
            if (food != null)
            {
                TransitionTo<Tracking>();
                return;
            }

            var offsetToDestination = _destination - Context.transform.position;

            // if I've reached the destination start searching again
            if (offsetToDestination.magnitude <= Services.Config.DestinationThreshold)
            {
                TransitionTo<Searching>();
            }
            // otherwise move towards the destination
            else
            {
                Context.MoveTowardsPosition(_destination, Services.Config.SearchSpeed);
            }
        }

    }

    // TRACKING
    // If food is found hurry up towards it...
    private class Tracking : CritterState
    {
        public override void OnEnter()
        {
            Context.ChangeColor(Services.Config.TrackingColor);
        }

        public override void Update()
        {
            var food = GetFoodInRange();
            // If for some reason there's no longer food in range, go back to searching
            if (food == null)
            {
                TransitionTo<Searching>();
                return;
            }

            // Otherwise move towards the food...
            Context.MoveTowardsPosition(food.transform.position, Services.Config.TrackingSpeed);
        }
    }

    // EATING
    // Eat the food when you find some...
    private class Eating : CritterState
    {
        public override void OnEnter()
        {
            Context.ChangeColor(Services.Config.EatingColor);
            Context.StartCoroutine(Context.EatingAnimation());
        }

        public override void EatingAnimationEnded()
        {
            TransitionTo<Sleeping>();
        }
    }

    // SLEEPING
    // If you're full then take a nap...
    private class Sleeping : CritterState
    {
        public override void OnEnter()
        {
            Context.ChangeColor(Services.Config.SleepingColor);
            Context.StartCoroutine(Context.SleepingAnimation());
        }

        public override void SleepingAnimationEnded()
        {
            TransitionTo<Searching>();
        }
    }


}
