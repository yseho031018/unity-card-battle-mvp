using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    public static class CardBackVisual
    {
        public const string RootName = "CardBackArtRoot";

        private static readonly Color BackColor = new(0.04f, 0.07f, 0.12f, 0.97f);
        private static readonly Color InnerColor = new(0.09f, 0.14f, 0.21f, 0.96f);
        private static readonly Color AccentColor = new(0.58f, 0.46f, 0.24f, 0.9f);
        private static readonly Color LineColor = new(0.78f, 0.68f, 0.45f, 0.74f);

        public static GameObject Ensure(Transform parent, float margin = 12f)
        {
            if (parent == null)
            {
                return null;
            }

            var root = parent.Find(RootName)?.gameObject ?? CreateImageObject(RootName, parent, BackColor).gameObject;
            root.transform.SetSiblingIndex(0);
            ConfigureStretch(root.GetComponent<RectTransform>(), margin);

            var rootImage = root.GetComponent<Image>();
            rootImage.color = BackColor;
            rootImage.raycastTarget = false;

            var rootOutline = EnsureOutline(root);
            rootOutline.effectColor = new Color(0.82f, 0.86f, 0.9f, 0.62f);
            rootOutline.effectDistance = new Vector2(2f, -2f);
            rootOutline.useGraphicAlpha = false;

            RemoveLegacyChildren(root.transform);

            var inner = EnsureImage(root.transform, "BackInnerPanel", InnerColor);
            ConfigureStretch(inner.rectTransform, 16f);
            inner.transform.SetSiblingIndex(0);

            var border = EnsureImage(root.transform, "BackInnerBorder", Color.clear);
            ConfigureStretch(border.rectTransform, 26f);
            border.transform.SetSiblingIndex(1);
            var borderOutline = EnsureOutline(border.gameObject);
            borderOutline.effectColor = LineColor;
            borderOutline.effectDistance = new Vector2(1f, -1f);
            borderOutline.useGraphicAlpha = false;

            var band = EnsureImage(root.transform, "BackCenterBand", AccentColor);
            ConfigureBand(band.rectTransform);
            band.transform.SetSiblingIndex(2);

            var mark = EnsureImage(root.transform, "BackCenterMark", LineColor);
            ConfigureCentered(mark.rectTransform, new Vector2(42f, 42f), 45f);
            mark.transform.SetSiblingIndex(3);

            root.SetActive(true);
            return root;
        }

        public static void SetVisible(Transform parent, bool visible)
        {
            var root = parent != null ? parent.Find(RootName) : null;
            if (root != null)
            {
                root.gameObject.SetActive(visible);
            }
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            var image = parent.Find(name)?.GetComponent<Image>();
            if (image == null)
            {
                image = CreateImageObject(name, parent, color);
            }

            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateImageObject(string name, Transform parent, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Outline EnsureOutline(GameObject target)
        {
            var outline = target.GetComponent<Outline>();
            if (outline == null)
            {
                outline = target.AddComponent<Outline>();
            }

            return outline;
        }

        private static void ConfigureStretch(RectTransform rectTransform, float margin)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(margin, margin);
            rectTransform.offsetMax = new Vector2(-margin, -margin);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureBand(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(-58f, 30f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private static void ConfigureCentered(RectTransform rectTransform, Vector2 size, float zRotation)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
            rectTransform.localScale = Vector3.one;
        }

        private static void RemoveLegacyChildren(Transform root)
        {
            RemoveChild(root, "BackThinBorder");
            RemoveChild(root, "BackCenterPanel");
            RemoveChild(root, "BackDiamond");
            RemoveChild(root, "BackDiamondCore");
        }

        private static void RemoveChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(child.gameObject);
            }
            else
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}
