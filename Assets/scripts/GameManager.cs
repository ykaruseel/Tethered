using UnityEngine;
using Photon.Pun; // Обязательно добавляем библиотеку

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Точка старта")]
    public Transform startPoint; // Перетащи сюда объект StartPoint со сцены

    private Vector2 currentCheckpoint;

    void Awake()
    {
        // Синглтон (оставляем как было)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Запоминаем старт
        if (startPoint != null)
            currentCheckpoint = startPoint.position;
    }

    void Start()
    {
        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: СПАВН ИГРОКОВ ---
        
        // Photon сам проверяет: ты создал комнату или вошел в нее?
        
        if (PhotonNetwork.IsMasterClient)
        {
            // Если ты Хост (Игрок 1) -> Создаем Белого игрока
            // "PlayerWhite" - это имя префаба в папке Resources
            PhotonNetwork.Instantiate("PlayerWhite", currentCheckpoint, Quaternion.identity);
        }
        else
        {
            // Если ты Гость (Игрок 2) -> Создаем Черного игрока
            // Спавним чуть правее (+1.5f), чтобы не застряли друг в друге
            Vector2 spawnPos = currentCheckpoint + new Vector2(1.5f, 0f);
            PhotonNetwork.Instantiate("PlayerBlack", spawnPos, Quaternion.identity);
        }
    }

    // Твоя функция для сохранения чекпоинта (оставляем)
    public void SetCheckpoint(Vector2 newPoint)
    {
        currentCheckpoint = newPoint;
    }

    // Простая функция респавна (для своей смерти)
    public void RespawnMyPlayer(GameObject playerObject)
    {
        playerObject.transform.position = currentCheckpoint;
        // Сброс скорости
        if (playerObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
