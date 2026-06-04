using CardBattle.Cards;
using CardBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class FieldSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler, ICardDropTarget
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CardPreviewManager cardPreviewManager;
        [SerializeField] private bool isEnemySlot;

        private FieldManager fieldManager;
        private CardView placedCardView;
        private CardData placedCardData;

        public bool IsOccupied => placedCardView != null;
        public bool IsEnemySlot => isEnemySlot;
        public CardData PlacedCardData => placedCardData;
        public bool HasAttackedThisTurn { get; private set; }

        public void Initialize(FieldManager fieldManager, BattleManager battleManager = null, CardPreviewManager cardPreviewManager = null)
        {
            this.fieldManager = fieldManager;

            if (battleManager != null)
            {
                this.battleManager = battleManager;
            }

            if (cardPreviewManager != null)
            {
                this.cardPreviewManager = cardPreviewManager;
            }
        }

        public bool SetCard(CardData cardData, CardView cardViewPrefab)
        {
            if (IsOccupied || cardData == null || cardViewPrefab == null)
            {
                return false;
            }

            placedCardData = cardData;
            HasAttackedThisTurn = false;
            placedCardView = Instantiate(cardViewPrefab, transform);
            placedCardView.SetCard(cardData);
            placedCardView.SetSelectable(false);
            DisableCardRaycasts(placedCardView);

            var rectTransform = placedCardView.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            FitCardToSlot(rectTransform);

            if (labelText != null)
            {
                labelText.gameObject.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = isEnemySlot
                    ? new Color(0.2f, 0.09f, 0.08f, 0.76f)
                    : new Color(0.12f, 0.16f, 0.2f, 0.65f);
            }

            return true;
        }

        public void Clear()
        {
            if (placedCardView != null)
            {
                placedCardView.SetSelected(false);
                Destroy(placedCardView.gameObject);
                placedCardView = null;
            }

            placedCardData = null;
            HasAttackedThisTurn = false;

            if (labelText != null)
            {
                labelText.gameObject.SetActive(true);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = isEnemySlot
                    ? new Color(0.18f, 0.08f, 0.08f, 0.62f)
                    : new Color(0.08f, 0.12f, 0.16f, 0.58f);
            }
        }

        public void MarkAttackedThisTurn()
        {
            HasAttackedThisTurn = true;
        }

        public void ResetAttackState()
        {
            HasAttackedThisTurn = false;
        }

        public void SetBattleSelected(bool selected)
        {
            if (placedCardView != null)
            {
                placedCardView.SetSelected(selected);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (fieldManager != null && fieldManager.TryUseSelectedSpellCard())
            {
                return;
            }

            if (IsOccupied)
            {
                if (isEnemySlot)
                {
                    GetBattleManager()?.TryAttackTarget(this);
                    return;
                }

                GetBattleManager()?.SelectAttacker(this);
                return;
            }

            if (isEnemySlot)
            {
                return;
            }

            fieldManager?.TryPlaceSelectedCard(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsOccupied || placedCardData == null)
            {
                return;
            }

            GetCardPreviewManager()?.Show(placedCardData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GetCardPreviewManager()?.Hide();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cardView = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<CardView>()
                : null;

            TryDropCard(cardView);
        }

        public bool TryDropCard(CardView cardView)
        {
            if (isEnemySlot || cardView == null || cardView.CardData == null || fieldManager == null)
            {
                return false;
            }

            var succeeded = cardView.CardData.CardType switch
            {
                CardType.Monster => fieldManager.TryPlaceCard(cardView, this),
                CardType.Spell => fieldManager.TryUseSpellCard(cardView),
                _ => false
            };

            return succeeded;
        }

        private void FitCardToSlot(RectTransform cardRectTransform)
        {
            var slotRectTransform = transform as RectTransform;
            if (slotRectTransform == null || cardRectTransform == null)
            {
                return;
            }

            var slotSize = slotRectTransform.rect.size;
            var cardSize = cardRectTransform.rect.size;
            if (slotSize.x <= 0f || slotSize.y <= 0f || cardSize.x <= 0f || cardSize.y <= 0f)
            {
                return;
            }

            var scale = Mathf.Min(slotSize.x / cardSize.x, slotSize.y / cardSize.y);
            cardRectTransform.localScale = Vector3.one * Mathf.Min(scale, 1f);
        }

        private BattleManager GetBattleManager()
        {
            if (battleManager == null)
            {
                battleManager = Object.FindAnyObjectByType<BattleManager>();
            }

            return battleManager;
        }

        private CardPreviewManager GetCardPreviewManager()
        {
            if (cardPreviewManager == null)
            {
                cardPreviewManager = Object.FindAnyObjectByType<CardPreviewManager>();
            }

            return cardPreviewManager;
        }

        private static void DisableCardRaycasts(CardView cardView)
        {
            if (cardView == null)
            {
                return;
            }

            var graphics = cardView.GetComponentsInChildren<Graphic>();
            foreach (var graphic in graphics)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}
