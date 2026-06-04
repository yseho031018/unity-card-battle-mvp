using System.Collections.Generic;
using System.Linq;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class FieldManager : MonoBehaviour
    {
        [SerializeField] private List<FieldSlot> fieldSlots = new();
        [SerializeField] private List<FieldSlot> enemyFieldSlots = new();
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private CardPreviewManager cardPreviewManager;

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
                slot.Initialize(this, GetBattleManager(), GetCardPreviewManager());
            }

            foreach (var slot in enemyFieldSlots)
            {
                slot.Initialize(this, GetBattleManager(), GetCardPreviewManager());
            }
        }

        public void TryPlaceSelectedCard(FieldSlot slot)
        {
            if (TryPlaceCard(selectedHandCard, slot))
            {
                selectedHandCard = null;
            }
        }

        public bool TryUseSelectedSpellCard()
        {
            return TryUseSpellCard(selectedHandCard);
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

            if (slot.IsEnemySlot)
            {
                Debug.Log("상대 필드에는 몬스터를 소환할 수 없습니다.");
                return false;
            }

            if (cardView == null || cardView.CardData == null)
            {
                Debug.Log("먼저 손패에서 몬스터 카드를 선택해주세요.");
                return false;
            }

            var cardData = cardView.CardData;
            if (cardData.CardType != CardType.Monster)
            {
                Debug.Log($"{cardData.CardName}은(는) 몬스터 존에 놓을 수 없습니다.");
                return false;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.CanNormalSummonWithLog())
            {
                return false;
            }

            if (!slot.SetCard(cardData, cardViewPrefab) || !deckManager.RemoveCardFromHand(cardData))
            {
                return false;
            }

            activeTurnManager?.RegisterNormalSummon();
            cardView.HideAfterSuccessfulDrop();
            selectedHandCard = null;
            handManager.ClearSelection();
            GameLogManager.Log($"{cardData.CardName}을(를) 소환했습니다.");
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

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.CanUseMainPhaseActionWithLog("마법 카드"))
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
            GetBattleManager()?.ClearSelection();

            foreach (var slot in fieldSlots)
            {
                slot?.Clear();
            }

            foreach (var slot in enemyFieldSlots)
            {
                slot?.Clear();
            }
        }

        public void ResetMonsterAttackStates()
        {
            foreach (var slot in fieldSlots)
            {
                slot?.ResetAttackState();
            }
        }

        public bool HasEnemyMonsters()
        {
            return enemyFieldSlots.Any(slot => slot != null && slot.IsOccupied);
        }

        public void SetupEnemyMonsters(IEnumerable<CardData> enemyCards)
        {
            foreach (var slot in enemyFieldSlots)
            {
                slot?.Clear();
            }

            if (enemyCards == null)
            {
                return;
            }

            var slotIndex = 0;
            var placedCount = 0;
            foreach (var cardData in enemyCards)
            {
                if (cardData == null || cardData.CardType != CardType.Monster)
                {
                    continue;
                }

                while (slotIndex < enemyFieldSlots.Count && enemyFieldSlots[slotIndex] == null)
                {
                    slotIndex++;
                }

                if (slotIndex >= enemyFieldSlots.Count)
                {
                    break;
                }

                if (enemyFieldSlots[slotIndex].SetCard(cardData, cardViewPrefab))
                {
                    placedCount++;
                }

                slotIndex++;
            }

            if (placedCount > 0)
            {
                GameLogManager.Log($"상대 필드에 몬스터 {placedCount}장을 배치했습니다.");
            }
        }

        private void Awake()
        {
            foreach (var slot in fieldSlots)
            {
                slot.Initialize(this, GetBattleManager(), GetCardPreviewManager());
            }

            foreach (var slot in enemyFieldSlots)
            {
                slot.Initialize(this, GetBattleManager(), GetCardPreviewManager());
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

        private TurnManager GetTurnManager()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindAnyObjectByType<TurnManager>();
            }

            return turnManager;
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
    }
}
