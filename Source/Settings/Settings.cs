using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static NeedBarOverflow.Needs.Utility;

namespace NeedBarOverflow
{
	public class Settings : ModSettings
	{
		private static bool showHiddenSettings;
		internal static int migrateSettings;
		private Vector2 settingsScrollPos;

		public Settings() => Debug.Message("NeedBarOverflow_Settings constructor called");

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
			ls.Label(Strings.RestartNReq.Translate());
			Setting_Common.AddSettings(ls);
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
			Debug.Message("ExposeData() called");
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
			// Old settings have issues
			//   so we call this method to copy old settings to new
			// This call will be removed for 1.6
			if (Scribe.mode == LoadSaveMode.LoadingVars &&
				migrateSettings == 1)
			{
				MigrateSettings();
			}

			if (NeedBarOverflow.Initialized &&
				(Scribe.mode == LoadSaveMode.PostLoadInit ||
				Scribe.mode == LoadSaveMode.Saving))
			{
				Patches.PatchApplier.ApplyPatches();
			}
		}

		// Old settings have issues (see comments under methods below)
		//   so we had this method to copy old settings to new
		// These migration method will be removed for 1.6
		private static void MigrateSettings()
		{
			Debug.Message("MigrateSettings() called");
			Dictionary<IntVec2, bool> enabledB = [];
			Dictionary<IntVec2, float> statsB = [];
			Scribe_Collections.Look(ref enabledB, nameof(enabledB), LookMode.Value, LookMode.Value);
			Scribe_Collections.Look(ref statsB, nameof(statsB), LookMode.Value, LookMode.Value);
			enabledB ??= [];
			statsB ??= [];
			Setting_Common.MigrateSettings(enabledB);
			Setting_Food.MigrateSettings(enabledB, statsB);
			OverflowStats<Need_Rest>.MigrateSettings(enabledB, statsB, 1);
			OverflowStats<Need_Joy>.MigrateSettings(enabledB, statsB, 2);
#if g1_4
			OverflowStats<Need_KillThirst>.MigrateSettings(enabledB, statsB, 16);
#endif
			migrateSettings = 2;
		}
	}
}
