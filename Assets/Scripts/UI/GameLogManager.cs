using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace CardBattle.UI
{
    public class GameLogManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;
        [SerializeField, Min(1)] private int maxEntries = 8;

        private static GameLogManager instance;

        private readonly Queue<string> entries = new();
        private readonly StringBuilder logBuilder = new();

        private void Awake()
        {
            instance = this;
            RefreshView();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Debug.Log(message);
            instance?.AddEntry(message);
        }

        public static void ClearLog()
        {
            instance?.Clear();
        }

        public void AddEntry(string message)
        {
            entries.Enqueue(message);

            while (entries.Count > maxEntries)
            {
                entries.Dequeue();
            }

            RefreshView();
        }

        public void Clear()
        {
            entries.Clear();
            RefreshView();
        }

        private void RefreshView()
        {
            if (logText == null)
            {
                return;
            }

            if (entries.Count == 0)
            {
                logText.text = "Log";
                return;
            }

            logBuilder.Clear();
            foreach (var entry in entries)
            {
                logBuilder.Append("- ");
                logBuilder.AppendLine(entry);
            }

            logText.text = logBuilder.ToString();
        }
    }
}
