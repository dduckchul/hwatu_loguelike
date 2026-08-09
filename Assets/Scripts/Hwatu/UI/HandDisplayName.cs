using System;
using Hwatu.Hands;

namespace Hwatu.UI
{
    public static class HandDisplayName
    {
        private const int TypeSetFontSizePercent = 70;

        public static string Get(HandResult hand)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            string handName;
            switch (hand.Type)
            {
                case HandType.Ggeut:
                    handName = hand.Ggeut + "끗";
                    break;
                case HandType.SeRyuk:
                    handName = "세륙";
                    break;
                case HandType.JangSa:
                    handName = "장사";
                    break;
                case HandType.JangBbing:
                    handName = "장삥";
                    break;
                case HandType.GuBbing:
                    handName = "구삥";
                    break;
                case HandType.DokSa:
                    handName = "독사";
                    break;
                case HandType.Ali:
                    handName = "알리";
                    break;
                case HandType.Ddang:
                    handName = hand.FirstCard.Definition.Month + "땡";
                    break;
                case HandType.BrightDdang:
                    int lowMonth = Math.Min(
                        hand.FirstCard.Definition.Month,
                        hand.SecondCard.Definition.Month);
                    int highMonth = Math.Max(
                        hand.FirstCard.Definition.Month,
                        hand.SecondCard.Definition.Month);
                    handName = $"{lowMonth}{highMonth}광땡";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return AppendTypeSetName(handName, hand.Tags);
        }

        private static string AppendTypeSetName(string handName, HandTag tags)
        {
            if ((tags & HandTag.RibbonSet) != 0)
            {
                return AppendTypeSetTag(handName, "쌍띠");
            }

            if ((tags & HandTag.AnimalSet) != 0)
            {
                return AppendTypeSetTag(handName, "쌍열끗");
            }

            return handName;
        }

        private static string AppendTypeSetTag(string handName, string tagName)
        {
            return $"{handName}\n<size={TypeSetFontSizePercent}%>+ {tagName}</size>";
        }
    }
}
