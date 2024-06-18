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
				string? name = null, string? tip = null, bool showAsPerc = false)
		{
			string numString = num.CustomToString(showAsPerc, true);
			string txt_min_str = txt_min.CustomToString(showAsPerc, true);
			string txt_max_str = txt_max.CustomToString(showAsPerc, true);
			if (name is not null && name.Length != 0)
			{
				string labeltxt = name.MyTranslate(numString);
				if (tip is not null && tip.Length != 0)
				{
					TooltipHandler.TipRegion(new Rect(
						0f, ls.CurHeight, ls.ColumnWidth,
						Text.LineHeight * 1.2f + Text.CalcHeight(labeltxt, ls.ColumnWidth)),
						tip.MyTranslate(numString));
				}

				ls.Label(labeltxt);
			}
			else
			{
				if (tip is not null && tip.Length != 0)
				{
					TooltipHandler.TipRegion(new Rect(
						0, ls.CurHeight, ls.ColumnWidth,
						Text.LineHeight * 1.2f),
						tip.MyTranslate(numString));
				}

				ls.Gap(ls.verticalSpacing * 1.5f);
			}
			float mul = showAsPerc ? 100f : 1f;
			Rect rectNum = new(
				ls.ColumnWidth * 0.88f, ls.CurHeight,
				ls.ColumnWidth * 0.12f, Text.LineHeight);
			Rect rectSlider = new(
				0, ls.CurHeight + Text.LineHeight * 0.2f,
				ls.ColumnWidth * 0.85f, Text.LineHeight);
			num = AddTextFieldNumeric(rectNum, num, txt_min, txt_max, showAsPerc);
			if (logSlider)
			{
				float num_pow;
				if (num <= txt_min)
					num_pow = slider_min;
				else if (num >= txt_max)
					num_pow = slider_max;
				else
					num_pow = Mathf.Log10(num);
#if v1_2 || v1_3
				// Obsolete as of 1.4
				num_pow = Widgets.HorizontalSlider(
					rectSlider, num_pow,
					slider_min, slider_max);
#elif v1_4
				// Temporary for 1.4
				// The nuget package alerts an error for this
				// But if this is the only "error" it compiles correctly...
				num_pow = Widgets.HorizontalSlider_NewTemp(
					rectSlider, num_pow,
					slider_min, slider_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str);
#else
				// For 1.5
				num_pow = Widgets.HorizontalSlider(
					rectSlider, num_pow,
					slider_min, slider_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str);
#endif
				if (num_pow <= slider_min)
					num = txt_min;
				else if (num_pow == slider_max)
					num = txt_max;
				else
					num = Mathf.Pow(10f, num_pow).RoundToSigFig();
			}
			else
			{
#if v1_2 || v1_3
				// Obsolete as of 1.4
				num = Widgets.HorizontalSlider(
					rectSlider, num,
					txt_min, txt_max
					).RoundToSigFig();
#elif v1_4
				// Temporary for 1.4
				// The nuget package alerts an error for this
				// But if this is the only "error" it compiles correctly...
				num = Widgets.HorizontalSlider_NewTemp(
					rectSlider, num, txt_min, txt_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str
					).RoundToSigFig();
#else
				// For 1.5
				num = Widgets.HorizontalSlider(
					rectSlider, num, txt_min, txt_max,
					leftAlignedLabel: txt_min_str,
					rightAlignedLabel: txt_max_str,
					roundTo: num.SigFigScale());
#endif
			}
			ls.Gap(ls.verticalSpacing * 1.5f + Text.LineHeight);
		}

		private static float AddTextFieldNumeric(Rect rectNum, float num, float txt_min, float txt_max, bool showAsPerc)
		{
			float mult = showAsPerc ? 100f : 1f;
			float invMult = showAsPerc ? 0.01f : 1f;
			float val = num * mult;
			string buffer = num.CustomToString(showAsPerc, false);
			Widgets.TextFieldNumeric(rectNum, ref val, ref buffer, txt_min * mult, txt_max * mult);
			return val * invMult;
		}

		public static bool AddSimpleSetting(Listing_Standard ls, Type needType)
		{
			LsGap(ls);
			float f1 = Setting_Common.GetOverflow(needType);
			bool b1 = f1 >= 0f;
			f1 = b1 ? f1 : -f1 - 1f;
			string numString = f1.CustomToString(true, true);
			SettingLabel sl = new(needType.Name,
				b1 ? Strings.OverfPerc : Strings.OverfEnabled);
			ls.CheckboxLabeled(sl.TranslatedLabel(numString), ref b1, sl.TranslatedTip(numString));
			ls.Gap(ls.verticalSpacing * -0.5f);
			if (b1)
			{
				AddNumSetting(ls, ref f1, true,
					0f, 2.002f, 1f, float.PositiveInfinity, null, sl.tip, true);
			}
			Setting_Common.SetOverflow(needType, b1 ? f1 : -f1 - 1f);
			return b1;
		}
	}
}
