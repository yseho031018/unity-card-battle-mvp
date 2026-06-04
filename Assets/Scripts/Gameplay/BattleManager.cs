using CardBattle.Cards;
using CardBattle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.Gameplay
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private Button attackButton;
        [SerializeField] private TMP_Text playerLifeText;
        [SerializeField] private TMP_Text enemyLifeText;
        [SerializeField] private TMP_Text selectedAttackerText;
        [SerializeField] private Text playerLifeLegacyText;
        [SerializeField] private Text enemyLifeLegacyText;
        [SerializeField] private Text selectedAttackerLegacyText;
        [SerializeField, Min(1)] private int startingPlayerLife = 8000;
        [SerializeField, Min(1)] private int startingEnemyLife = 8000;

        private int playerLife;
        private int enemyLife;
        private FieldSlot selectedAttacker;
        private TurnManager subscribedTurnManager;

        public bool IsGameOver { get; private set; }
        public int PlayerLife => playerLife;
        public int EnemyLife => enemyLife;

        private void Awake()
        {
            playerLife = startingPlayerLife;
            enemyLife = startingEnemyLife;
            ConfigureButton();
            SubscribeToTurnManager(GetTurnManager());
            RefreshView();
        }

        private void Start()
        {
            SubscribeToTurnManager(GetTurnManager());
            RefreshView();
        }

        private void OnDestroy()
        {
            if (attackButton != null)
            {
                attackButton.onClick.RemoveListener(TryDirectAttack);
            }

            SubscribeToTurnManager(null);
        }

        public void ResetBattle()
        {
            playerLife = startingPlayerLife;
            enemyLife = startingEnemyLife;
            selectedAttacker = null;
            IsGameOver = false;
            GetFieldManager()?.ResetMonsterAttackStates();
            RefreshView();
        }

        public void ResetAttacksForNewTurn()
        {
            GetFieldManager()?.ResetMonsterAttackStates();
            ClearSelection();
        }

        public void LoseByDeckOut()
        {
            EndGame("패배. 드로우할 카드가 없습니다.");
        }

        public void ClearSelection()
        {
            selectedAttacker = null;
            RefreshView();
        }

        public void SelectAttacker(FieldSlot slot)
        {
            if (slot == null || !slot.IsOccupied)
            {
                return;
            }

            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                return;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.IsBattlePhase)
            {
                GameLogManager.Log("몬스터는 배틀 페이즈에만 공격할 수 있습니다.");
                return;
            }

            if (slot.HasAttackedThisTurn)
            {
                GameLogManager.Log($"{slot.PlacedCardData.CardName}은(는) 이미 이번 턴에 공격했습니다.");
                return;
            }

            selectedAttacker = slot;
            GameLogManager.Log($"{slot.PlacedCardData.CardName}을(를) 공격 몬스터로 선택했습니다.");
            RefreshView();
        }

        public void TryDirectAttack()
        {
            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                RefreshView();
                return;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.IsBattlePhase)
            {
                GameLogManager.Log("직접 공격은 배틀 페이즈에만 가능합니다.");
                RefreshView();
                return;
            }

            if (selectedAttacker == null || !selectedAttacker.IsOccupied || selectedAttacker.PlacedCardData == null)
            {
                GameLogManager.Log("먼저 내 필드의 몬스터를 선택해주세요.");
                RefreshView();
                return;
            }

            if (GetFieldManager()?.HasEnemyMonsters() == true)
            {
                GameLogManager.Log("상대 몬스터가 있으면 직접 공격할 수 없습니다.");
                RefreshView();
                return;
            }

            if (selectedAttacker.HasAttackedThisTurn)
            {
                GameLogManager.Log($"{selectedAttacker.PlacedCardData.CardName}은(는) 이미 이번 턴에 공격했습니다.");
                ClearSelection();
                return;
            }

            var attackerData = selectedAttacker.PlacedCardData;
            var damage = Mathf.Max(0, attackerData.Attack);
            enemyLife = Mathf.Max(0, enemyLife - damage);
            selectedAttacker.MarkAttackedThisTurn();

            GameLogManager.Log($"{attackerData.CardName}이(가) 직접 공격하여 {damage} 데미지를 주었습니다.");

            if (enemyLife <= 0)
            {
                EndGame("승리! 상대 LP가 0이 되었습니다.");
            }

            activeTurnManager?.RefreshRuleState();
            ClearSelection();
        }

        public void TryAttackTarget(FieldSlot targetSlot)
        {
            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                RefreshView();
                return;
            }

            var activeTurnManager = GetTurnManager();
            if (activeTurnManager != null && !activeTurnManager.IsBattlePhase)
            {
                GameLogManager.Log("몬스터 전투는 배틀 페이즈에만 가능합니다.");
                RefreshView();
                return;
            }

            if (selectedAttacker == null || !selectedAttacker.IsOccupied || selectedAttacker.PlacedCardData == null)
            {
                GameLogManager.Log("먼저 공격할 내 몬스터를 선택해주세요.");
                RefreshView();
                return;
            }

            if (selectedAttacker.HasAttackedThisTurn)
            {
                GameLogManager.Log($"{selectedAttacker.PlacedCardData.CardName}은(는) 이미 이번 턴에 공격했습니다.");
                ClearSelection();
                return;
            }

            if (targetSlot == null || !targetSlot.IsEnemySlot || !targetSlot.IsOccupied || targetSlot.PlacedCardData == null)
            {
                GameLogManager.Log("공격 대상이 될 상대 몬스터를 선택해주세요.");
                RefreshView();
                return;
            }

            ResolveMonsterBattle(selectedAttacker, targetSlot);
            activeTurnManager?.RefreshView();
            ClearSelection();
        }

        public void RefreshView()
        {
            SetText(playerLifeText, playerLifeLegacyText, $"내 LP {playerLife}");

            SetText(enemyLifeText, enemyLifeLegacyText, $"상대 LP {enemyLife}");

            var attackerText = selectedAttacker != null && selectedAttacker.PlacedCardData != null
                ? $"공격 몬스터: {selectedAttacker.PlacedCardData.CardName}"
                : "공격 몬스터: 없음";
            SetText(selectedAttackerText, selectedAttackerLegacyText, attackerText);

            if (attackButton != null)
            {
                attackButton.interactable = CanAttackSelected();
            }
        }

        private bool CanAttackSelected()
        {
            if (IsGameOver || selectedAttacker == null || !selectedAttacker.IsOccupied || selectedAttacker.HasAttackedThisTurn)
            {
                return false;
            }

            if (GetFieldManager()?.HasEnemyMonsters() == true)
            {
                return false;
            }

            var activeTurnManager = GetTurnManager();
            return activeTurnManager == null || activeTurnManager.IsBattlePhase;
        }

        private static void SetText(TMP_Text tmpText, Text legacyText, string value)
        {
            if (tmpText != null)
            {
                tmpText.text = value;
            }

            if (legacyText != null)
            {
                legacyText.text = value;
            }
        }

        private void ResolveMonsterBattle(FieldSlot attackerSlot, FieldSlot defenderSlot)
        {
            var attackerData = attackerSlot.PlacedCardData;
            var defenderData = defenderSlot.PlacedCardData;
            var attackerAttack = Mathf.Max(0, attackerData.Attack);
            var defenderAttack = Mathf.Max(0, defenderData.Attack);

            attackerSlot.MarkAttackedThisTurn();
            GameLogManager.Log($"{attackerData.CardName}이(가) {defenderData.CardName}을(를) 공격했습니다.");

            if (attackerAttack > defenderAttack)
            {
                var damage = attackerAttack - defenderAttack;
                SendToGraveyard(defenderData);
                defenderSlot.Clear();
                enemyLife = Mathf.Max(0, enemyLife - damage);
                GameLogManager.Log($"{defenderData.CardName} 파괴. 상대가 {damage} 데미지를 받았습니다.");
            }
            else if (attackerAttack < defenderAttack)
            {
                var damage = defenderAttack - attackerAttack;
                SendToGraveyard(attackerData);
                attackerSlot.Clear();
                playerLife = Mathf.Max(0, playerLife - damage);
                GameLogManager.Log($"{attackerData.CardName} 파괴. 내가 {damage} 데미지를 받았습니다.");
            }
            else
            {
                SendToGraveyard(attackerData);
                SendToGraveyard(defenderData);
                attackerSlot.Clear();
                defenderSlot.Clear();
                GameLogManager.Log("두 몬스터가 모두 파괴되었습니다.");
            }

            CheckGameOver();
        }

        private void CheckGameOver()
        {
            if (enemyLife <= 0)
            {
                EndGame("승리! 상대 LP가 0이 되었습니다.");
                return;
            }

            if (playerLife <= 0)
            {
                EndGame("패배. 내 LP가 0이 되었습니다.");
            }
        }

        private void EndGame(string message)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            selectedAttacker = null;
            GameLogManager.Log(message);
            RefreshView();
            GetTurnManager()?.RefreshRuleState();
        }

        private void SendToGraveyard(CardData cardData)
        {
            GetDeckManager()?.SendToGraveyard(cardData);
        }

        private void ConfigureButton()
        {
            if (attackButton == null)
            {
                return;
            }

            attackButton.onClick.RemoveListener(TryDirectAttack);
            attackButton.onClick.AddListener(TryDirectAttack);
        }

        private TurnManager GetTurnManager()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindAnyObjectByType<TurnManager>();
            }

            return turnManager;
        }

        private DeckManager GetDeckManager()
        {
            if (deckManager == null)
            {
                deckManager = Object.FindAnyObjectByType<DeckManager>();
            }

            return deckManager;
        }

        private FieldManager GetFieldManager()
        {
            if (fieldManager == null)
            {
                fieldManager = Object.FindAnyObjectByType<FieldManager>();
            }

            return fieldManager;
        }

        private void SubscribeToTurnManager(TurnManager nextTurnManager)
        {
            if (subscribedTurnManager == nextTurnManager)
            {
                return;
            }

            if (subscribedTurnManager != null)
            {
                subscribedTurnManager.RuleStateChanged -= HandleRuleStateChanged;
            }

            subscribedTurnManager = nextTurnManager;

            if (subscribedTurnManager != null)
            {
                subscribedTurnManager.RuleStateChanged += HandleRuleStateChanged;
            }
        }

        private void HandleRuleStateChanged()
        {
            if (GetTurnManager()?.IsBattlePhase != true)
            {
                selectedAttacker = null;
            }

            RefreshView();
        }
    }
}
