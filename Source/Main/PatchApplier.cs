using NeedBarOverflow.Patches.ModCompat;

namespace NeedBarOverflow.Patches
{
	public static class PatchApplier
	{
		public static Settings s;
		public static void ApplyPatches()
		{
			if (s == null)
				return;
			Debug.Message("Applying Patches...");
			//General Patches
			SavedGameLoaderNow_.LoadGameFromSaveFileNow	.Toggle();
			GenUI_.BottomPart							.Toggle();
			GenUI_.DrawStatusLevel						.Toggle();
			Need_.CurLevel								.Toggle();
			Need_.DrawOnGUI								.Toggle();
			BeautyUtility_.AverageBeautyPerceptible		.Toggle();
			//Setting_Food
			Need_Food_.NutritionWanted				.Toggle();
			FloatMenuMakerMap_.AddHumanlikeOrders	.Toggle();
			FoodUtility_.WillIngestFromInventoryNow	.Toggle();
			Need_Food_.NeedInterval					.Toggle();
			TraitSet_.GainTrait						.Toggle();
			//Rest
			Need_Rest_.NeedInterval.Toggle();
			//Scribe_Joy
			Need_Joy_.GainJoy			.Toggle();
			Need_Joy_.NeedInterval_Drain.Toggle();
			Need_Joy_.GainJoy_Gain		.Toggle();
			//Mood
			Need_Mood_.CurInstantLevel					.Toggle();
			ColonistBarColonistDrawer_.DrawColonist		.Toggle();
			InspectPaneFiller_.DrawMood					.Toggle();
			InspirationHandler_.StartInspirationMTBDays	.Toggle();
			//Beauty
			Need_Beauty_.LevelFromBeauty.Toggle();
			//Comfort
			Need_Comfort_.CurInstantLevel.Toggle();
			//Outdoors
			Need_Outdoors_.NeedInterval.Toggle();
#if (v1_3 || v1_4 || v1_5)
			//Indoors
			Need_Indoors_.NeedInterval.Toggle();
			//Suppression
			Need_Suppression_.DrawSuppressionBar.Toggle();
#endif
#if (v1_4 || v1_5)
			//KillThirst
			Need_KillThirst_.Notify_KilledPawn	.Toggle();
			Need_KillThirst_.NeedInterval_Drain	.Toggle();
			//Need_Learning
			Need_Learning_.Learn			.Toggle();
			Gizmo_GrowthTier_.DrawLearning	.Toggle();
			//Need_MechEnergy
			InspectPaneFiller_.DrawMechEnergy.Toggle();
			//Need_Play
			Need_Play_.Play.Toggle();
#endif
			ApplyModPatches();
            Debug.Message("Done Applying Patches.");
		}
		public static void ApplyModPatches()
        {
            //CM Color Coded Mood Bar [1.1+]
            //https://steamcommunity.com/sharedfiles/filedetails/?id=2006605356
            CM_Color_Coded_Mood_Bar.Toggle();
        }
	}
}
