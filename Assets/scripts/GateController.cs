// Assets/Scripts/CoopGates/GateController.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GateController : MonoBehaviour
{
    [Header("Позиции (мировой Y)")]
    public float closedY = 0f;
    public float openY = 3f;

    [Header("Движение")]
    [Min(0.01f)] public float moveSpeed = 3f;

    public bool IsOpenLatched => _latchedOpen;

    bool _latchedOpen;
    float _targetY;

    void Awake()
    {
        var p = transform.position;
        p.y = closedY; // старт закрыт
        transform.position = p;
        _targetY = closedY;
    }

    void Update()
    {
        var pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, _targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    /// <summary>Открыть без фиксации (пока логика не решит иначе).</summary>
    public void Open()
    {
        if (_latchedOpen) return;
        _targetY = openY;
    }

    /// <summary>Открыть и зафиксировать навсегда.</summary>
    public void OpenPermanently()
    {
        _latchedOpen = true;
        _targetY = openY;
    }

    /// <summary>Закрыть (если не зафиксированы).</summary>
    public void Close()
    {
        if (_latchedOpen) return;
        _targetY = closedY;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(transform.position.x - 0.3f, openY, 0), new Vector3(transform.position.x + 0.3f, openY, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(transform.position.x - 0.3f, closedY, 0), new Vector3(transform.position.x + 0.3f, closedY, 0));
    }
#endif
}
