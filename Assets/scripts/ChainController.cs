using UnityEngine;
using Photon.Pun;

public class ChainController : MonoBehaviour
{
    [Header("Настройки")]
    public float maxChainLength = 5f;
    public string targetTag = "Player2"; // Ищем игрока с этим тегом

    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;
    private GameObject targetObject;

    void Start()
    {
        // Настраиваем линию
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f; 
        // Можно назначить материал, если есть, или будет розовым (не страшно для теста)
    }

    void Update()
    {
        // 1. ПОИСК ПАРТНЕРА
        if (targetObject == null)
        {
            // Пытаемся найти объект с тегом "Player2"
            GameObject found = GameObject.FindGameObjectWithTag(targetTag);
            if (found != null)
            {
                targetObject = found;
                ConnectTo(targetObject); // Нашли! Соединяем.
            }
            return; // Если не нашли, выходим и ждем следующего кадра
        }

        // 2. ОТРИСОВКА ЦЕПИ
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetObject.transform.position);
    }

    void ConnectTo(GameObject otherPlayer)
    {
        // Физическое соединение создаем только если мы владелец этого персонажа
        if (GetComponent<PhotonView>().IsMine)
        {
            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = otherPlayer.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = maxChainLength;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false; 
        }
    }
}
