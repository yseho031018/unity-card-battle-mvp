using System;
using System.Collections;
using CardBattle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.Gameplay
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private Button startRestartButton;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField] private TMP_Text summonText;
        [SerializeField] private Text turnLegacyText;
        [SerializeField] private Text phaseLegacyText;
        [SerializeField] private Text deckCountLegacyText;
        [SerializeField] private Text summonLegacyText;
        [SerializeField, Min(1)] private int cardsDrawnPerTurn = 1;

        private enum TurnPhase
        {
            Draw,
            Main,
            Battle,
            End
        }

        private int turnNumber = 1;
        private TurnPhase currentPhase = TurnPhase.Main;
        private DeckManager subscribedDeckManager;
        private bool wroteInitialLog;
        private bool hasNormalSummonedThisTurn;

        public event Action RuleStateChanged;

        public bool IsMainPhase => currentPhase == TurnPhase.Main;
        public bool IsBattlePhase => currentPhase == TurnPhase.Battle;
        public bool IsGameOver => GetBattleManager()?.IsGameOver ?? false;
        public bool CanUseMainPhaseActions => IsMainPhase && !IsGameOver;
        public bool CanNormalSummon => IsMainPhase && !hasNormalSummonedThisTurn && !IsGameOver;

        private void Awake()
        {
            if (nextTurnButton != null)
            {
                nextTurnButton.onClick.RemoveListener(AdvancePhase);
                nextTurnButton.onClick.AddListener(AdvancePhase);
            }

            if (startRestartButton != null)
            {
                startRestartButton.onClick.RemoveListener(StartOrRestartGame);
                startRestartButton.onClick.AddListener(StartOrRestartGame);
            }

            SubscribeToDeckManager();
        }

        private IEnumerator Start()
        {
            yield return null;
            SubscribeToDeckManager();
            WriteInitialLog();
            RefreshView();
        }

        private void OnDestroy()
        {
            if (nextTurnButton != null)
            {
                nextTurnButton.onClick.RemoveListener(AdvancePhase);
            }

            if (startRestartButton != null)
            {
                startRestartButton.onClick.RemoveListener(StartOrRestartGame);
            }

            UnsubscribeFromDeckManager();
        }

        public void StartOrRestartGame()
        {
            if (deckManager == null)
            {
                Debug.LogWarning("턴 매니저에 덱 매니저 참조가 필요합니다.");
                return;
            }

            GameLogManager.ClearLog();
            turnNumber = 1;
            currentPhase = TurnPhase.Main;
            hasNormalSummonedThisTurn = false;
            GetBattleManager()?.ResetBattle();
            deckManager.StartNewGame();
            GameLogManager.Log("게임을 시작했습니다.");
            GameLogManager.Log($"시작 손패: {deckManager.Hand.Count}장");
            wroteInitialLog = true;
            RefreshView();
            NotifyRuleStateChanged();
        }

        public void AdvancePhase()
        {
            if (deckManager == null)
            {
                Debug.LogWarning("턴 매니저에 덱 매니저 참조가 필요합니다.");
                return;
            }

            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                return;
            }

            switch (currentPhase)
            {
                case TurnPhase.Main:
                    currentPhase = TurnPhase.Battle;
                    GameLogManager.Log("배틀 페이즈");
                    break;
                case TurnPhase.Battle:
                    currentPhase = TurnPhase.End;
                    GetBattleManager()?.ClearSelection();
                    GameLogManager.Log("엔드 페이즈");
                    break;
                case TurnPhase.End:
                    turnNumber++;
                    currentPhase = TurnPhase.Draw;
                    hasNormalSummonedThisTurn = false;
                    GetBattleManager()?.ResetAttacksForNewTurn();
                    GameLogManager.Log($"{turnNumber}턴 - 드로우 페이즈");
                    DrawForTurn();
                    break;
                case TurnPhase.Draw:
                    currentPhase = TurnPhase.Main;
                    GameLogManager.Log("메인 페이즈");
                    break;
            }

            RefreshView();
            NotifyRuleStateChanged();
        }

        public void RefreshView()
        {
            SetText(turnText, turnLegacyText, $"{turnNumber}턴");

            SetText(phaseText, phaseLegacyText, IsGameOver ? "게임 종료" : GetPhaseText(currentPhase));

            if (deckCountText != null && deckManager != null)
            {
                deckCountText.text = $"덱 {deckManager.DrawPile.Count}";
            }
            if (deckCountLegacyText != null && deckManager != null)
            {
                deckCountLegacyText.text = $"덱 {deckManager.DrawPile.Count}";
            }

            SetText(summonText, summonLegacyText, $"소환 {(hasNormalSummonedThisTurn ? 1 : 0)}/1");

            if (nextTurnButton != null)
            {
                nextTurnButton.interactable = deckManager != null && !IsGameOver;
            }

            if (startRestartButton != null)
            {
                startRestartButton.interactable = deckManager != null;
            }
        }

        public void RefreshRuleState()
        {
            RefreshView();
            NotifyRuleStateChanged();
        }

        public bool CanUseMainPhaseActionWithLog(string actionName)
        {
            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                return false;
            }

            if (IsMainPhase)
            {
                return true;
            }

            GameLogManager.Log($"{actionName}은(는) 메인 페이즈에만 사용할 수 있습니다.");
            return false;
        }

        public bool CanNormalSummonWithLog()
        {
            if (IsGameOver)
            {
                GameLogManager.Log("게임이 종료되었습니다. 시작 / 재시작을 눌러주세요.");
                return false;
            }

            if (!IsMainPhase)
            {
                GameLogManager.Log("몬스터는 메인 페이즈에만 소환할 수 있습니다.");
                return false;
            }

            if (hasNormalSummonedThisTurn)
            {
                GameLogManager.Log("일반 소환은 한 턴에 한 번만 가능합니다.");
                return false;
            }

            return true;
        }

        public void RegisterNormalSummon()
        {
            hasNormalSummonedThisTurn = true;
            RefreshView();
            NotifyRuleStateChanged();
        }

        private void DrawForTurn()
        {
            var drawnCards = deckManager.DrawCards(cardsDrawnPerTurn);
            if (drawnCards.Count == 0)
            {
                GetBattleManager()?.LoseByDeckOut();
                return;
            }

            GameLogManager.Log($"{drawnCards.Count}장을 드로우했습니다.");
        }

        private void WriteInitialLog()
        {
            if (wroteInitialLog || deckManager == null)
            {
                return;
            }

            GameLogManager.Log("게임 준비 완료");
            GameLogManager.Log($"시작 손패: {deckManager.Hand.Count}장");
            wroteInitialLog = true;
        }

        private static string GetPhaseText(TurnPhase phase)
        {
            return phase switch
            {
                TurnPhase.Draw => "드로우 페이즈",
                TurnPhase.Battle => "배틀 페이즈",
                TurnPhase.End => "엔드 페이즈",
                _ => "메인 페이즈"
            };
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

        private void NotifyRuleStateChanged()
        {
            RuleStateChanged?.Invoke();
        }

        private void SubscribeToDeckManager()
        {
            if (subscribedDeckManager == deckManager)
            {
                return;
            }

            UnsubscribeFromDeckManager();

            if (deckManager == null)
            {
                return;
            }

            subscribedDeckManager = deckManager;
            subscribedDeckManager.StateChanged += RefreshView;
        }

        private void UnsubscribeFromDeckManager()
        {
            if (subscribedDeckManager == null)
            {
                return;
            }

            subscribedDeckManager.StateChanged -= RefreshView;
            subscribedDeckManager = null;
        }

        private BattleManager GetBattleManager()
        {
            if (battleManager == null)
            {
                battleManager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            }

            return battleManager;
        }
    }
}
