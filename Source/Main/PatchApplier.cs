using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using NeedBarOverflow.Patches.ModCompat;
using Verse;

namespace NeedBarOverflow.Patches
{
	public static class PatchApplier
	{
		public static Settings? s;
		public static List<Patch_Single> Patches { get; }
		static PatchApplier()
		{
			Debug.WatchStart("static PatchApplier()");
			IEnumerable<Patch_Single> patchesCandidate = GenTypes
				.AllTypes
				.AsParallel()
				.Where(type =>
				!type.IsAbstract &&
				type.IsSubclassOf(typeof(Patch_Single)))
				.Select(type => Activator.CreateInstance(type))
				.Cast<Patch_Single>()
				.Where(patch => patch.Original is not null);
			HashSet<Patch_Single> patchesSet = [];
			foreach (Patch_Single patch in patchesCandidate)
			{
				if (patchesSet.Add(patch))
				{
					Debug.Warning("Adding Patch " + patch.Original?.Name);
					continue;
				}
				patchesSet.TryGetValue(patch, out Patch_Single existing);
				Debug.Error(string.Concat(
					"Multiple patches to method ",
					patch.Original?.Name, ": [",
					patch.GetType().Name, "], [",
					existing.GetType().Name, "]."));
			}
			Patches = [.. patchesSet];
			Debug.WatchLog("static PatchApplier()");
		}

		public static void ApplyPatches()
		{
			if (s is null)
				return;
			Debug.WatchStart("Applying static Patches...");
			//General Patches
			SavedGameLoaderNow_.LoadGameFromSaveFileNow.Toggle();
			GenUI_.BottomPart.Toggle();
			GenUI_.DrawStatusLevel.Toggle();
			Need_.CurLevel.Toggle();
			Need_.DrawOnGUI.Toggle();
			//BeautyUtility_.AverageBeautyPerceptible.Toggle();
			//Setting_Food
			Need_Food_.NutritionWanted.Toggle();
			//FloatMenuMakerMap_.AddHumanlikeOrders.Toggle();
			//FoodUtility_.WillIngestFromInventoryNow.Toggle();
			Need_Food_.NeedInterval.Toggle();
			TraitSet_.GainTrait.Toggle();
			//Rest
			Need_Rest_.NeedInterval.Toggle();
			//Scribe_Joy
			Need_Joy_.GainJoy.Toggle();
			Need_Joy_.NeedInterval_Drain.Toggle();
			Need_Joy_.GainJoy_Gain.Toggle();
			//Mood
			Need_Mood_.CurInstantLevel.Toggle();
			//ColonistBarColonistDrawer_.DrawColonist.Toggle();
			InspectPaneFiller_.DrawMood.Toggle();
			InspirationHandler_.StartInspirationMTBDays.Toggle();
			//Beauty
			Need_Beauty_.LevelFromBeauty.Toggle();
			//Comfort
			Need_Comfort_.CurInstantLevel.Toggle();
			//Outdoors
			Need_Outdoors_.NeedInterval.Toggle();
#if !v1_2
			//Indoors
			Need_Indoors_.NeedInterval.Toggle();
			//Suppression
			Need_Suppression_.DrawSuppressionBar.Toggle();
#endif
#if !v1_2 && !v1_3
			//KillThirst
			Need_KillThirst_.Notify_KilledPawn.Toggle();
			Need_KillThirst_.NeedInterval_Drain.Toggle();
			//Need_Learning
			Need_Learning_.Learn.Toggle();
			//Gizmo_GrowthTier_.DrawLearning.Toggle();
			//Need_MechEnergy
			InspectPaneFiller_.DrawMechEnergy.Toggle();
			//Need_Play
			Need_Play_.Play.Toggle();
#endif
			Debug.WatchLog("static Patches...", "Applying PatchList Patches.");
			foreach (Patch_Single patch in Patches)
				patch.Toggle();
			Debug.WatchLog("PatchList Patches...", "Applying Mod Patches.");
			ApplyModPatches();
			Debug.WatchLog("Mod Patches...");
			Debug.WatchStop("Done Applying Patches.");
		}

		//CM Color Coded Mood Bar [1.1+]
		//https://steamcommunity.com/sharedfiles/filedetails/?id=2006605356
		public static void ApplyModPatches()
			=> CM_Color_Coded_Mood_Bar.Toggle();
	}
}
