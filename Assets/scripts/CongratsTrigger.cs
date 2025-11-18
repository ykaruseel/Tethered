// Assets/Scripts/UI/CongratsTrigger.cs
using System.Collections;
using UnityEngine;
#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
using TMPro;
#endif
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class CongratsTrigger : MonoBehaviour
{
    [Header("Who can trigger")]
    public LayerMask triggerLayers;          // укажи слой(и) игроков

    [Header("Message UI")]
    [Tooltip(" орневой объект надписи (панель/группа).")]
    public GameObject messageRoot;
    [Tooltip("ќпционально: CanvasGroup дл€ фейда (если нет Ч включим/выключим объект).")]
    public CanvasGroup messageGroup;

    [Tooltip("ѕоле текста (TextMeshProUGUI или Text). ≈сли пусто Ч попробуем найти на messageRoot.")]
    public Graphic textGraphic; // можно перетащить TextMeshProUGUI или Text (оба наследуют Graphic)

    [TextArea]
    public string message = "Congratulations!";

    [Header("Behavior")]
    public bool onceOnly = true;
    public bool hideOnExit = false;
    [Min(0f)] public float fadeDuration = 0.35f;

    bool _fired;
    Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;

        if (messageRoot == null && messageGroup != null)
            messageRoot = messageGroup.gameObject;

        if (messageGroup == null && messageRoot != null)
            messageGroup = messageRoot.GetComponent<CanvasGroup>();

        // начальное скрытие
        if (messageGroup != null)
        {
            if (messageRoot != null) messageRoot.SetActive(true); // CanvasGroup управл€ет альфой
            messageGroup.alpha = 0f;
            messageGroup.interactable = false;
            messageGroup.blocksRaycasts = false;
        }
        else if (messageRoot != null)
        {
            messageRoot.SetActive(false);
        }

        EnsureTextAssigned();
        ApplyText(message);
    }

    void OnValidate()
    {
        if (_col == null) _col = GetComponent<Collider2D>();
        if (_col != null) _col.isTrigger = true;
        if (messageRoot == null && messageGroup != null) messageRoot = messageGroup.gameObject;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!MatchesLayer(other.gameObject)) return;
        if (onceOnly && _fired) return;

        _fired = true;
        ShowMessage(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!hideOnExit) return;
        if (!MatchesLayer(other.gameObject)) return;
        if (onceOnly) return; // если одноразово Ч не пр€чем

        ShowMessage(false);
    }

    void ShowMessage(bool show)
    {
        StopAllCoroutines();

        if (messageGroup != null)
        {
            if (messageRoot != null) messageRoot.SetActive(true);
            StartCoroutine(FadeCanvasGroup(messageGroup, show ? 1f : 0f, fadeDuration));
        }
        else if (messageRoot != null)
        {
            messageRoot.SetActive(show);
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }
        cg.alpha = target;
        bool visible = target > 0.99f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
        if (!visible && messageRoot != null) messageRoot.SetActive(false);
    }

    bool MatchesLayer(GameObject go) => (triggerLayers.value & (1 << go.layer)) != 0;

    void EnsureTextAssigned()
    {
        if (textGraphic != null) return;
        if (messageRoot == null) return;

#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
        var tmp = messageRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null) { textGraphic = tmp; return; }
#endif
        var uiText = messageRoot.GetComponentInChildren<Text>(true);
        if (uiText != null) textGraphic = uiText;
    }

    void ApplyText(string s)
    {
        if (textGraphic == null) return;

#if TMP_PRESENT || TEXTMESHPRO || UNITY_TEXTMESHPRO
        if (textGraphic is TextMeshProUGUI tmp) { tmp.text = s; return; }
#endif
        if (textGraphic is Text uText) { uText.text = s; }
    }
}
