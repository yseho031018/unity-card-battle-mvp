using System.Collections.Generic;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class FieldManager : MonoBehaviour
    {
        [SerializeField] private List<FieldSlot> fieldSlots = new();
        [SerializeField] private CardView cardViewPrefab;

        private DeckManager deckManager;
        private HandManager handManager;
        private CardView selectedHandCard;

        public void Initialize(DeckManager deckManager, HandManager handManager)
        {
            this.deckManager = deckManager;
            this.handManager = handManager;

            if (this.handManager != null)
            {
                this.handManager.CardSelected -= HandleCardSelected;
                this.handManager.CardSelected += HandleCardSelected;
            }

            foreach (var slot in fieldSlots)
            {
                slot.Initialize(this);
            }
        }

        public void TryPlaceSelectedCard(FieldSlot slot)
        {
            if (TryPlaceCard(selectedHandCard, slot))
            {
                selectedHandCard = null;
            }
        }

        public bool TryPlaceCard(CardView cardView, FieldSlot slot)
        {
            if (deckManager == null || handManager == null)
            {
                Debug.LogWarning("FieldManager needs DeckManager and HandManager references.");
                return false;
            }

            if (slot == null || slot.IsOccupied)
            {
                return false;
            }

            if (cardView == null || cardView.CardData == null)
            {
                Debug.Log("Select a monster card from your hand first.");
                return false;
            }

            var cardData = cardView.CardData;
            if (cardData.CardType != CardType.Monster)
            {
                Debug.Log($"{cardData.CardName} cannot be placed in a monster zone.");
                return false;
            }

            if (!slot.SetCard(cardData, cardViewPrefab) || !deckManager.RemoveCardFromHand(cardData))
            {
                return false;
            }

            cardView.HideAfterSuccessfulDrop();
            selectedHandCard = null;
            handManager.ClearSelection();
            GameLogManager.Log($"{cardData.CardName} summoned.");
            return true;
        }

        public bool TryUseSpellCard(CardView cardView)
        {
            if (deckManager == null || handManager == null)
            {
                Debug.LogWarning("FieldManager needs DeckManager and HandManager references.");
                return false;
            }

            var cardData = cardView != null ? cardView.CardData : null;
            if (cardData == null || cardData.CardType != CardType.Spell)
            {
                return false;
            }

            if (!deckManager.UseSpellFromHand(cardData))
            {
                return false;
            }

            cardView.HideAfterSuccessfulDrop();
            selectedHandCard = null;
            handManager.ClearSelection();
            return true;
        }

        public void ClearField()
        {
            selectedHandCard = null;

            foreach (var slot in fieldSlots)
            {
                slot?.Clear();
            }
        }

        private void Awake()
        {
            foreach (var slot in fieldSlots)
            {
                slot.Initialize(this);
            }
        }

        private void OnDestroy()
        {
            if (handManager != null)
            {
                handManager.CardSelected -= HandleCardSelected;
            }
        }

        private void HandleCardSelected(CardView cardView)
        {
            selectedHandCard = cardView;
        }
    }
}
