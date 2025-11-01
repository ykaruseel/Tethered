// Подключаем библиотеку Unity
using UnityEngine;

// Объявляем класс ChainController
public class ChainController : MonoBehaviour
{
    [Header("Объекты для связи")]
    [Tooltip("Перетащите сюда Rigidbody2D второго игрока")]
    public Rigidbody2D connectedPlayer; // Ссылка на физическое тело второго игрока

    [Header("Настройки цепи")]
    [Tooltip("Максимальная длина цепи")]
    public float maxChainLength = 5.0f; // Максимальное расстояние между игроками

    // Линия для визуализации цепи (необязательно, но полезно)
    private LineRenderer lineRenderer;

    void Start()
    {
        // --- Создание физической связи ---
        
        // Получаем или добавляем компонент DistanceJoint2D к текущему объекту (Player1)
        DistanceJoint2D joint = gameObject.AddComponent<DistanceJoint2D>();
        
        // Выключаем автоматическую настройку дистанции
        joint.autoConfigureDistance = false;
        
        // Устанавливаем максимальную дистанцию из нашей переменной
        joint.distance = maxChainLength;
        
        // Важный параметр! Позволяет игрокам сближаться, но не расходиться дальше maxChainLength
        joint.maxDistanceOnly = true;
        
        // "Привязываем" второй конец сустава к другому игроку
        joint.connectedBody = connectedPlayer;

        // --- Создание видимой цепи (визуализация) ---
        
        // Добавляем компонент LineRenderer для отрисовки линии
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Настраиваем внешний вид линии
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        
        // Указываем, что у линии будет две точки (начало и конец)
        lineRenderer.positionCount = 2;
        
        // Можно добавить материал, чтобы цепь выглядела лучше (например, простой белый)
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void Update()
    {
        // В каждом кадре обновляем позиции точек нашей видимой цепи
        // Первая точка - позиция первого игрока
        lineRenderer.SetPosition(0, transform.position);
        // Вторая точка - позиция второго игрока
        lineRenderer.SetPosition(1, connectedPlayer.transform.position);
    }
}
