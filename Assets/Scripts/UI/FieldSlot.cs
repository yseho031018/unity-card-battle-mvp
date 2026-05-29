using CardBattle.Cards;
using CardBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class FieldSlot : MonoBehaviour, IPointerClickHandler, IDropHandler, ICardDropTarget
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        private FieldManager fieldManager;
        private CardView placedCardView;

        public bool IsOccupied => placedCardView != null;

        public void Initialize(FieldManager fieldManager)
        {
            this.fieldManager = fieldManager;
        }

        public bool SetCard(CardData cardData, CardView cardViewPrefab)
        {
            if (IsOccupied || cardData == null || cardViewPrefab == null)
            {
                return false;
            }

            placedCardView = Instantiate(cardViewPrefab, transform);
            placedCardView.SetCard(cardData);
            placedCardView.SetSelectable(false);

            var rectTransform = placedCardView.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            if (labelText != null)
            {
                labelText.gameObject.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.12f, 0.16f, 0.2f, 0.65f);
            }

            return true;
        }

        public void Clear()
        {
            if (placedCardView != null)
            {
                Destroy(placedCardView.gameObject);
                placedCardView = null;
            }

            if (labelText != null)
            {
                labelText.gameObject.SetActive(true);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.08f, 0.12f, 0.16f, 0.58f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            fieldManager?.TryPlaceSelectedCard(this);
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
            if (cardView == null || cardView.CardData == null || fieldManager == null)
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
    }
}
