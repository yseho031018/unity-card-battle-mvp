using CardBattle.Cards;
using UnityEditor;
using UnityEngine;

namespace CardBattle.Editor
{
    public static class TestCardDataCreator
    {
        private const string CardFolder = "Assets/CardAssets";
        private const string TestCardFolder = CardFolder + "/TestCards";

        [MenuItem("Tools/Create Test Card Data")]
        public static void CreateTestCardData()
        {
            EnsureFolder("Assets", "CardAssets");
            EnsureFolder(CardFolder, "TestCards");

            CreateOrUpdateCard("Iron Knight", CardType.Monster, 0, 4, 1600, 1200, "A reliable front-line warrior.");
            CreateOrUpdateCard("Fire Dragon", CardType.Monster, 0, 6, 2400, 1800, "A high-level dragon with fierce attack power.");
            CreateOrUpdateCard("Stone Golem", CardType.Monster, 0, 5, 1000, 2500, "A defensive guardian made of ancient stone.");
            CreateOrUpdateCard("Wind Falcon", CardType.Monster, 0, 3, 1400, 1000, "A swift monster that rules the sky.");
            CreateOrUpdateCard("Shadow Assassin", CardType.Monster, 0, 4, 1800, 800, "A fragile attacker that strikes from darkness.");

            CreateOrUpdateCard("Power Boost", CardType.Spell, 1, 0, 0, 0, "Target monster gains +500 ATK. Effect is not implemented yet.");
            CreateOrUpdateCard("Healing Light", CardType.Spell, 1, 0, 0, 0, "Recover 1000 life. Effect is not implemented yet.");
            CreateOrUpdateCard("Draw Scroll", CardType.Spell, 1, 0, 0, 0, "Draw 2 cards. Effect is not implemented yet.");

            CreateOrUpdateCard("Mirror Shield", CardType.Trap, 1, 0, 0, 0, "Negate an attack. Effect is not implemented yet.");
            CreateOrUpdateCard("Pitfall Trap", CardType.Trap, 1, 0, 0, 0, "Destroy an attacking monster. Effect is not implemented yet.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created or updated test cards in {TestCardFolder}.");
        }

        private static void CreateOrUpdateCard(
            string cardName,
            CardType cardType,
            int cost,
            int level,
            int attack,
            int defense,
            string description)
        {
            var path = FindExistingCardPath(cardName) ?? $"{TestCardFolder}/{cardName}.asset";
            var cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (cardData == null)
            {
                cardData = ScriptableObject.CreateInstance<CardData>();
                AssetDatabase.CreateAsset(cardData, path);
            }

            var serializedObject = new SerializedObject(cardData);
            serializedObject.FindProperty("cardName").stringValue = cardName;
            serializedObject.FindProperty("cardType").enumValueIndex = (int)cardType;
            serializedObject.FindProperty("cost").intValue = cost;
            serializedObject.FindProperty("level").intValue = level;
            serializedObject.FindProperty("attack").intValue = attack;
            serializedObject.FindProperty("defense").intValue = defense;
            serializedObject.FindProperty("description").stringValue = description;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(cardData);
        }

        private static string FindExistingCardPath(string cardName)
        {
            var guids = AssetDatabase.FindAssets("t:CardData");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (cardData != null && (cardData.name == cardName || cardData.CardName == cardName))
                {
                    return path;
                }
            }

            return null;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
