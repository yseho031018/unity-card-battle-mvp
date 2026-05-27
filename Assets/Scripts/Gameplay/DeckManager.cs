using System.Collections.Generic;
using CardBattle.Cards;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Gameplay
{
    public class DeckManager : MonoBehaviour
    {
        [SerializeField] private List<CardData> startingDeck = new();
        [SerializeField] private HandManager handManager;
        [SerializeField] private FieldManager fieldManager;
        [SerializeField] private TrapZoneManager trapZoneManager;
        [SerializeField] private CardActionManager cardActionManager;
        [SerializeField, Min(0)] private int openingHandSize = 5;
        [SerializeField] private bool drawOpeningHandOnStart = true;

        private readonly List<CardData> drawPile = new();
        private readonly List<CardData> hand = new();
        private readonly List<CardData> graveyard = new();

        public IReadOnlyList<CardData> DrawPile => drawPile;
        public IReadOnlyList<CardData> Hand => hand;
        public IReadOnlyList<CardData> Graveyard => graveyard;

        private void Start()
        {
            fieldManager?.Initialize(this, handManager);
            trapZoneManager?.Initialize();
            cardActionManager?.Initialize(this, handManager, trapZoneManager);
            InitializeDeck();

            if (drawOpeningHandOnStart)
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
        }

        public void DrawOpeningHand()
        {
            DrawCards(openingHandSize);
        }

        public List<CardData> DrawCards(int count)
        {
            var drawnCards = new List<CardData>();
            var cardsToDraw = Mathf.Min(count, drawPile.Count);

            for (var i = 0; i < cardsToDraw; i++)
            {
                var card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
                drawnCards.Add(card);
            }

            handManager?.ShowCards(hand);
            return drawnCards;
        }

        public bool RemoveCardFromHand(CardData cardData)
        {
            if (cardData == null || !hand.Remove(cardData))
            {
                return false;
            }

            handManager?.ShowCards(hand);
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
            Debug.Log($"{cardData.CardName} used. Effect is not implemented yet. Sent to graveyard list.");
            return true;
        }

        public void ShuffleDrawPile()
        {
            for (var i = drawPile.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (drawPile[i], drawPile[randomIndex]) = (drawPile[randomIndex], drawPile[i]);
            }
        }
    }
}
