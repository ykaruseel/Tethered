using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated && collision.CompareTag("Player"))
        {
            activated = true;
            GameManager.Instance.SetCheckpoint(transform.position);

            // можно добавить эффект активации
            Debug.Log("Checkpoint activated!");
        }
    }
}
