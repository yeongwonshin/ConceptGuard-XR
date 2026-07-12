using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ConceptGuardXR
{
    public static class WorldTextFactory
    {
        private static Font cachedFont;

        public static TextMesh Create(
            Transform parent,
            string objectName,
            string content,
            Vector3 localPosition,
            float characterSize,
            int fontSize,
            Color color,
            TextAnchor anchor = TextAnchor.UpperLeft,
            TextAlignment alignment = TextAlignment.Left)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            var text = textObject.AddComponent<TextMesh>();
            text.text = content;
            text.characterSize = characterSize;
            text.fontSize = fontSize;
            text.anchor = anchor;
            text.alignment = alignment;
            text.color = color;
            text.richText = true;
            text.font = ResolveFont();
            var renderer = text.GetComponent<MeshRenderer>();
            if (text.font != null && renderer != null)
            {
                renderer.sharedMaterial = text.font.material;
            }
            return text;
        }

        public static string Wrap(string text, int maxCharactersPerLine)
        {
            if (string.IsNullOrEmpty(text) || maxCharactersPerLine < 4)
            {
                return text ?? string.Empty;
            }

            var result = new StringBuilder();
            foreach (var paragraph in text.Replace("\r", string.Empty).Split('\n'))
            {
                if (paragraph.Length <= maxCharactersPerLine)
                {
                    if (result.Length > 0)
                    {
                        result.Append('\n');
                    }
                    result.Append(paragraph);
                    continue;
                }

                var words = new List<string>(paragraph.Split(' '));
                if (words.Count == 1)
                {
                    for (var index = 0; index < paragraph.Length; index += maxCharactersPerLine)
                    {
                        if (result.Length > 0)
                        {
                            result.Append('\n');
                        }
                        result.Append(paragraph.Substring(index, Mathf.Min(maxCharactersPerLine, paragraph.Length - index)));
                    }
                    continue;
                }

                var currentLength = 0;
                foreach (var word in words)
                {
                    var required = currentLength == 0 ? word.Length : word.Length + 1;
                    if (currentLength > 0 && currentLength + required > maxCharactersPerLine)
                    {
                        result.Append('\n');
                        currentLength = 0;
                    }
                    else if (currentLength > 0)
                    {
                        result.Append(' ');
                        currentLength += 1;
                    }

                    result.Append(word);
                    currentLength += word.Length;
                }
            }

            return result.ToString();
        }

        private static Font ResolveFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            var candidates = new[]
            {
                "Noto Sans CJK KR",
                "Noto Sans KR",
                "Malgun Gothic",
                "Apple SD Gothic Neo",
                "Arial Unicode MS",
                "Arial"
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var font = Font.CreateDynamicFontFromOSFont(candidate, 48);
                    if (font != null)
                    {
                        cachedFont = font;
                        return cachedFont;
                    }
                }
                catch
                {
                    // Continue to the next installed system font.
                }
            }

            try
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return cachedFont;
        }
    }
}
