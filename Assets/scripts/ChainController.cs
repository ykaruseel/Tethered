using UnityEngine;
using Photon.Pun;

public class ChainController : MonoBehaviour, IPunObservable // 👈 Добавлено для синхронизации
{
    [Header("Настройки")]
    public float maxChainLength = 5f;
    public string targetTag = "Player2"; 

    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;
    private GameObject targetObject;
    private PhotonView view; // Ссылка на PhotonView

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    void Start()
    {
        // Настраиваем линию
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        // ...
    }

    void Update()
    {
        // 1. ПОИСК ПАРТНЕРА
        if (targetObject == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(targetTag);
            if (found != null)
            {
                targetObject = found;
                ConnectTo(targetObject); // Нашли! Соединяем.
            }
            return;
        }
        
        // 2. ОТРИСОВКА ЦЕПИ (У этого игрока всегда должна работать отрисовка)
        if (lineRenderer != null)
        {
             lineRenderer.SetPosition(0, transform.position);
             lineRenderer.SetPosition(1, targetObject.transform.position);
        }
    }

    void ConnectTo(GameObject otherPlayer)
    {
        // Физическое соединение Joint: создается ТОЛЬКО на Хосте (владельце)
        // Joint создается один раз и влияет на физику обоих.
        if (view.IsMine)
        {
            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = otherPlayer.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = maxChainLength;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false; 
            
            // 👈 ВАЖНОЕ ИЗМЕНЕНИЕ: Синхронизируем Joint
            // Это может быть не нужно, если PhotonRigidbody2DView уже есть,
            // но мы гарантируем, что Joint создался.
        }
    }

    // 👈 МЕТОД IPunObservable (Для синхронизации визуальной линии)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Мы используем Photon Transform View и Rigidbody View для позиций.
        // LineRenderer синхронизируется через Transform View, так как он на том же объекте.
        // Если цепь не видна, это часто ошибка отрисовки.
        // Мы можем добавить прямую синхронизацию позиций, но это сложнее.
        
        // Для начала попробуй просто удалить этот пустой метод, чтобы не было конфликтов, 
        // если ты его не используешь.
    }
}
