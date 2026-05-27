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
            if (deckManager == null || handManager == null)
            {
                Debug.LogWarning("FieldManager is not initialized with DeckManager and HandManager.");
                return;
            }

            if (slot == null || slot.IsOccupied)
            {
                return;
            }

            if (selectedHandCard == null || selectedHandCard.CardData == null)
            {
                Debug.Log("Select a monster card from your hand first.");
                return;
            }

            var cardData = selectedHandCard.CardData;
            if (cardData.CardType != CardType.Monster)
            {
                Debug.Log($"{cardData.CardName} cannot be placed on the monster field yet.");
                return;
            }

            if (slot.SetCard(cardData, cardViewPrefab) && deckManager.RemoveCardFromHand(cardData))
            {
                selectedHandCard = null;
                handManager.ClearSelection();
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
