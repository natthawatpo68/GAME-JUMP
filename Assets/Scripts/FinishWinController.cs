using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

[RequireComponent(typeof(Collider))]
public sealed class FinishWinController : MonoBehaviour
{
    [SerializeField] private KeyExitManager exitManager;
    [SerializeField] private Transform startPoint;
    private float startedAt;
    private bool won;
    private GameObject panel;
    private Text result;

    private void Start()
    {
        startedAt = Time.realtimeSinceStartup;
        GetComponent<Collider>().isTrigger = true;
        CreateUi();
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (won && Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            PlayAgain();
#else
        if (won && Input.GetKeyDown(KeyCode.Return))
            PlayAgain();
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (won || other.GetComponent<PlayerRespawn>() == null) return;
        if (exitManager == null || exitManager.keysCollected < exitManager.keysRequired) return;
        won = true;
        result.text = "YOU WIN!\nTime: " + Format(Time.realtimeSinceStartup - startedAt);
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CreateUi()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("Event System", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<InputSystemUIInputModule>();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
        }
        var canvasObject = new GameObject("Win Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        panel = new GameObject("Win Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.sizeDelta = new Vector2(520, 310);
        panel.GetComponent<Image>().color = new Color(.025f, .055f, .12f, .96f);

        var title = new GameObject("Win Time", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(panel.transform, false);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, .42f); titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(30, 10); titleRect.offsetMax = new Vector2(-30, -20);
        result = title.GetComponent<Text>();
        result.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        result.fontSize = 42; result.alignment = TextAnchor.MiddleCenter; result.color = Color.white;

        var buttonObject = new GameObject("Play Again", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(.5f, .16f);
        buttonRect.sizeDelta = new Vector2(230, 68);
        buttonObject.GetComponent<Image>().color = new Color(.15f, .62f, .36f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(PlayAgain);

        var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(buttonObject.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        var labelText = label.GetComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 30; labelText.alignment = TextAnchor.MiddleCenter; labelText.color = Color.white;
        labelText.text = "PLAY AGAIN";
        panel.SetActive(false);
    }

    private void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }

    private static string Format(float seconds)
    {
        return Mathf.FloorToInt(seconds / 60f).ToString("00") + ":" +
            Mathf.FloorToInt(seconds % 60f).ToString("00") + "." +
            Mathf.FloorToInt((seconds * 100f) % 100f).ToString("00");
    }
}