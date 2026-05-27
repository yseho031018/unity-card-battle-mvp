using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.Gameplay
{
    public class CardActionManager : MonoBehaviour
    {
        [SerializeField] private Button useCardButton;
        [SerializeField] private Button setTrapButton;

        private DeckManager deckManager;
        private HandManager handManager;
        private TrapZoneManager trapZoneManager;
        private CardView selectedCardView;

        public void Initialize(DeckManager deckManager, HandManager handManager, TrapZoneManager trapZoneManager)
        {
            this.deckManager = deckManager;
            this.handManager = handManager;
            this.trapZoneManager = trapZoneManager;

            if (this.handManager != null)
            {
                this.handManager.CardSelected -= HandleCardSelected;
                this.handManager.CardSelected += HandleCardSelected;
            }

            ConfigureButtons();
            RefreshButtons();
        }

        private void Awake()
        {
            ConfigureButtons();
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (handManager != null)
            {
                handManager.CardSelected -= HandleCardSelected;
            }
        }

        private void ConfigureButtons()
        {
            if (useCardButton != null)
            {
                useCardButton.onClick.RemoveListener(UseSelectedCard);
                useCardButton.onClick.AddListener(UseSelectedCard);
            }

            if (setTrapButton != null)
            {
                setTrapButton.onClick.RemoveListener(SetSelectedTrap);
                setTrapButton.onClick.AddListener(SetSelectedTrap);
            }
        }

        private void HandleCardSelected(CardView cardView)
        {
            selectedCardView = cardView;
            RefreshButtons();
        }

        private void UseSelectedCard()
        {
            var cardData = selectedCardView != null ? selectedCardView.CardData : null;
            if (cardData == null || cardData.CardType != CardType.Spell)
            {
                Debug.Log("Select a spell card first.");
                return;
            }

            if (deckManager != null && deckManager.UseSpellFromHand(cardData))
            {
                selectedCardView = null;
                handManager?.ClearSelection();
                RefreshButtons();
            }
        }

        private void SetSelectedTrap()
        {
            var cardData = selectedCardView != null ? selectedCardView.CardData : null;
            if (cardData == null || cardData.CardType != CardType.Trap)
            {
                Debug.Log("Select a trap card first.");
                return;
            }

            if (trapZoneManager == null || deckManager == null)
            {
                Debug.LogWarning("Trap setup needs TrapZoneManager and DeckManager references.");
                return;
            }

            if (trapZoneManager.SetTrap(cardData) && deckManager.RemoveCardFromHand(cardData))
            {
                selectedCardView = null;
                handManager?.ClearSelection();
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            var cardData = selectedCardView != null ? selectedCardView.CardData : null;

            if (useCardButton != null)
            {
                useCardButton.interactable = cardData != null && cardData.CardType == CardType.Spell;
            }

            if (setTrapButton != null)
            {
                setTrapButton.interactable = cardData != null && cardData.CardType == CardType.Trap;
            }
        }
    }
}
