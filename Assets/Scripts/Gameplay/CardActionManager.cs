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
        [SerializeField] private TurnManager turnManager;

        private DeckManager deckManager;
        private HandManager handManager;
        private TrapZoneManager trapZoneManager;
        private CardView selectedCardView;
        private TurnManager subscribedTurnManager;

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
            SubscribeToTurnManager(GetTurnManager());
            RefreshButtons();
        }

        private void Awake()
        {
            ConfigureButtons();
            SubscribeToTurnManager(GetTurnManager());
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (handManager != null)
            {
                handManager.CardSelected -= HandleCardSelected;
            }

            SubscribeToTurnManager(null);
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
                Debug.Log("먼저 마법 카드를 선택해주세요.");
                return false;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.CanUseMainPhaseActionWithLog("마법 카드"))
            {
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
                Debug.Log("먼저 함정 카드를 선택해주세요.");
                return false;
            }

            if (trapZoneManager == null || deckManager == null)
            {
                Debug.LogWarning("Trap setup needs TrapZoneManager and DeckManager references.");
                return false;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.CanUseMainPhaseActionWithLog("함정 카드"))
            {
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
            var canUseMainPhaseActions = GetTurnManager()?.CanUseMainPhaseActions ?? true;

            if (useCardButton != null)
            {
                useCardButton.interactable = canUseMainPhaseActions
                    && cardData != null
                    && cardData.CardType == CardType.Spell;
            }

            if (setTrapButton != null)
            {
                setTrapButton.interactable = canUseMainPhaseActions
                    && cardData != null
                    && cardData.CardType == CardType.Trap;
            }
        }

        private TurnManager GetTurnManager()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindAnyObjectByType<TurnManager>();
            }

            return turnManager;
        }

        private void SubscribeToTurnManager(TurnManager nextTurnManager)
        {
            if (subscribedTurnManager == nextTurnManager)
            {
                return;
            }

            if (subscribedTurnManager != null)
            {
                subscribedTurnManager.RuleStateChanged -= RefreshButtons;
            }

            subscribedTurnManager = nextTurnManager;

            if (subscribedTurnManager != null)
            {
                subscribedTurnManager.RuleStateChanged += RefreshButtons;
            }
        }
    }
}
