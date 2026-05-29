using System;
using System.Collections.Generic;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class DeckManager : MonoBehaviour
    {
        private const string DrawScrollCardName = "Draw Scroll";
        private const int DrawScrollAmount = 2;

        [SerializeField] private List<CardData> startingDeck = new();
        [SerializeField] private HandManager handManager;
        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private TrapZoneManager trapZoneManager;
        [SerializeField] private CardActionManager cardActionManager;
        [SerializeField] private DeckView deckView;
        [SerializeField] private GraveyardView graveyardView;
        [SerializeField, Min(0)] private int openingHandSize = 5;
        [SerializeField] private bool drawOpeningHandOnStart = true;

        private readonly List<CardData> drawPile = new();
        private readonly List<CardData> hand = new();
        private readonly List<CardData> graveyard = new();

        public event Action StateChanged;

        public IReadOnlyList<CardData> DrawPile => drawPile;
        public IReadOnlyList<CardData> Hand => hand;
        public IReadOnlyList<CardData> Graveyard => graveyard;

        private void Start()
        {
            fieldManager?.Initialize(this, handManager);
            cardActionManager?.Initialize(this, handManager, trapZoneManager);
            trapZoneManager?.Initialize(cardActionManager);
            deckView?.Initialize(this);
            graveyardView?.Initialize(this);
            StartNewGame(drawOpeningHandOnStart);
        }

        public void StartNewGame()
        {
            StartNewGame(true);
        }

        public void StartNewGame(bool drawOpeningHand)
        {
            fieldManager?.ClearField();
            trapZoneManager?.ClearTraps();
            InitializeDeck();

            if (drawOpeningHand)
            {
                DrawOpeningHand();
            }
        }

        public void InitializeDeck()
        {
            drawPile.Clear();
            drawPile.AddRange(startingDeck);
            hand.Clear();
            graveyard.Clear();

            ShuffleDrawPile();
            handManager?.ClearHand();
            RefreshPileViews();
            NotifyStateChanged();
        }

        public void DrawOpeningHand()
        {
            DrawCards(openingHandSize);
        }

        public List<CardData> DrawCards(int count)
        {
            var drawnCards = new List<CardData>();
            if (count <= 0)
            {
                return drawnCards;
            }

            var cardsToDraw = Mathf.Min(count, drawPile.Count);

            for (var i = 0; i < cardsToDraw; i++)
            {
                var card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
                drawnCards.Add(card);
            }

            handManager?.ShowCards(hand);
            RefreshPileViews();
            NotifyStateChanged();
            return drawnCards;
        }

        public bool RemoveCardFromHand(CardData cardData)
        {
            if (cardData == null || !hand.Remove(cardData))
            {
                return false;
            }

            handManager?.ShowCards(hand);
            NotifyStateChanged();
            return true;
        }

        public bool UseSpellFromHand(CardData cardData)
        {
            if (cardData == null || cardData.CardType != CardType.Spell)
            {
                return false;
            }

            if (!RemoveCardFromHand(cardData))
            {
                return false;
            }

            graveyard.Add(cardData);
            RefreshPileViews();
            ResolveSpellEffect(cardData);
            return true;
        }

        private void RefreshPileViews()
        {
            deckView?.RefreshView();
            graveyardView?.RefreshView();
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void ResolveSpellEffect(CardData cardData)
        {
            if (cardData.CardName == DrawScrollCardName)
            {
                var drawnCards = DrawCards(DrawScrollAmount);
                GameLogManager.Log($"{cardData.CardName} used. Drew {drawnCards.Count} card(s).");
                return;
            }

            GameLogManager.Log($"{cardData.CardName} used. Effect is not implemented yet.");
        }

        public void ShuffleDrawPile()
        {
            for (var i = drawPile.Count - 1; i > 0; i--)
            {
                var randomIndex = UnityEngine.Random.Range(0, i + 1);
                (drawPile[i], drawPile[randomIndex]) = (drawPile[randomIndex], drawPile[i]);
            }
        }
    }
}
