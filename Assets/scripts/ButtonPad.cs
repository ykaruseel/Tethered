// Assets/Scripts/CoopGates/ButtonPad.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class ButtonPad : MonoBehaviour
{
    [Header("Требуемый цвет")]
    public PlayerColor requiredColor = PlayerColor.White;

    [Header("Визуал (опционально)")]
    public Transform topVisual;
    public float pressedOffsetY = -0.06f;
    public float pressLerpSpeed = 12f;

    public event Action<ButtonPad, bool> OnPressChanged;
    public bool Pressed { get; private set; }

    // why: считаем по игрокам, а не по коллайдерам
    private readonly HashSet<GameObject> _pressingPlayers = new();

    private Vector3 _topStartPos;
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
        if (topVisual != null) _topStartPos = topVisual.localPosition;
    }

    void Update()
    {
        if (topVisual == null) return;
        var target = _topStartPos + Vector3.up * (Pressed ? pressedOffsetY : 0f);
        topVisual.localPosition = Vector3.Lerp(topVisual.localPosition, target, Time.deltaTime * pressLerpSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var playerGO = GetValidPlayerRoot(other);
        if (playerGO == null) return;
        if (_pressingPlayers.Add(playerGO))
            SetPressed(_pressingPlayers.Count > 0);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var playerGO = GetValidPlayerRoot(other);
        if (playerGO == null) return;
        if (_pressingPlayers.Remove(playerGO))
            SetPressed(_pressingPlayers.Count > 0);
    }

    private GameObject GetValidPlayerRoot(Collider2D col)
    {
        // why: поддержка коллайдеров на дочерних объектах персонажа
        var id = col.GetComponentInParent<PlayerIdentity>();
        if (id == null || id.color != requiredColor) return null;
        return id.gameObject;
    }

    private void SetPressed(bool value)
    {
        if (Pressed == value) return;
        Pressed = value;
        OnPressChanged?.Invoke(this, Pressed);
    }
}
