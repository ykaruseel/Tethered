// Assets/Scripts/Platforms/UpDownPlatform2D.cs
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class UpDownPlatform2D : MonoBehaviour
{
    [Header("Motion")]
    [Min(0.01f)] public float distance = 3f;
    [Min(0.01f)] public float speed = 2f;
    [Min(0f)] public float waitAtEnds = 0.2f;
    public bool startAtTop = false;
    public bool autoStart = true;

    [Header("Passengers")]
    public LayerMask passengerLayers = ~0;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private float _startY, _bottomY, _topY;
    private bool _goingUp, _moving;
    private float _waitTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _col.isTrigger = false;

        _startY = transform.position.y;
        _bottomY = _startY;
        _topY = _startY + Mathf.Abs(distance);

        if (startAtTop) { SetY(_topY); _goingUp = false; }
        else { SetY(_bottomY); _goingUp = true; }

        _moving = autoStart;
    }

    void FixedUpdate()
    {
        if (!_moving) return;

        if (_waitTimer > 0f) { _waitTimer -= Time.fixedDeltaTime; return; }

        float targetY = _goingUp ? _topY : _bottomY;
        float step = speed * Time.fixedDeltaTime;
        float nextY = Mathf.MoveTowards(transform.position.y, targetY, step);

        _rb.MovePosition(new Vector2(_rb.position.x, nextY));

        if (Mathf.Approximately(nextY, targetY))
        {
            _goingUp = !_goingUp;
            if (waitAtEnds > 0f) _waitTimer = waitAtEnds;
        }
    }

    public void StartMoving() => _moving = true;
    public void StopMoving() => _moving = false;

    public void ResetToStart(bool toTop = false)
    {
        _moving = false;
        if (toTop) { SetY(_topY); _goingUp = false; }
        else { SetY(_bottomY); _goingUp = true; }
        _waitTimer = 0f;
    }

    void SetY(float y)
    {
        var p = transform.position; p.y = y; transform.position = p;
        _rb.position = new Vector2(_rb.position.x, y);
    }

    // === Passenger handling ===
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsPassenger(other.collider)) return;

        // Нормаль контакта у платформы вверх (> 0.5), если другой коллайдер сверху
        foreach (var c in other.contacts)
        {
            if (c.normal.y > 0.5f)
            {
                other.transform.SetParent(transform, true);
                break;
            }
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!IsPassenger(other.collider)) return;

        bool onTop = false;
        foreach (var c in other.contacts)
        {
            if (c.normal.y > 0.5f) { onTop = true; break; }
        }

        // Если перестали стоять сверху — отстыковать
        if (!onTop && other.transform.parent == transform)
            other.transform.SetParent(null, true);
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (!IsPassenger(other.collider)) return;
        if (other.transform.parent == transform)
            other.transform.SetParent(null, true);
    }

    bool IsPassenger(Collider2D col)
    {
        return ((1 << col.gameObject.layer) & passengerLayers) != 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var pos = Application.isPlaying ? (Vector3)_rb?.position : transform.position;
        float startY = Application.isPlaying ? _startY : pos.y;
        float topY   = startY + Mathf.Abs(distance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(pos.x, startY, 0), new Vector3(pos.x, topY, 0));
        Gizmos.DrawSphere(new Vector3(pos.x, startY, 0), 0.05f);
        Gizmos.DrawSphere(new Vector3(pos.x, topY, 0), 0.05f);
    }
#endif
}
