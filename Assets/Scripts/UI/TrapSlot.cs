using CardBattle.Cards;
using CardBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class TrapSlot : MonoBehaviour, IDropHandler, ICardDropTarget
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text labelText;

        private CardActionManager cardActionManager;
        private CardData trapCard;
        private GameObject cardBackRoot;

        public bool IsOccupied => trapCard != null;

        public void Initialize(CardActionManager cardActionManager)
        {
            this.cardActionManager = cardActionManager;
        }

        public void SetTrap(CardData cardData)
        {
            trapCard = cardData;
            cardBackRoot = CardBackVisual.Ensure(transform, 10f);
            cardBackRoot.SetActive(true);

            if (labelText != null)
            {
                labelText.gameObject.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.08f, 0.06f, 0.11f, 0.92f);
            }
        }

        public void ClearIfEmpty()
        {
            if (trapCard != null)
            {
                return;
            }

            ResetVisual();
        }

        public void Clear()
        {
            trapCard = null;
            ResetVisual();
        }

        private void ResetVisual()
        {
            if (labelText != null)
            {
                labelText.gameObject.SetActive(true);
                labelText.text = "TRAP ZONE";
            }

            cardBackRoot = transform.Find(CardBackVisual.RootName)?.gameObject;
            if (cardBackRoot != null)
            {
                cardBackRoot.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.1f, 0.08f, 0.16f, 0.72f);
            }
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
            if (cardView == null || cardView.CardData == null || cardActionManager == null)
            {
                return false;
            }

            var succeeded = cardView.CardData.CardType switch
            {
                CardType.Spell => cardActionManager.TryUseCard(cardView),
                CardType.Trap => cardActionManager.TrySetTrap(cardView, this),
                _ => false
            };

            return succeeded;
        }
    }
}
