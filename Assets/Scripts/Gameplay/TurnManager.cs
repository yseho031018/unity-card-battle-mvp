using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.Gameplay
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField, Min(1)] private int cardsDrawnPerTurn = 1;

        private int turnNumber = 1;

        private void Awake()
        {
            if (nextTurnButton != null)
            {
                nextTurnButton.onClick.RemoveListener(AdvanceTurn);
                nextTurnButton.onClick.AddListener(AdvanceTurn);
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            RefreshView();
        }

        public void AdvanceTurn()
        {
            if (deckManager == null)
            {
                Debug.LogWarning("TurnManager needs a DeckManager reference.");
                return;
            }

            turnNumber++;
            deckManager.DrawCards(cardsDrawnPerTurn);
            RefreshView();
        }

        public void RefreshView()
        {
            if (turnText != null)
            {
                turnText.text = $"Turn {turnNumber}";
            }

            if (deckCountText != null && deckManager != null)
            {
                deckCountText.text = $"Deck {deckManager.DrawPile.Count}";
            }

            if (nextTurnButton != null && deckManager != null)
            {
                nextTurnButton.interactable = deckManager.DrawPile.Count > 0;
            }
        }
    }
}
