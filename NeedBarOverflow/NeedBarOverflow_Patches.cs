namespace NeedBarOverflow
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Reflection.Emit;
	using HarmonyLib;
	using RimWorld;
	using Verse;
	using C = NeedBarOverflow_Consts;
	using S = NeedBarOverflow_Settings;
	using N = NeedBarOverflow;

	[StaticConstructorOnStartup]
	public static class NeedBarOverflow_Patches
	{
		public static S s = N.s;
		public static readonly Type patchType = typeof(N);
		public static readonly Harmony harmony = new Harmony(id: "AmCh.NeedBarOverflow");
		public delegate IEnumerable<CodeInstruction> d_trans_s(IEnumerable<CodeInstruction> c);
		public delegate IEnumerable<CodeInstruction> d_trans_i(IEnumerable<CodeInstruction> c, ILGenerator i = null);
		public static MethodInfo PatchMethod(string typeColonName, MethodType mType = MethodType.Normal,
			Type[] parameters = null, Type[] generics = null,
			string prefix = null, string postfix = null, Delegate transpiler = null)
		{
			if (typeColonName is null)
			{
#if DEBUG
				throw new ArgumentNullException(nameof(typeColonName));
#else
				return null;
#endif
			}
			var parts = typeColonName.Split(':');
			if (parts.Length != 2)
			{
#if DEBUG
				throw new ArgumentException($" must be specified as 'Namespace.Type1.Type2:MemberName", nameof(typeColonName));
#else
				return null;
#endif
			}
			return PatchMethod(AccessTools.TypeByName(parts[0]), parts[1], mType, parameters, generics, prefix, postfix, transpiler);
		}
		public static MethodInfo PatchMethod(Type type, string name,
			MethodType mType = MethodType.Normal, Type[] parameters = null, Type[] generics = null,
			string prefix = null, string postfix = null, Delegate transpiler = null)
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
#if DEBUG
				Log.Warning(string.Format("[Need Bar Overflow]: PatchMethod patching {0}:{1} - No matching original function found.", type, name));
#endif
				return null;
			}
			return PatchMethod(original, prefix, postfix, transpiler);
		}
		public static MethodInfo PatchMethod(MethodBase original,
			string prefix = null, string postfix = null, Delegate transpiler = null)
		{
			HarmonyMethod preMethod = (prefix == null) ? null : new HarmonyMethod(patchType, prefix);
			HarmonyMethod postMethod = (postfix == null) ? null : new HarmonyMethod(patchType, postfix);
			HarmonyMethod transMethod = (transpiler == null) ? null : new HarmonyMethod(transpiler.Method);
			if (original == null)
				return null;
			return harmony.Patch(original, prefix: preMethod, postfix: postMethod, transpiler: transMethod);
		}
		static NeedBarOverflow_Patches()
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: NeedBarOverflow_Patches constructor called");
#endif
			if (!(s.enableGlobal_Session = s.AnyPatchEnabled))
				return;
			//General Patches
			foreach (string funcName in new string[] { nameof(GenUI.BottomPart), nameof(GenUI.LeftPart), nameof(GenUI.RightPart), nameof(GenUI.TopPart) })
				PatchMethod(typeof(GenUI), funcName, prefix: nameof(N.GenUI_Prefix));
			PatchMethod(typeof(Need), nameof(Need.CurLevel), mType: MethodType.Setter, transpiler: (d_trans_s)N.CurLevel_Transpiler);
			PatchMethod(typeof(GenUI), nameof(GenUI.DrawStatusLevel), transpiler: (d_trans_s)N.CurLevelPercentage_Transpiler);
			PatchMethod(typeof(Need), nameof(Need.DrawOnGUI), transpiler: (d_trans_i)N.DrawOnGUI_Transpiler);
			//Food Patches
			if (s.patches_Session[C.FoodNutri] = s.enabledA[C.Food])
			{
				PatchMethod(typeof(Need_Food), nameof(Need_Food.NutritionWanted), mType: MethodType.Getter, prefix: nameof(N.NutritionWanted_Postfix));
				if (s.patches_Session[C.FoodNoEat] = s.enabledB[new IntVec2(C.Food, 1)])
				{
					PatchMethod(typeof(FloatMenuMakerMap), "AddHumanlikeOrders", postfix: nameof(N.AddHumanlikeOrders_Postfix));
					PatchMethod(typeof(FoodUtility), nameof(FoodUtility.WillIngestFromInventoryNow), postfix: nameof(N.WillIngestFromInventoryNow_Postfix));
				}
				if (s.patches_Session[C.FoodHediff] = s.FoodOverflowAffectHealth)
				{
					PatchMethod(typeof(Need_Food), nameof(Need_Food.NeedInterval), transpiler: (d_trans_i)N.Food_NeedInterval_Transpiler);
				}
			}
			//Rest Patches
			s.patches_Session[C.RestDrain] = s.enabledA[C.Rest] && s.enabledB[new IntVec2(C.Rest, 1)];
			s.patches_Session[C.RestGain] = s.enabledA[C.Rest] && s.enabledB[new IntVec2(C.Rest, 2)];
			if (s.patches_Session[C.RestDrain] || s.patches_Session[C.RestGain])
				PatchMethod(typeof(Need_Rest), nameof(Need_Rest.NeedInterval), transpiler: (d_trans_s)N.Rest_NeedInterval_Transpiler);
			//Joy Patches
			if (s.patches_Session[C.JoyPatch] = s.enabledA[S.patchParamInt[1] = C.Joy])
			{
				PatchMethod(typeof(Need_Joy), nameof(Need_Joy.GainJoy), transpiler: (d_trans_s)N.GainJoy_Transpiler);
				if (s.patches_Session[S.patchParamInt[0] = C.JoyDrain] = s.enabledB[new IntVec2(C.Joy, 1)])
					PatchMethod(typeof(Need_Joy), nameof(Need_Joy.NeedInterval), transpiler: (d_trans_s)N.Drain_NeedInterval_Transpiler);
				if (s.patches_Session[C.JoyGain] = s.enabledB[new IntVec2(C.Joy, 2)])
					PatchMethod(typeof(Need_Joy), nameof(Need_Joy.GainJoy), prefix: nameof(N.GainJoy_Prefix));
			}
			//Mood Patches
			if (s.patches_Session[C.MoodPatch] = s.enabledA[S.patchParamInt[0] = C.Mood])
			{
				PatchMethod(typeof(Need_Mood), nameof(Need_Mood.CurInstantLevel), mType: MethodType.Getter, transpiler: (d_trans_s)N.Clamp01_Transpiler);
				PatchMethod(typeof(ColonistBarColonistDrawer), nameof(ColonistBarColonistDrawer.DrawColonist), transpiler: (d_trans_s)N.CurLevelPercentage_Transpiler);
				PatchMethod(typeof(InspectPaneFiller), "DrawMood", transpiler: (d_trans_s)N.CurLevelPercentage_Transpiler);
				PatchMethod(typeof(InspirationHandler), "StartInspirationMTBDays", mType: MethodType.Getter, transpiler: (d_trans_s)N.StartInspirationMTBDays_Transpiler);
			}
			//Beauty
			if (s.patches_Session[C.BeautyPatch] = s.enabledA[S.patchParamInt[0] = C.Beauty])
				PatchMethod(typeof(Need_Beauty), "LevelFromBeauty", transpiler: (d_trans_s)N.Clamp01_Transpiler);
			//Comfort
			if (s.patches_Session[C.ComfortPatch] = s.enabledA[S.patchParamInt[0] = C.Comfort])
				PatchMethod(typeof(Need_Comfort), nameof(Need_Comfort.CurInstantLevel), mType: MethodType.Getter, transpiler: (d_trans_s)N.Clamp01_Transpiler);
			//Outdoors
			if (s.patches_Session[C.OutdoorsPatch] = s.enabledA[C.Outdoors])
				PatchMethod(typeof(Need_Outdoors), nameof(Need_Outdoors.NeedInterval), transpiler: (d_trans_s)N.RemoveLastMin_Transpiler);
