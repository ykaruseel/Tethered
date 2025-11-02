using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [Header("Ссылка на игроков")]
    public Transform player1;
    public Transform player2;

    [Header("Начальная точка спавна")]
    public Transform startPoint;

    private Vector2 currentCheckpoint;

    void Awake()
    {
        // Singleton, чтобы обращаться из других скриптов
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentCheckpoint = startPoint.position;
    }

    // Обновляем текущий чекпоинт
    public void SetCheckpoint(Vector2 newPoint)
    {
        currentCheckpoint = newPoint;
    }

    // Респавн обоих игроков
    public void RespawnPlayers()
    {
        if (player1 != null)
        {
            var rb1 = player1.GetComponent<Rigidbody2D>();
            rb1.linearVelocity = Vector2.zero;
            player1.position = currentCheckpoint;
        }

        if (player2 != null)
        {
            var rb2 = player2.GetComponent<Rigidbody2D>();
            rb2.linearVelocity = Vector2.zero;
            player2.position = currentCheckpoint + new Vector2(1.5f, 0f); // чуть правее, чтобы не стояли в одной точке
        }
    }
}
