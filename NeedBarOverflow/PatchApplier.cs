using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
    using C = Constants;
	using S = Settings;
	using D = Debug;
	using N = NeedBarOverflow;
    using P = Patches;

    public static class PatchApplier
    {
        public static S s;
        private static readonly Type patchType = typeof(N);
        private static readonly Harmony harmony = new Harmony(id: "AmCh.NeedBarOverflow");
        private delegate IEnumerable<CodeInstruction> Func_Transpiler(IEnumerable<CodeInstruction> c);
        private delegate IEnumerable<CodeInstruction> Func_TranspilerILG(IEnumerable<CodeInstruction> c, ILGenerator i = null);
        private delegate void ActionRef<T>(ref T t1);
        private delegate void ActionRef_r2<T1,T2>(T1 t1, ref T2 t2);
        private delegate void ActionRef_r3<T1,T2,T3>(T1 t1, T2 t2, ref T3 t3);
        private static MethodInfo PatchMethod(string typeColonName, MethodType mType = MethodType.Normal,
			Type[] parameters = null, Type[] generics = null,
			Delegate prefix = null, Delegate postfix = null, Delegate transpiler = null)
		{
			if (typeColonName is null) return null;
			var parts = typeColonName.Split(':');
			if (parts.Length != 2) return null;
			return PatchMethod(AccessTools.TypeByName(parts[0]), parts[1], mType, parameters, generics, prefix, postfix, transpiler);
        }
		private static MethodInfo PatchMethod(Type type, string name,
			MethodType mType = MethodType.Normal, Type[] parameters = null, Type[] generics = null,
			Delegate prefix = null, Delegate postfix = null, Delegate transpiler = null)
			=> PatchMethod(type, name, mType, parameters, generics,
			prefix?.Method, postfix?.Method, transpiler?.Method);

        private static MethodInfo PatchMethod(Type type, string name,
			MethodType mType = MethodType.Normal, Type[] parameters = null, Type[] generics = null,
			MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
		{
			MethodBase original;
			switch (mType)
			{
				case MethodType.Getter:
					original = AccessTools.PropertyGetter(type, name);
					break;
				case MethodType.Setter:
					original = AccessTools.PropertySetter(type, name);
					break;
				case MethodType.Constructor:
					original = AccessTools.Constructor(type, parameters, false);
					break;
				case MethodType.StaticConstructor:
					original = AccessTools.Constructor(type, parameters, true);
					break;
				case MethodType.Enumerator:
				case MethodType.Normal:
				default:
					original = AccessTools.Method(type, name, parameters, generics);
					break;
			}
			if (original == null)
			{
				D.Error(string.Format("PatchMethod patching {0}:{1} - No matching original function found.", type, name));
				return null;
			}
			return PatchMethod(original, prefix, postfix, transpiler);
		}
		private static MethodInfo PatchMethod(MethodBase original,
			Delegate prefix = null, Delegate postfix = null, Delegate transpiler = null)
			=> PatchMethod(original, prefix?.Method, postfix?.Method, transpiler?.Method);
        private static MethodInfo PatchMethod(MethodBase original,
            MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            HarmonyMethod preMethod = (prefix == null) ? null : new HarmonyMethod(prefix);
            HarmonyMethod postMethod = (postfix == null) ? null : new HarmonyMethod(postfix);
            HarmonyMethod transMethod = (transpiler == null) ? null : new HarmonyMethod(transpiler);
            if (original == null)
            {
                D.Error("PatchMethod - original method is null.");
                return null;
            }
            return harmony.Patch(original, prefix: preMethod, postfix: postMethod, transpiler: transMethod);
        }
        private static bool ShouldApplyPatch(int enableIdx, int sessionEnableIdx)
			=> ShouldApplyPatch(s.enabledA[enableIdx], sessionEnableIdx);
        private static bool ShouldApplyPatch(bool enabled, int sessionEnableIdx)
		{
			if (!enabled || s.patches_Session[sessionEnableIdx])
				return false;
			D.Message(string.Format("Enabling patch #{0}", sessionEnableIdx));
			s.patches_Session[sessionEnableIdx] = true;
			return true;
		}
		public static void ApplyPatches()
		{
			if (s == null || !s.AnyPatchEnabled)
			{
				if (s == null)
					D.Message("NeedBarOverflow_Patches ApplyPatches() quit becasue s is null");
				else
					D.Message("NeedBarOverflow_Patches ApplyPatches() quit becasue s.AnyPatchEnabled is false");
				return;
			}
			D.Message("NeedBarOverflow_Patches ApplyPatches() called");
			//General Patches
			if (!s.enableGlobal_Session)
            {
				s.enableGlobal_Session = true;
				D.Message("Enabling Global patches");
                foreach (string funcName in new string[] { nameof(GenUI.BottomPart), nameof(GenUI.LeftPart), nameof(GenUI.RightPart), nameof(GenUI.TopPart) })
                    PatchMethod(typeof(GenUI), funcName, prefix: P.GenUI.Prefix);
                PatchMethod(typeof(Need), nameof(Need.CurLevel), mType: MethodType.Setter, transpiler: P.CurLevel.Transpiler);
				PatchMethod(typeof(GenUI), nameof(GenUI.DrawStatusLevel), transpiler: P.CurLevelPercentage.Transpiler);
                D.Message("Enabling DrawOnGUI");
                PatchMethod(typeof(Need), nameof(Need.DrawOnGUI), prefix: P.DrawOnGUI.Prefix, transpiler: P.DrawOnGUI.Transpiler);
                D.Message("Done Enabling DrawOnGUI");
                if (!ModLister.HasActiveModWithName("Performance Optimizer"))
					PatchMethod(typeof(BeautyUtility), nameof(BeautyUtility.AverageBeautyPerceptible), prefix: P.AverageBeautyPerceptible.Prefix, postfix: P.AverageBeautyPerceptible.Postfix);
                D.Message("Done Enabling Global patches");
            }
			//Food Patches
			if (s.enabledA[C.Food])
			{
				if (ShouldApplyPatch(C.Food, C.FoodNutri))
					PatchMethod(typeof(Need_Food), nameof(Need_Food.NutritionWanted), mType: MethodType.Getter, prefix: P.Clamp0.Postfix);
				if (ShouldApplyPatch(s.enabledB[C.V(C.Food, 1)], C.FoodNoEat))
				{
					PatchMethod(typeof(FloatMenuMakerMap), "AddHumanlikeOrders", postfix: P.AddHumanlikeOrders.Postfix);
					PatchMethod(typeof(FoodUtility), nameof(FoodUtility.WillIngestFromInventoryNow), postfix: P.WillIngestFromInventoryNow.Postfix);
				}
				if (ShouldApplyPatch(s.FoodOverflowAffectHealth, C.FoodHediff))
					PatchMethod(typeof(Need_Food), nameof(Need_Food.NeedInterval), transpiler: P.Food_NeedInterval.Transpiler);
			}
			//Rest Patches
			if (s.enabledA[C.Rest])
            {
				if (ShouldApplyPatch(s.enabledB[C.V(C.Rest, 1)], C.RestDrain))
                {
					S.patchParamInt[0] = C.Rest;
					PatchMethod(typeof(Need_Rest), nameof(Need_Rest.NeedInterval), transpiler: P.NeedInterval.Drain_Transpiler);
				}
				if (ShouldApplyPatch(s.enabledB[C.V(C.Rest, 2)], C.RestGain))
				{
					S.patchParamInt[0] = C.Rest;
					PatchMethod(typeof(Need_Rest), nameof(Need_Rest.NeedInterval), transpiler: P.NeedInterval.Gain_Transpiler);
				}
			}
			//Joy Patches
			if (s.enabledA[C.Joy])
			{
				if (ShouldApplyPatch(C.Joy, C.JoyPatch))
					PatchMethod(typeof(Need_Joy), nameof(Need_Joy.GainJoy), transpiler: P.GainJoy.Transpiler);
				if (ShouldApplyPatch(s.enabledB[C.V(C.Joy, 1)], C.JoyDrain))
                {
                    S.patchParamInt[0] = C.Joy;
					PatchMethod(typeof(Need_Joy), nameof(Need_Joy.NeedInterval), transpiler: P.NeedInterval.Drain_Transpiler);
				}
				if (ShouldApplyPatch(s.enabledB[C.V(C.Joy, 2)], C.JoyGain))
					PatchMethod(typeof(Need_Joy), nameof(Need_Joy.GainJoy), prefix: P.GainJoy.Prefix);
			}
			//Mood Patches
			if (ShouldApplyPatch(C.Mood, C.MoodPatch))
			{
				S.patchParamInt[0] = C.Mood;
				PatchMethod(typeof(Need_Mood), nameof(Need_Mood.CurInstantLevel), mType: MethodType.Getter, transpiler: P.Clamp01.Transpiler);
				PatchMethod(typeof(ColonistBarColonistDrawer), nameof(ColonistBarColonistDrawer.DrawColonist), transpiler: P.CurLevelPercentage.Transpiler);
				PatchMethod(typeof(InspectPaneFiller), "DrawMood", transpiler: P.CurLevelPercentage.Transpiler);
				PatchMethod(typeof(InspirationHandler), "StartInspirationMTBDays", mType: MethodType.Getter, transpiler: P.StartInspirationMTBDays.Transpiler);
			}
			//Beauty
			if (ShouldApplyPatch(C.Beauty, C.BeautyPatch))
			{
				S.patchParamInt[0] = C.Beauty;
				PatchMethod(typeof(Need_Beauty), "LevelFromBeauty", transpiler: P.Clamp01.Transpiler);
			}
			//Comfort
			if (ShouldApplyPatch(C.Comfort, C.ComfortPatch))
			{
				S.patchParamInt[0] = C.Comfort;
				PatchMethod(typeof(Need_Comfort), nameof(Need_Comfort.CurInstantLevel), mType: MethodType.Getter, transpiler: P.Clamp01.Transpiler);
			}
			//Outdoors
			if (ShouldApplyPatch(C.Outdoors, C.OutdoorsPatch))
				PatchMethod(typeof(Need_Outdoors), nameof(Need_Outdoors.NeedInterval), transpiler: P.NeedInterval.RemoveLastMin_Transpiler);
#if (v1_3 || v1_4 || v1_5)
            //Indoors
            if (ShouldApplyPatch(C.Indoors, C.IndoorsPatch))
				PatchMethod(typeof(Need_Indoors), nameof(Need_Indoors.NeedInterval), transpiler: P.NeedInterval.RemoveLastMin_Transpiler);
			//Suppression
			if (ShouldApplyPatch(C.Suppression, C.SuppressionPatch))
			{
				PatchMethod(typeof(Need_Suppression), nameof(Need_Suppression.DrawSuppressionBar), transpiler: P.CurLevelPercentage.Transpiler);
				PatchMethod(typeof(Need_Suppression), nameof(Need_Suppression.DrawSuppressionBar), transpiler: P.DrawSuppressionBar.Transpiler);
			}
#endif
#if (v1_4 || v1_5)
            //KillThirst
            if (s.enabledA[C.KillThirst])
			{
				if (ShouldApplyPatch(C.KillThirst, C.KillThirstPatch))
					PatchMethod(typeof(Need_KillThirst), nameof(Need_KillThirst.Notify_KilledPawn), transpiler: P.Notify_KilledPawn.Transpiler);
				if (ShouldApplyPatch(s.enabledB[C.V(C.KillThirst, 1)], C.KillThirstDrain))
				{
					S.patchParamInt[0] = C.KillThirst;
					PatchMethod(typeof(Need_KillThirst), nameof(Need_KillThirst.NeedInterval), transpiler: P.NeedInterval.Drain_Transpiler);
				}
			}
			//Need_Learning
			if (ShouldApplyPatch(C.Learning, C.LearnPatch))
			{
				PatchMethod(typeof(Need_Learning), nameof(Need_Learning.Learn), transpiler: P.Learn.Transpiler);
				PatchMethod(typeof(Gizmo_GrowthTier), "DrawLearning", transpiler: P.CurLevelPercentage.Transpiler);
			}
			//Need_MechEnergy
			if (ShouldApplyPatch(C.MechEnergy, C.MechEnergyPatch))
				PatchMethod(typeof(InspectPaneFiller), "DrawMechEnergy", transpiler: P.CurLevelPercentage.Transpiler);
			//Need_Play
			if (ShouldApplyPatch(C.Play, C.PlayPatch))
				PatchMethod(typeof(Need_Play), nameof(Need_Play.Play), transpiler: P.Play.Transpiler);
#endif
		}
	}
}