#if !v1_2
			//Indoors
			if (s.patches_Session[C.IndoorsPatch] = s.enabledA[C.Indoors])
				PatchMethod(typeof(Need_Indoors), nameof(Need_Indoors.NeedInterval), transpiler: (d_trans_s)N.RemoveLastMin_Transpiler);
			//Suppression
			if (s.patches_Session[C.SuppressionPatch] = s.enabledA[C.Suppression])
			{
				PatchMethod(typeof(Need_Suppression), nameof(Need_Suppression.DrawSuppressionBar), transpiler: (d_trans_s)N.CurLevelPercentage_Transpiler);
				PatchMethod(typeof(Need_Suppression), nameof(Need_Suppression.DrawSuppressionBar), transpiler: (d_trans_i)N.DrawSuppressionBar_Transpiler);
			}
#endif
#if !v1_2 && !v1_3
			//KillThirst
			if (s.patches_Session[C.KillThirstPatch] = s.enabledA[C.KillThirst])
			{
				if (s.patches_Session[S.patchParamInt[0] = C.KillThirstDrain] = s.enabledB[new IntVec2(S.patchParamInt[1] = C.KillThirst, 1)])
					PatchMethod(typeof(Need_KillThirst), nameof(Need_KillThirst.NeedInterval), transpiler: (d_trans_s)N.Drain_NeedInterval_Transpiler);
				PatchMethod(typeof(Need_KillThirst), nameof(Need_KillThirst.Notify_KilledPawn), transpiler: (d_trans_s)N.KillThirst_KilledPawn_Transpiler);
			}
			//Need_Learning
			if (s.patches_Session[C.LearnPatch] = s.enabledA[C.Learning])
			{
				PatchMethod(typeof(Need_Learning), nameof(Need_Learning.Learn), transpiler: (d_trans_s)N.Learn_Transpiler);
				PatchMethod(typeof(Gizmo_GrowthTier), "DrawLearning", transpiler: (d_trans_s)N.DrawLearning_Transpiler);
			}
			//Need_MechEnergy
			if (s.patches_Session[C.MechEnergyPatch] = s.enabledA[C.MechEnergy])
				PatchMethod(typeof(InspectPaneFiller), "DrawMechEnergy", transpiler: (d_trans_s)N.CurLevelPercentage_Transpiler);
			//Need_Play
			if (s.patches_Session[C.PlayPatch] = s.enabledA[C.Play])
				PatchMethod(typeof(Need_Play), nameof(Need_Play.Play), transpiler: (d_trans_s)N.Play_Transpiler);
#endif
		}
	}
}
