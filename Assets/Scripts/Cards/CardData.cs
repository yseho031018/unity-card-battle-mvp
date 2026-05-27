using UnityEngine;

namespace CardBattle.Cards
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card Battle/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string cardName;
        [SerializeField] private CardType cardType;

        [Header("Costs")]
        [SerializeField, Min(0)] private int cost;
        [SerializeField, Min(0)] private int level;

        [Header("Monster Stats")]
        [SerializeField, Min(0)] private int attack;
        [SerializeField, Min(0)] private int defense;

        [Header("Text")]
        [SerializeField, TextArea(3, 8)] private string description;

        public string CardName => string.IsNullOrWhiteSpace(cardName) ? name : cardName;
        public CardType CardType => cardType;
        public int Cost => cost;
        public int Level => level;
        public int Attack => attack;
        public int Defense => defense;
        public string Description => description;
    }
}
