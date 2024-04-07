using UnityEngine;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	using Needs;
	public class Settings : ModSettings
	{
		private static bool showHiddenSettings;

		private Vector2 settingsScrollPos;

		public Settings()
		{
			Debug.Message("NeedBarOverflow_Settings constructor called");
		}

		public void DoWindowContents(Rect inRect)
		{
			Rect outRect = new Rect(0f, 0f, inRect.width, inRect.height);
			Rect rect = new Rect(0f, 0f, inRect.width - 20f, Mathf.Max(inRect.height, Utility.height));
			GUI.BeginGroup(inRect);
			Widgets.BeginScrollView(outRect, ref settingsScrollPos, rect);
			Listing_Standard listing_Standard = new Listing_Standard();
			Utility.height = 0f;
			listing_Standard.Begin(rect);
			Utility.LsGap(listing_Standard);
			listing_Standard.Label(Strings.RestartNReq.MyTranslate());


			if (Utility.AddSimpleSetting(listing_Standard, typeof(Need_Food)))
				Food.AddSettings(listing_Standard);
			if (Utility.AddSimpleSetting(listing_Standard, typeof(Need_Rest)))
				NeedSetting<Need_Rest>.AddSettings(listing_Standard);
			if (Utility.AddSimpleSetting(listing_Standard, typeof(Need_Joy)))
				NeedSetting<Need_Joy>.AddSettings(listing_Standard);
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Mood));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Beauty));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Comfort));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Chemical));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Chemical_Any));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Outdoors));
#if (!v1_2)
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Indoors));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Suppression));
#endif
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_RoomSize));
#if (!v1_2 && !v1_3)
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Deathrest));
			if (Utility.AddSimpleSetting(listing_Standard, typeof(Need_KillThirst)))
				NeedSetting<Need_KillThirst>.AddSettings(listing_Standard);
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_MechEnergy));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Learning));
			Utility.AddSimpleSetting(listing_Standard, typeof(Need_Play));
#endif
			Utility.LsGap(listing_Standard);
			SettingLabel settingLabel = new SettingLabel(string.Empty, Strings.ShowHiddenSettings);
			listing_Standard.CheckboxLabeled(settingLabel.TranslatedLabel(), ref showHiddenSettings, settingLabel.TranslatedTip());
			if (showHiddenSettings)
			{
				Utility.AddSimpleSetting(listing_Standard, typeof(Need));
				Utility.AddSimpleSetting(listing_Standard, typeof(Need_Authority));
#if (!v1_2)
				Utility.AddSimpleSetting(listing_Standard, typeof(Need_Sadism));
#endif
			}
			Utility.LsGap(listing_Standard);
			listing_Standard.End();
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		public override void ExposeData()
		{
			Debug.Message("ExposeData() called");
			base.ExposeData();
			Common common = new Common();
			Food food = new Food();
			NeedSetting<Need_Rest> rest = new NeedSetting<Need_Rest>();
			NeedSetting<Need_Joy> joy = new NeedSetting<Need_Joy>();
#if (!v1_2 && !v1_3)
			NeedSetting<Need_KillThirst> killThirst = new NeedSetting<Need_KillThirst>();
#endif
			Scribe_Deep.Look(ref common, nameof(Common));
			Scribe_Deep.Look(ref food, nameof(Need_Food));
			Scribe_Deep.Look(ref rest, nameof(Need_Rest));
			Scribe_Deep.Look(ref joy, nameof(Need_Joy));
#if (!v1_2 && !v1_3)
			Scribe_Deep.Look(ref killThirst, nameof(Need_KillThirst));
#endif
			if (Refs.initialized && 
				(Scribe.mode == LoadSaveMode.PostLoadInit || 
				Scribe.mode == LoadSaveMode.Saving))
				Patches.PatchApplier.ApplyPatches();
		}
	}
}
