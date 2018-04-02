using UnityEngine;

public static class Util
{
    public static Vector3 RandomPositionInView()
    {
        var x = Random.value;
        var y = Random.value;
        return Camera.main.ViewportToWorldPoint(new Vector3(x, y));
    }
}