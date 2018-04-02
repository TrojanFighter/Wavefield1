using UnityEngine;

public class BPlayer : MonoBehaviour {

    private void Start () {
		GetComponent<Renderer>().material.color = new Color(1.0f, 0.7f, 0);
	}

    private void Update ()
    {
        var z = transform.position.z;
        var mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(mousePos.x, mousePos.y, z);
    }
}
