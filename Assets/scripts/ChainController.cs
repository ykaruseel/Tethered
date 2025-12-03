// Assets/Scripts/ChainController.cs
using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public class ChainController : MonoBehaviour
{
    [Header("Настройки цепи")]
    [Min(0.1f)] public float maxChainLength = 5f;
    [Tooltip("Tag второго игрока, к которому цепляемся.")]
    public string targetTag = "Player2";

    [Header("Визуализация")]
    public Material chainMaterial;                  // можно назначить в инспекторе
    [Min(0.001f)] public float lineWidth = 0.08f;
    public Color lineColor = Color.white;
    [Tooltip("Сортинг-слой линии (часто тот же, что и у игроков).")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 5;

    private DistanceJoint2D joint;
    private LineRenderer lineRenderer;
    private GameObject targetObject;
    private PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // --- LineRenderer ---
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // Материал
        if (chainMaterial != null)
        {
            lineRenderer.material = chainMaterial;
        }
        else
        {
            // создаём безопасный материал на известном шейдере
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("ChainController: Shader 'Sprites/Default' не найден, линия может быть невидима.");
                lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            }
            else
            {
                lineRenderer.material = new Material(shader);
            }

            lineRenderer.material.color = lineColor;
        }

        // Сортинг — чтобы линия не пряталась за фоном
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;
    }

    private void Update()
    {
        // 1. Ищем партнёра, если ещё не нашли
        if (targetObject == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(targetTag);
            if (found != null)
            {
                targetObject = found;
                ConnectTo(targetObject);
            }
            // Пока не нашли — ничего не рисуем
            lineRenderer.enabled = false;
            return;
        }

        // 2. Рисуем линию между игроками
        if (lineRenderer != null && targetObject != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetObject.transform.position);
        }
    }

    private void ConnectTo(GameObject otherPlayer)
    {
        // Сустав создаём только у владельца этого объекта
        if (!view.IsMine) return;

        if (joint == null)
            joint = gameObject.AddComponent<DistanceJoint2D>();

        joint.connectedBody = otherPlayer.GetComponent<Rigidbody2D>();
        joint.autoConfigureDistance = false;
        joint.distance = maxChainLength;
        joint.maxDistanceOnly = true;
        joint.enableCollision = false;
    }
}
