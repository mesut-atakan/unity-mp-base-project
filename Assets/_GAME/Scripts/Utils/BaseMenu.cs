using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game.Utils
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseMenu : MonoBehaviour
    {
        private static Vector2 REFERANCE_RESULATION = new Vector2(1920.0f, 1080.0f);
        private const float MATCH_WIDTH_OR_HEIGHT = 0.5f;
        private const float REFERANCE_PIXELS_PER_UNIT = 100.0f;
        private const float OPEN_ALPHA = 1.0f;
        private const float CLOSE_ALPHA = 0.0f;

        [SerializeField] private bool openOnAwake = false;
        [SerializeField] private bool canInput = false;

        private CanvasGroup _canvasGroup;
        private CanvasScaler _canvasScaler;

        public bool IsOpen => _canvasGroup.alpha > 0.1f;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasScaler = GetComponent<CanvasScaler>();

            ApplyAwakeSettings();
        }

        private void OnEnable()
        {
            SetCanvasScalerDefaultSettings();
        }

        private void Reset()
        {
            SetCanvasScalerDefaultSettings();
        }

        [ContextMenu(nameof(Open))]
        public virtual void Open()
        {
#if UNITY_EDITOR
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
#endif

            if (IsOpen)
                return;

            _canvasGroup.alpha = OPEN_ALPHA;
            _canvasGroup.interactable = canInput;
            _canvasGroup.blocksRaycasts = canInput;
        }

        [ContextMenu(nameof(Close))]
        public virtual void Close()
        {
#if UNITY_EDITOR
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
#endif

            if (!IsOpen)
                return;

            _canvasGroup.alpha = CLOSE_ALPHA;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void SetCanvasScalerDefaultSettings()
        {
#if UNITY_EDITOR
            _canvasScaler = GetComponent<CanvasScaler>();
#endif
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = REFERANCE_RESULATION;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;
            _canvasScaler.referencePixelsPerUnit = REFERANCE_PIXELS_PER_UNIT;
        }

        protected virtual void ApplyAwakeSettings()
        {
            if (openOnAwake)
                Open();
            else
                Close();
        }
    }
}