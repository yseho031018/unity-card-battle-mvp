using System;
using CardBattle.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Frame")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image typeBannerImage;
        [SerializeField] private Outline outline;
        [SerializeField] private Canvas sortingCanvas;

        [Header("Text")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text costOrLevelText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text descriptionText;

        private static readonly Color MonsterColor = new(0.86f, 0.51f, 0.28f);
        private static readonly Color SpellColor = new(0.25f, 0.61f, 0.56f);
        private static readonly Color TrapColor = new(0.64f, 0.29f, 0.58f);
        private static readonly Color NormalOutlineColor = new(0.06f, 0.06f, 0.07f, 0.95f);
        private static readonly Color SelectedOutlineColor = new(1f, 0.84f, 0.28f, 1f);

        private RectTransform rectTransform;
        private LayoutElement layoutElement;
        private Vector3 restingLocalPosition;
        private float baseScale = 1f;
        private Vector2 baseSize;
        private bool isSelected;
        private bool isSelectable = true;
        private bool hasRestingTransform;

        public event Action<CardView> Clicked;
        public CardData CardData { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            layoutElement = GetComponent<LayoutElement>();
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
            SetText(typeText, cardData.CardType.ToString());
            SetText(costOrLevelText, GetCostOrLevelText(cardData));
            SetText(attackText, $"ATK {cardData.Attack}");
            SetText(defenseText, $"DEF {cardData.Defense}");
            SetText(descriptionText, cardData.Description);

            var showMonsterStats = cardData.CardType == CardType.Monster;
            SetActive(attackText, showMonsterStats);
            SetActive(defenseText, showMonsterStats);
            ApplyTypeColor(cardData.CardType);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelectable)
            {
                return;
            }

            CacheRestingTransform();

            if (rectTransform != null)
            {
                rectTransform.localPosition = restingLocalPosition + new Vector3(0f, 28f, 0f);
                rectTransform.localScale = Vector3.one * baseScale * 1.06f;
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = true;
                sortingCanvas.sortingOrder = 10;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelectable)
            {
                return;
            }

            if (rectTransform != null)
            {
                rectTransform.localPosition = restingLocalPosition;
                rectTransform.localScale = Vector3.one * baseScale;
            }

            if (sortingCanvas != null)
            {
                sortingCanvas.overrideSorting = false;
                sortingCanvas.sortingOrder = 0;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isSelectable)
            {
                return;
            }

            Clicked?.Invoke(this);
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

        private static string GetCostOrLevelText(CardData cardData)
        {
            return cardData.CardType == CardType.Monster
                ? $"LV {cardData.Level}"
                : $"Cost {cardData.Cost}";
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
