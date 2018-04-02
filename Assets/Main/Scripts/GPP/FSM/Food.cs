using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour {

    private void Start()
    {
        GetComponent<Renderer>().material.color = Color.red;

        var endScale = transform.localScale;
        var startScale = new Vector3(0.1f, 0.1f, 0.1f);
        transform.localScale = startScale;
        StartCoroutine(Coroutines.DoOverEasedTime(1, Easing.QuadEaseOut, t =>
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
        }));
    }

    private void OnDestroy()
    {
        Services.Events.Fire(new FoodDestroyedEvent(gameObject));
    }

    public void Disappear()
    {
        Destroy(GetComponent<Collider>());
        StartCoroutine(DisappearAnimation());
    }

    public IEnumerator DisappearAnimation()
    {
        var startScale = transform.localScale;
        var endScale = new Vector3(0.1f, 0.1f, 0.1f);
        yield return StartCoroutine(Coroutines.DoOverEasedTime(1, Easing.QuadEaseIn, t =>
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
        }));

        Destroy(gameObject);
    }

}
