
using UnityEngine;
using WaveField.Event;

namespace GPP.FSM
{

    public class FoodDestroyedEvent : GameEvent
    {
        public GameObject Food { get; private set; }

        public FoodDestroyedEvent(GameObject food)
        {
            Food = food;
        }
    }
}