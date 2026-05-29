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
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private Button startRestartButton;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField, Min(1)] private int cardsDrawnPerTurn = 1;

        private enum TurnPhase
        {
            Draw,
            Main,
            End
        }

        private int turnNumber = 1;
        private TurnPhase currentPhase = TurnPhase.Main;
        private DeckManager subscribedDeckManager;
        private bool wroteInitialLog;

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
            deckManager.StartNewGame();
            GameLogManager.Log("Game started.");
            GameLogManager.Log($"Opening hand: {deckManager.Hand.Count} card(s).");
            wroteInitialLog = true;
            RefreshView();
        }

        public void AdvancePhase()
        {
            if (deckManager == null)
            {
                Debug.LogWarning("턴 매니저에 덱 매니저 참조가 필요합니다.");
                return;
            }

            switch (currentPhase)
            {
                case TurnPhase.Main:
                    currentPhase = TurnPhase.End;
                    GameLogManager.Log("End Phase.");
                    break;
                case TurnPhase.End:
                    turnNumber++;
                    currentPhase = TurnPhase.Draw;
                    GameLogManager.Log($"Turn {turnNumber} - Draw Phase.");
                    DrawForTurn();
                    break;
                case TurnPhase.Draw:
                    currentPhase = TurnPhase.Main;
                    GameLogManager.Log("Main Phase.");
                    break;
            }

            RefreshView();
        }

        public void RefreshView()
        {
            if (turnText != null)
            {
                turnText.text = $"Turn {turnNumber}";
            }

            if (phaseText != null)
            {
                phaseText.text = GetPhaseText(currentPhase);
            }

            if (deckCountText != null && deckManager != null)
            {
                deckCountText.text = $"Deck {deckManager.DrawPile.Count}";
            }

            if (nextTurnButton != null)
            {
                nextTurnButton.interactable = deckManager != null;
            }

            if (startRestartButton != null)
            {
                startRestartButton.interactable = deckManager != null;
            }
        }

        private void DrawForTurn()
        {
            var drawnCards = deckManager.DrawCards(cardsDrawnPerTurn);
            if (drawnCards.Count == 0)
            {
                GameLogManager.Log("Deck is empty.");
                return;
            }

            GameLogManager.Log($"Drew {drawnCards.Count} card(s).");
        }

        private void WriteInitialLog()
        {
            if (wroteInitialLog || deckManager == null)
            {
                return;
            }

            GameLogManager.Log("Game ready.");
            GameLogManager.Log($"Opening hand: {deckManager.Hand.Count} card(s).");
            wroteInitialLog = true;
        }

        private static string GetPhaseText(TurnPhase phase)
        {
            return phase switch
            {
                TurnPhase.Draw => "Draw Phase",
                TurnPhase.End => "End Phase",
                _ => "Main Phase"
            };
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
    }
}
