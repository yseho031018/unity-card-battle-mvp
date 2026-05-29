using System;
using System.Collections;
using System.Collections.Generic;
using CardBattle.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("프레임")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image typeBannerImage;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Outline outline;
        [SerializeField] private Canvas sortingCanvas;

        [Header("텍스트")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text costOrLevelText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text artworkPlaceholderText;
        [SerializeField] private TMP_Text descriptionText;

        private static readonly Color MonsterColor = new(0.86f, 0.51f, 0.28f);
        private static readonly Color SpellColor = new(0.25f, 0.61f, 0.56f);
        private static readonly Color TrapColor = new(0.64f, 0.29f, 0.58f);
        private static readonly Color NormalOutlineColor = new(0.06f, 0.06f, 0.07f, 0.95f);
        private static readonly Color SelectedOutlineColor = new(1f, 0.84f, 0.28f, 1f);
        private const int MaxDescriptionCharacters = 48;
        private const float HoverOffsetY = 28f;
        private const float HoverScale = 1.06f;
        private const float HoverAnimationDuration = 0.08f;

        private RectTransform rectTransform;
        private LayoutElement layoutElement;
        private CanvasGroup canvasGroup;
        private Vector3 restingLocalPosition;
        private float baseScale = 1f;
        private Vector2 baseSize;
        private Transform dragOriginalParent;
        private int dragOriginalSiblingIndex;
        private Canvas rootCanvas;
        private RectTransform dragPlane;
        private Camera dragEventCamera;
        private Vector3 dragPointerOffset;
        private GameObject dragPlaceholder;
        private Coroutine hoverAnimation;
        private readonly List<RaycastResult> dragRaycastResults = new();
        private bool isSelected;
        private bool isSelectable = true;
        private bool showFullDescription;
        private bool hasRestingTransform;
        private bool isDragging;
        private bool dropConsumed;

        public event Action<CardView> Clicked;
        public event Action<CardView> DragStarted;
        public event Action<CardView> PointerEntered;
        public event Action<CardView> PointerExited;
        public CardData CardData { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            layoutElement = GetComponent<LayoutElement>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (rectTransform != null)
            {
                baseSize = rectTransform.sizeDelta;
            }

            if (sortingCanvas == null)
            {
                sortingCanvas = GetComponent<Canvas>();
            }

            if (sortingCanvas != null && GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            rootCanvas = sortingCanvas != null ? sortingCanvas.rootCanvas : GetComponentInParent<Canvas>();

            ConfigureTextFitting();
            ApplySelectionState();
        }

        public void SetCard(CardData cardData)
        {
            CardData = cardData;

            if (cardData == null)
            {
                Clear();
                return;
            }

            SetText(nameText, cardData.CardName);
            SetText(typeText, GetTypeText(cardData.CardType));
            SetText(costOrLevelText, GetCostOrLevelText(cardData));
            SetText(attackText, $"ATK {cardData.Attack}");
            SetText(defenseText, $"DEF {cardData.Defense}");
            SetText(descriptionText, GetDescriptionText(cardData.Description));
            SetArtwork(cardData.Artwork);

            var showMonsterStats = cardData.CardType == CardType.Monster;
            SetActive(attackText, showMonsterStats);
            SetActive(defenseText, showMonsterStats);
            ApplyTypeColor(cardData.CardType);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelectable || isDragging)
            {
                return;
            }

            CacheRestingTransform();

            if (rectTransform != null)
            {
                AnimateTransform(restingLocalPosition + new Vector3(0f, HoverOffsetY, 0f), Vector3.one * baseScale * HoverScale);
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = true;
                sortingCanvas.sortingOrder = 10;
            }

            PointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelectable || isDragging)
            {
                return;
            }

            if (rectTransform != null)
            {
                AnimateTransform(restingLocalPosition, Vector3.one * baseScale);
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = false;
                sortingCanvas.sortingOrder = 0;
            }

            PointerExited?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isSelectable)
            {
                return;
            }

            Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isSelectable || rectTransform == null)
            {
                return;
            }

            StopHoverAnimation();
            CacheRestingTransform();

            isDragging = true;
            dropConsumed = false;
            dragOriginalParent = transform.parent;
            dragOriginalSiblingIndex = transform.GetSiblingIndex();
            CreateDragPlaceholder();
            rootCanvas = FindRootCanvas();
            dragPlane = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
            dragEventCamera = GetEventCamera(rootCanvas, eventData);
            dragPointerOffset = GetPointerWorldOffset(eventData);

            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            RebuildOriginalLayout();

            if (dragPlane != null)
            {
                transform.SetParent(dragPlane, true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.92f;
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = true;
                sortingCanvas.sortingOrder = 100;
            }

            PointerExited?.Invoke(this);
            DragStarted?.Invoke(this);
            UpdateDragPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || rectTransform == null)
            {
                return;
            }

            UpdateDragPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            var dropSucceeded = dropConsumed || TryResolveCardDrop(eventData);

            if (dropSucceeded)
            {
                eventData.pointerDrag = null;
                if (dropConsumed)
                {
                    RemoveDragPlaceholder();
                }
                else
                {
                    HideAfterSuccessfulDrop();
                }

                ResetDragState();
                return;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
            }

            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = false;
            }

            var returnSiblingIndex = GetReturnSiblingIndex();
            if (dragOriginalParent != null)
            {
                transform.SetParent(dragOriginalParent, false);
                transform.SetSiblingIndex(returnSiblingIndex);
            }

            RemoveDragPlaceholder();

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one * baseScale;
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = false;
                sortingCanvas.sortingOrder = 0;
            }

            RebuildOriginalLayout();
            ResetDragState();
        }

        public void HideAfterSuccessfulDrop()
        {
            dropConsumed = true;
            isDragging = false;
            StopHoverAnimation();
            RemoveDragPlaceholder();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = false;
                sortingCanvas.sortingOrder = 0;
            }

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (dragPlaceholder != null)
            {
                RemoveDragPlaceholder();
                RebuildOriginalLayout();
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            ApplySelectionState();
        }

        public void SetSelectable(bool selectable)
        {
            isSelectable = selectable;

            if (!isSelectable)
            {
                SetSelected(false);
            }
        }

        public void SetShowFullDescription(bool showFullDescription)
        {
            this.showFullDescription = showFullDescription;

            if (CardData != null)
            {
                SetText(descriptionText, GetDescriptionText(CardData.Description));
            }
        }

        public void SetBaseScale(float scale)
        {
            baseScale = scale;

            if (layoutElement != null && baseSize.x > 0f && baseSize.y > 0f)
            {
                layoutElement.preferredWidth = baseSize.x * baseScale;
                layoutElement.preferredHeight = baseSize.y * baseScale;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * baseScale;
            }
        }

        private void UpdateDragPosition(PointerEventData eventData)
        {
            if (dragPlane != null)
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        dragPlane,
                        eventData.position,
                        dragEventCamera,
                        out var worldPoint))
                {
                    rectTransform.position = worldPoint + dragPointerOffset;
                    return;
                }
            }

            rectTransform.position = eventData.position;
        }

        private bool TryResolveCardDrop(PointerEventData eventData)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            dragRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, dragRaycastResults);

            foreach (var result in dragRaycastResults)
            {
                if (result.gameObject == null || result.gameObject.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (TryFindCardDropTarget(result.gameObject.transform, out var dropTarget)
                    && dropTarget.TryDropCard(this))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindCardDropTarget(Transform startTransform, out ICardDropTarget dropTarget)
        {
            var currentTransform = startTransform;
            while (currentTransform != null)
            {
                var behaviours = currentTransform.GetComponents<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is ICardDropTarget target)
                    {
                        dropTarget = target;
                        return true;
                    }
                }

                currentTransform = currentTransform.parent;
            }

            dropTarget = null;
            return false;
        }

        private void ResetDragState()
        {
            RemoveDragPlaceholder();
            hasRestingTransform = false;
            dragPlane = null;
            dragEventCamera = null;
            dragPointerOffset = Vector3.zero;
            dragOriginalParent = null;
            dragOriginalSiblingIndex = 0;
            dropConsumed = false;
            dragRaycastResults.Clear();
        }

        private void RebuildOriginalLayout()
        {
            var parentRectTransform = dragOriginalParent as RectTransform;
            if (parentRectTransform == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
            Canvas.ForceUpdateCanvases();
        }

        private void CreateDragPlaceholder()
        {
            if (dragOriginalParent == null || dragPlaceholder != null)
            {
                return;
            }

            dragPlaceholder = new GameObject("CardDragPlaceholder", typeof(RectTransform), typeof(LayoutElement));
            dragPlaceholder.transform.SetParent(dragOriginalParent, false);
            dragPlaceholder.transform.SetSiblingIndex(dragOriginalSiblingIndex);

            var placeholderLayout = dragPlaceholder.GetComponent<LayoutElement>();
            placeholderLayout.minWidth = layoutElement != null ? layoutElement.minWidth : 0f;
            placeholderLayout.minHeight = layoutElement != null ? layoutElement.minHeight : 0f;
            placeholderLayout.preferredWidth = layoutElement != null && layoutElement.preferredWidth > 0f
                ? layoutElement.preferredWidth
                : rectTransform.rect.width;
            placeholderLayout.preferredHeight = layoutElement != null && layoutElement.preferredHeight > 0f
                ? layoutElement.preferredHeight
                : rectTransform.rect.height;
            placeholderLayout.flexibleWidth = 0f;
            placeholderLayout.flexibleHeight = 0f;
        }

        private int GetReturnSiblingIndex()
        {
            if (dragPlaceholder != null && dragPlaceholder.transform.parent == dragOriginalParent)
            {
                return dragPlaceholder.transform.GetSiblingIndex();
            }

            if (dragOriginalParent == null)
            {
                return 0;
            }

            return Mathf.Clamp(dragOriginalSiblingIndex, 0, dragOriginalParent.childCount);
        }

        private void RemoveDragPlaceholder()
        {
            if (dragPlaceholder == null)
            {
                return;
            }

            dragPlaceholder.SetActive(false);
            Destroy(dragPlaceholder);
            dragPlaceholder = null;
        }

        private Vector3 GetPointerWorldOffset(PointerEventData eventData)
        {
            if (dragPlane == null || rectTransform == null)
            {
                return Vector3.zero;
            }

            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    dragPlane,
                    eventData.position,
                    dragEventCamera,
                    out var worldPoint))
            {
                return Vector3.zero;
            }

            return rectTransform.position - worldPoint;
        }

        private Canvas FindRootCanvas()
        {
            var parentCanvas = transform.parent != null
                ? transform.parent.GetComponentInParent<Canvas>()
                : null;

            if (parentCanvas != null)
            {
                return parentCanvas.rootCanvas;
            }

            return sortingCanvas != null ? sortingCanvas.rootCanvas : GetComponentInParent<Canvas>()?.rootCanvas;
        }

        private static Camera GetEventCamera(Canvas canvas, PointerEventData eventData)
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            if (canvas != null && canvas.worldCamera != null)
            {
                return canvas.worldCamera;
            }

            return eventData.pressEventCamera;
        }

        private static string GetCostOrLevelText(CardData cardData)
        {
            return cardData.CardType == CardType.Monster
                ? $"LV {cardData.Level}"
                : $"Cost {cardData.Cost}";
        }

        private static string GetTypeText(CardType cardType)
        {
            return cardType switch
            {
                CardType.Spell => "Spell",
                CardType.Trap => "Trap",
                _ => "Monster"
            };
        }

        private string GetDescriptionText(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return string.Empty;
            }

            var trimmedDescription = description.Trim();
            return showFullDescription || trimmedDescription.Length <= MaxDescriptionCharacters
                ? trimmedDescription
                : trimmedDescription.Substring(0, MaxDescriptionCharacters) + "...";
        }

        private void Clear()
        {
            CardData = null;
            SetText(nameText, string.Empty);
            SetText(typeText, string.Empty);
            SetText(costOrLevelText, string.Empty);
            SetText(attackText, string.Empty);
            SetText(defenseText, string.Empty);
            SetText(descriptionText, string.Empty);
            SetArtwork(null);
        }

        private void SetArtwork(Sprite artwork)
        {
            if (artworkImage != null)
            {
                artworkImage.sprite = artwork;
                artworkImage.enabled = artwork != null;
                artworkImage.preserveAspect = true;
            }

            SetActive(artworkPlaceholderText, artwork == null);
        }

        private void ConfigureTextFitting()
        {
            ConfigureSingleLineText(nameText, 16f);
            ConfigureSingleLineText(typeText, 14f);
            ConfigureSingleLineText(costOrLevelText, 13f);
            ConfigureSingleLineText(attackText, 11f);
            ConfigureSingleLineText(defenseText, 11f);
            ConfigureSingleLineText(artworkPlaceholderText, 20f);
            ConfigureWrappedText(descriptionText, 10f);
        }

        private static void ConfigureSingleLineText(TMP_Text text, float fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Truncate;
        }

        private static void ConfigureWrappedText(TMP_Text text, float fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
        }

        private void ApplyTypeColor(CardType cardType)
        {
            var typeColor = GetTypeColor(cardType);

            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(typeColor, Color.white, 0.72f);
            }

            if (typeBannerImage != null)
            {
                typeBannerImage.color = typeColor;
            }
        }

        private void ApplySelectionState()
        {
            if (outline == null)
            {
                return;
            }

            outline.effectColor = isSelected ? SelectedOutlineColor : NormalOutlineColor;
            outline.effectDistance = isSelected ? new Vector2(4f, -4f) : new Vector2(2f, -2f);
        }

        private void CacheRestingTransform()
        {
            if (hasRestingTransform || rectTransform == null)
            {
                return;
            }

            restingLocalPosition = rectTransform.localPosition;
            hasRestingTransform = true;
        }

        private void AnimateTransform(Vector3 targetLocalPosition, Vector3 targetLocalScale)
        {
            StopHoverAnimation();
            hoverAnimation = StartCoroutine(AnimateTransformRoutine(targetLocalPosition, targetLocalScale));
        }

        private IEnumerator AnimateTransformRoutine(Vector3 targetLocalPosition, Vector3 targetLocalScale)
        {
            if (rectTransform == null)
            {
                yield break;
            }

            var startPosition = rectTransform.localPosition;
            var startScale = rectTransform.localScale;
            var elapsed = 0f;

            while (elapsed < HoverAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / HoverAnimationDuration);
                var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                rectTransform.localPosition = Vector3.Lerp(startPosition, targetLocalPosition, easedProgress);
                rectTransform.localScale = Vector3.Lerp(startScale, targetLocalScale, easedProgress);
                yield return null;
            }

            rectTransform.localPosition = targetLocalPosition;
            rectTransform.localScale = targetLocalScale;
            hoverAnimation = null;
        }

        private void StopHoverAnimation()
        {
            if (hoverAnimation == null)
            {
                return;
            }

            StopCoroutine(hoverAnimation);
            hoverAnimation = null;
        }

        private static Color GetTypeColor(CardType cardType)
        {
            return cardType switch
            {
                CardType.Spell => SpellColor,
                CardType.Trap => TrapColor,
                _ => MonsterColor
            };
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetActive(Component component, bool isActive)
        {
            if (component != null)
            {
                component.gameObject.SetActive(isActive);
            }
        }
    }
}
