using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static NeedBarOverflow.Utility;

namespace NeedBarOverflow
{
	public class Settings : ModSettings
	{
		public static bool showHiddenSettings;
		public Vector2 settingsScrollPos;

		public Settings()
			=> Debug.Message("NeedBarOverflow_Settings constructor called with Scribe.mode == " + Scribe.mode);

		public void DoWindowContents(Rect inRect)
		{
			Rect outRect = new(0f, 0f, inRect.width, inRect.height);
			Rect rect = new(0f, 0f, inRect.width - 20f, Mathf.Max(inRect.height, height));
			GUI.BeginGroup(inRect);
			Widgets.BeginScrollView(outRect, ref settingsScrollPos, rect);
			Listing_Standard ls = new();
			height = 0f;
			ls.Begin(rect);
			LsGap(ls);
			ls.Label(Strings.RestartNReq_Tip.Translate());
			DisablingDefs.AddSettings(ls);
			if (AddSimpleSetting(ls, typeof(Need_Food)))
				Setting_Food.AddSettings(ls);
			if (AddSimpleSetting(ls, typeof(Need_Rest)))
				OverflowStats<Need_Rest>.AddSettings(ls);
			if (AddSimpleSetting(ls, typeof(Need_Joy)))
				OverflowStats<Need_Joy>.AddSettings(ls);
			AddSimpleSetting(ls, typeof(Need_Mood));
			AddSimpleSetting(ls, typeof(Need_Beauty));
			AddSimpleSetting(ls, typeof(Need_Comfort));
			AddSimpleSetting(ls, typeof(Need_Chemical));
			AddSimpleSetting(ls, typeof(Need_Chemical_Any));
			AddSimpleSetting(ls, typeof(Need_Outdoors));
#if g1_3
			AddSimpleSetting(ls, typeof(Need_Indoors));
			AddSimpleSetting(ls, typeof(Need_Suppression));
#endif
			AddSimpleSetting(ls, typeof(Need_RoomSize));
#if g1_4
			AddSimpleSetting(ls, typeof(Need_Deathrest));
			if (AddSimpleSetting(ls, typeof(Need_KillThirst)))
				OverflowStats<Need_KillThirst>.AddSettings(ls);
			AddSimpleSetting(ls, typeof(Need_MechEnergy));
			AddSimpleSetting(ls, typeof(Need_Learning));
			AddSimpleSetting(ls, typeof(Need_Play));
#endif
			LsGap(ls);
			SettingLabel settingLabel = new(string.Empty, Strings.ShowHiddenSettings);
			ls.CheckboxLabeled(settingLabel.TranslatedLabel(), ref showHiddenSettings, settingLabel.TranslatedTip());
			if (showHiddenSettings)
			{
				AddSimpleSetting(ls, typeof(Need));
				AddSimpleSetting(ls, typeof(Need_Authority));
#if g1_3
				AddSimpleSetting(ls, typeof(Need_Sadism));
#endif
			}
			LsGap(ls);
			ls.End();
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		public override void ExposeData()
		{
			Debug.Message("Settings.ExposeData() called with Scribe.mode == " + Scribe.mode);
			base.ExposeData();
			Setting_Common common = new();
			Setting_Food food = new();
			OverflowStats<Need_Rest> rest = new();
			OverflowStats<Need_Joy> joy = new();
#if g1_4
			OverflowStats<Need_KillThirst> killThirst = new();
#endif
			Scribe_Deep.Look(ref common, nameof(Setting_Common));
			Scribe_Deep.Look(ref food, nameof(Need_Food));
			Scribe_Deep.Look(ref rest, nameof(Need_Rest));
			Scribe_Deep.Look(ref joy, nameof(Need_Joy));
#if g1_4
			Scribe_Deep.Look(ref killThirst, nameof(Need_KillThirst));
#endif

			if (Scribe.mode == LoadSaveMode.PostLoadInit ||
				Scribe.mode == LoadSaveMode.Saving)
				Patches.PatchApplier.ApplyPatches();
		}
	}
}
