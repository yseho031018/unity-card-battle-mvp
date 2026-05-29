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
                ConfigureDropTarget(useCardButton, CardActionDropTarget.DropAction.UseSpell);
            }

            if (setTrapButton != null)
            {
                setTrapButton.onClick.RemoveListener(SetSelectedTrap);
                setTrapButton.onClick.AddListener(SetSelectedTrap);
                ConfigureDropTarget(setTrapButton, CardActionDropTarget.DropAction.SetTrap);
            }
        }

        private void ConfigureDropTarget(Button button, CardActionDropTarget.DropAction dropAction)
        {
            if (button == null)
            {
                return;
            }

            var dropTarget = button.GetComponent<CardActionDropTarget>();
            if (dropTarget == null)
            {
                dropTarget = button.gameObject.AddComponent<CardActionDropTarget>();
            }

            dropTarget.Initialize(this, dropAction);
        }

        private void HandleCardSelected(CardView cardView)
        {
            selectedCardView = cardView;
            RefreshButtons();
        }

        private void UseSelectedCard()
        {
            TryUseCard(selectedCardView);
        }

        private void SetSelectedTrap()
        {
            TrySetTrap(selectedCardView);
        }

        public bool TryUseCard(CardView cardView)
        {
            var cardData = cardView != null ? cardView.CardData : null;
            if (cardData == null || cardData.CardType != CardType.Spell)
            {
                Debug.Log("Select a spell card first.");
                return false;
            }

            if (deckManager == null || !deckManager.UseSpellFromHand(cardData))
            {
                return false;
            }

            cardView.HideAfterSuccessfulDrop();
            selectedCardView = null;
            handManager?.ClearSelection();
            RefreshButtons();
            return true;
        }

        public bool TrySetTrap(CardView cardView, TrapSlot preferredSlot = null)
        {
            var cardData = cardView != null ? cardView.CardData : null;
            if (cardData == null || cardData.CardType != CardType.Trap)
            {
                Debug.Log("Select a trap card first.");
                return false;
            }

            if (trapZoneManager == null || deckManager == null)
            {
                Debug.LogWarning("Trap setup needs TrapZoneManager and DeckManager references.");
                return false;
            }

            if (!trapZoneManager.SetTrap(cardData, preferredSlot) || !deckManager.RemoveCardFromHand(cardData))
            {
                return false;
            }

            cardView.HideAfterSuccessfulDrop();
            selectedCardView = null;
            handManager?.ClearSelection();
            RefreshButtons();
            return true;
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
