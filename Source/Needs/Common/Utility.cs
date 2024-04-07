using System;
using UnityEngine;
using Verse;

namespace NeedBarOverflow.Needs
{
    public static class Utility
    {
        public static float height = 100000f;
        public static void LsGap(Listing_Standard ls)
        {
            ls.GapLine();
            if (ls.CurHeight < height)
                height = 100000f;
            else
                height = ls.CurHeight;
        }

        public static void AddNumSetting(Listing_Standard ls, 
            ref float num, SettingLabel settingLabel, bool logSlider = true, 
            float slider_min = -2.002f, float slider_max = 2.002f, 
            float txt_min = 0f, float txt_max = float.PositiveInfinity, 
            bool showAsPerc = false)
            => AddNumSetting(ls, ref num, logSlider, 
                slider_min, slider_max, txt_min, txt_max, 
                settingLabel.label, settingLabel.tip, showAsPerc);

        public static void AddNumSetting(Listing_Standard ls,
                ref float num, bool logSlider = true,
                float slider_min = -2.002f, float slider_max = 2.002f,
                float txt_min = 0f, float txt_max = float.PositiveInfinity,
                string name = null, string tip = null, bool showAsPerc = false)
        {
            string numString = num.CustomToString(showAsPerc, translate: true);
            string txt_min_str = txt_min.CustomToString(showAsPerc, translate: true);
            string txt_max_str = txt_max.CustomToString(showAsPerc, translate: true);
            if (!name.NullOrEmpty())
            {
                string labeltxt = name.MyTranslate(numString);
                if (!tip.NullOrEmpty())
                    TooltipHandler.TipRegion(new Rect(
                        0f, ls.CurHeight, ls.ColumnWidth, 
                        Text.LineHeight * 1.2f + Text.CalcHeight(labeltxt, ls.ColumnWidth)), 
                        tip.MyTranslate(numString));
                ls.Label(labeltxt);
            }
            else
            {
                if (!tip.NullOrEmpty())
                    TooltipHandler.TipRegion(new Rect(
                        0, ls.CurHeight, ls.ColumnWidth,
                        Text.LineHeight * 1.2f),
                        tip.Translate(numString));
                ls.Gap(ls.verticalSpacing * 1.5f);
            }
            float mul = (showAsPerc ? 100f : 1f);
            Rect rectNum = new Rect(
                ls.ColumnWidth * 0.88f, ls.CurHeight,
                ls.ColumnWidth * 0.12f, Text.LineHeight);
            Rect rectSlider = new Rect(
                0, ls.CurHeight + Text.LineHeight * 0.2f,
                ls.ColumnWidth * 0.85f, Text.LineHeight);
            num = AddTextFieldNumeric(rectNum, num, txt_min, txt_max, showAsPerc);
            if (logSlider)
            {
                float num_pow;
                if (num == txt_min)
                    num_pow = slider_min;
                else if (num == txt_max)
                    num_pow = slider_max;
                else
                    num_pow = Mathf.Log10(num);
#if (v1_2 || v1_3)
			    // Obsolete as of 1.4
			    num_pow = Widgets.HorizontalSlider(
				    rectSlider, (float)num_pow, 
				    slider_min, slider_max);
#elif (v1_4)
			    // Temporary for 1.4
			    // The nuget package alerts an error for this
			    // But if this is the only "error" it compiles correctly...
			    num_pow = Widgets.HorizontalSlider_NewTemp(
				    rectSlider, (float)num_pow, 
				    slider_min, slider_max, 
				    leftAlignedLabel: txt_min_str, 
				    rightAlignedLabel: txt_max_str);
#else
                // For 1.5
                num_pow = Widgets.HorizontalSlider(
                    rectSlider, (float)num_pow,
                    slider_min, slider_max,
                    leftAlignedLabel: txt_min_str,
                    rightAlignedLabel: txt_max_str);
#endif
                if (num_pow == slider_min)
                    num = txt_min;
                else if (num_pow == slider_max)
                    num = txt_max;
                else
                    num = Mathf.Pow(10f, num_pow).CustomRound();
            }
            else
            {
#if (v1_2 || v1_3)
			    // Obsolete as of 1.4
			    num = Widgets.HorizontalSlider(
				    rectSlider, num, 
				    txt_min, txt_max
				    ).CustomRound();
#elif (v1_4)
			    // Temporary for 1.4
			    // The nuget package alerts an error for this
			    // But if this is the only "error" it compiles correctly...
			    num = Widgets.HorizontalSlider_NewTemp(
				    rectSlider, num, txt_min, txt_max, 
				    leftAlignedLabel: txt_min_str, 
				    rightAlignedLabel: txt_max_str
				    ).CustomRound();
#else
                // For 1.5
                num = Widgets.HorizontalSlider(
                    rectSlider, num, txt_min, txt_max,
                    leftAlignedLabel: txt_min_str,
                    rightAlignedLabel: txt_max_str
                    ).CustomRound();
#endif
            }
            ls.Gap(ls.verticalSpacing * 1.5f + Text.LineHeight);
        }

        private static float AddTextFieldNumeric(Rect rectNum, float num, float txt_min, float txt_max, bool showAsPerc)
        {
            float num2 = (showAsPerc ? 100f : 1f);
            float val = num * num2;
            string buffer = val.CustomToString(showAsPerc: false, translate: false);
            Widgets.TextFieldNumeric(rectNum, ref val, ref buffer, txt_min * num2, txt_max * num2);
            if (Mathf.Abs(val - num * num2) >= 0.01f)
                num = val / num2;
            return Mathf.Clamp(num, txt_min, txt_max);
        }

        public static bool AddSimpleSetting(Listing_Standard ls, Type needType)
        {
            LsGap(ls);
            float num = Common.overflow[needType];
            bool checkOn = num > 0f;
            num = (checkOn ? num : (0f - num));
            SettingLabel settingLabel = new SettingLabel(needType.Name, Strings.OverfEnabled);
            ls.CheckboxLabeled(settingLabel.TranslatedLabel(), ref checkOn, settingLabel.TranslatedTip());
            ls.Gap(ls.verticalSpacing * -0.5f);
            if (checkOn)
            {
                settingLabel = new SettingLabel(needType.Name, Strings.OverfPerc);
                AddNumSetting(ls, ref num, settingLabel, logSlider: true, 0f, 2.002f, 1f, float.PositiveInfinity, showAsPerc: true);
            }
            Common.overflow[needType] = (checkOn ? num : (0f - num));
            return checkOn;
        }
    }
}