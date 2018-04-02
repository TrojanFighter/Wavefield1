using UnityEngine;

public class Main : MonoBehaviour {

    private void Awake()
    {
        Services.Events = new EventManager();
        Services.FoodManager = gameObject.AddComponent<FoodManager>();
        Services.Config = Resources.Load<Config>("Config");
    }

}
