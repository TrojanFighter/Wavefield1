
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    private readonly List<GameObject> _food = new List<GameObject>();

	private void Start()
	{
	    Debug.Log("STARTING");
	    Services.Events.AddHandler<FoodDestroyedEvent>(OnFoodDestroyed);
	    StartCoroutine(GenerateFood());
	}

    private void OnDestroy()
    {
        Services.Events.RemoveHandler<FoodDestroyedEvent>(OnFoodDestroyed);
    }

    private void OnFoodDestroyed(FoodDestroyedEvent e)
    {
        _food.Remove(e.Food);
    }

    private IEnumerator GenerateFood()
    {
        while (gameObject.activeInHierarchy)
        {
            if (_food.Count < Services.Config.MaxFood)
            {
                var pos = Util.RandomPositionInView();
                pos.z = 0.5f;
                _food.Add(Instantiate(Services.Config.FoodPrefab, pos, Quaternion.Euler(0, 0, Random.value * 360)));
            }

            yield return new WaitForSeconds(Services.Config.FoodGenerateInterval);
        }
    }

    public GameObject GetNearestFood(Vector3 position)
    {
        if (_food.Count > 0)
        {
            var nearest = _food[0];
            var nearestDistance = Vector3.Distance(position, nearest.transform.position);
            for (var i = 1; i < _food.Count; i++)
            {
                var other = _food[i];
                var otherDistance = Vector3.Distance(position, other.transform.position);
                if (otherDistance < nearestDistance)
                {
                    nearest = other;
                    nearestDistance = otherDistance;
                }
            }
            return nearest;
        }
        else
        {
            return null;
        }
    }
}