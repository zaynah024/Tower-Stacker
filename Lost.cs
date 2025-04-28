using UnityEngine;

public class Lost : MonoBehaviour
{
    private float lifetime = 5f;
    private float fallThreshold = -5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Destroy(gameObject);
        }
    }
}
