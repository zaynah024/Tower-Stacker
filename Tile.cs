using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject lostTilePrefab;
    [SerializeField] private float maxDistance = 5f;
    private float currentDistance;
    private float stepSpeed = 6f;
    private bool movingForward;
    public bool moveAlongX;

    private void Start()
    {
        currentDistance = maxDistance;
        Vector3 initialMove = moveAlongX ? new Vector3(currentDistance, 0f, 0f) : new Vector3(0f, 0f, currentDistance);
        transform.Translate(initialMove);
    }

    private void Update()
    {
        float step = stepSpeed * Time.deltaTime;
        if (moveAlongX)
            MoveAxis(Vector3.right, step);
        else
            MoveAxis(Vector3.forward, step);
    }

    private void MoveAxis(Vector3 direction, float step)
    {
        if (movingForward)
        {
            if (currentDistance < maxDistance)
            {
                transform.Translate(direction * step);
                currentDistance += step;
            }
            else
            {
                movingForward = false;
            }
        }
        else
        {
            if (currentDistance > -maxDistance)
            {
                transform.Translate(-direction * step);
                currentDistance -= step;
            }
            else
            {
                movingForward = true;
            }
        }
    }

    public void ScaleTile()
    {
        if (Mathf.Abs(currentDistance) > 0.2f)
        {
            float lostLength = Mathf.Abs(currentDistance);
            Vector3 scaleLoss = moveAlongX ? new Vector3(lostLength, 0, 0) : new Vector3(0, 0, lostLength);

            if ((moveAlongX && transform.localScale.x < lostLength) ||
                (!moveAlongX && transform.localScale.z < lostLength))
            {
                gameObject.AddComponent<Rigidbody>();
                Spawner.instance.GameOver();
                return;
            }

            CreateLostTile(scaleLoss, lostLength);

            transform.localScale -= scaleLoss;
            transform.Translate((currentDistance > 0 ? -0.5f : 0.5f) * scaleLoss);
        }
        else
        {
            transform.Translate(moveAlongX ? new Vector3(currentDistance, 0, 0) : new Vector3(0, 0, currentDistance));
        }

        Destroy(this);
    }

    private void CreateLostTile(Vector3 scaleLoss, float lostLength)
    {
        GameObject lostTile = Instantiate(lostTilePrefab);
        lostTile.transform.localScale = moveAlongX ?
            new Vector3(lostLength, transform.localScale.y, transform.localScale.z) :
            new Vector3(transform.localScale.x, transform.localScale.y, lostLength);

        Vector3 lostTilePosition = transform.position;
        if (moveAlongX)
            lostTilePosition.x += (currentDistance > 0 ? 1 : -1) * (transform.localScale.x - lostLength) / 2;
        else
            lostTilePosition.z += (currentDistance > 0 ? 1 : -1) * (transform.localScale.z - lostLength) / 2;

        lostTile.transform.position = lostTilePosition;
        lostTile.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
    }
}
