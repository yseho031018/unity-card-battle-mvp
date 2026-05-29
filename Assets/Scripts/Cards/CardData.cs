using UnityEngine;

namespace CardBattle.Cards
{
    [CreateAssetMenu(fileName = "새 카드", menuName = "카드 배틀/카드 데이터")]
    public class CardData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string cardName;
        [SerializeField] private CardType cardType;

        [Header("비용")]
        [SerializeField, Min(0)] private int cost;
        [SerializeField, Min(0)] private int level;

        [Header("몬스터 능력치")]
        [SerializeField, Min(0)] private int attack;
        [SerializeField, Min(0)] private int defense;

        [Header("일러스트")]
        [SerializeField] private Sprite artwork;

        [Header("설명")]
        [SerializeField, TextArea(3, 8)] private string description;

        public string CardName => string.IsNullOrWhiteSpace(cardName) ? name : cardName;
        public CardType CardType => cardType;
        public int Cost => cost;
        public int Level => level;
        public int Attack => attack;
        public int Defense => defense;
        public Sprite Artwork => artwork;
        public string Description => description;
    }
}
