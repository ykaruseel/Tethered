using UnityEngine;

public class ChainController : MonoBehaviour
{
    [Header("Настройки цепи")]
    public Rigidbody2D connectedPlayer; // тело другого игрока
    public float maxChainLength = 5f;

    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;

    void Start()
    {
        // Создаем сустав
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.autoConfigureDistance = false;
        joint.distance = maxChainLength;
        joint.maxDistanceOnly = true;
        joint.connectedBody = connectedPlayer;
        joint.enableCollision = false;

        // Визуализация
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.gray;
        lineRenderer.endColor = Color.gray;
    }

    void Update()
    {
        if (lineRenderer == null || connectedPlayer == null) return;

        // Рисуем линию между игроками
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, connectedPlayer.transform.position);
    }
}
