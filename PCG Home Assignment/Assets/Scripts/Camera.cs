using UnityEngine;

public class Camera : MonoBehaviour
{
    private Transform playerTransform;

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        Vector3 newPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
        transform.position = Vector3.Lerp(transform.position, newPos, 0.1f);
    }
}
