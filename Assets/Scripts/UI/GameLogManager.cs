using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public class GameLogManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;
        [SerializeField] private Text legacyLogText;
        [SerializeField, Min(1)] private int maxEntries = 5;
        [SerializeField, Min(0f)] private float duplicateSuppressSeconds = 0.25f;

        private static GameLogManager instance;

        private readonly Queue<string> entries = new();
        private readonly StringBuilder logBuilder = new();
        private string lastMessage;
        private float lastMessageTime = -1f;

        private void Awake()
        {
            instance = this;
            ConfigureLegacyLogText();
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

            if (instance != null && !instance.CanAddEntry(message))
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
            lastMessage = message;
            lastMessageTime = Time.unscaledTime;

            while (entries.Count > maxEntries)
            {
                entries.Dequeue();
            }

            RefreshView();
        }

        public void Clear()
        {
            entries.Clear();
            lastMessage = null;
            lastMessageTime = -1f;
            RefreshView();
        }

        private bool CanAddEntry(string message)
        {
            if (string.IsNullOrEmpty(lastMessage) || duplicateSuppressSeconds <= 0f)
            {
                return true;
            }

            return lastMessage != message || Time.unscaledTime - lastMessageTime > duplicateSuppressSeconds;
        }

        private void RefreshView()
        {
            if (logText == null && legacyLogText == null)
            {
                return;
            }

            ConfigureLegacyLogText();

            if (entries.Count == 0)
            {
                SetLogText("Log");
                return;
            }

            logBuilder.Clear();
            foreach (var entry in entries)
            {
                logBuilder.Append("- ");
                logBuilder.AppendLine(entry);
            }

            SetLogText(logBuilder.ToString());
        }

        private void SetLogText(string value)
        {
            if (legacyLogText != null)
            {
                legacyLogText.text = value;
                return;
            }

            if (logText != null)
            {
                logText.text = value;
            }
        }

        private void ConfigureLegacyLogText()
        {
            if (legacyLogText == null)
            {
                return;
            }

            var malgunFont = Font.CreateDynamicFontFromOSFont("Malgun Gothic", legacyLogText.fontSize);
            if (malgunFont != null)
            {
                legacyLogText.font = malgunFont;
            }

            legacyLogText.supportRichText = false;
            legacyLogText.resizeTextForBestFit = false;
        }
    }
}
