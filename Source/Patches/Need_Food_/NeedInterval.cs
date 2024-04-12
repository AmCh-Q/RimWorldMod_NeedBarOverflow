using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches.Need_Food_
{
	using static Patches.Utility;
	using Needs;

	public static class NeedInterval
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Food)
			.Method(nameof(Need_Food.NeedInterval));
		private static readonly TransIL transpiler = Transpiler;
		private static readonly Dictionary<Pawn, float>
			pawnsWithFoodOverflow = new Dictionary<Pawn, float>();
		public static void Toggle()
			=> Toggle(Setting_Food.AffectHealth);
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
			ResetHediff();
		}
		public static void ResetHediff()
		{
			if (!patched.HasValue)
			{
				ClearHediff();
				return;
			}
			foreach (Pawn pawn in new List<Pawn>(pawnsWithFoodOverflow.Keys))
			{
				pawnsWithFoodOverflow[pawn] = -1f;
				UpdateHediff(pawn);
			}
		}
		public static void ClearHediff()
		{
			foreach (Pawn pawn in pawnsWithFoodOverflow.Keys)
			{
#if (v1_2 || v1_3 || v1_4)
				Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Refs.FoodOverflow);
				if (hediff != null)
					pawn.health.RemoveHediff(hediff);
#else
				if (pawn.health.hediffSet.TryGetHediff(Refs.FoodOverflow, out Hediff hediff))
					pawn.health.RemoveHediff(hediff);
#endif
			}
			pawnsWithFoodOverflow.Clear();
		}
		public static void UpdateHediff(Pawn pawn)
		{
			Need_Food need = pawn?.needs?.food;
			if (need == null)
				return;
			UpdateHediff(need.CurLevel, need, pawn);
		}
#if (v1_2 || v1_3 || v1_4)
		private static readonly AccessTools.FieldRef<Hediff, bool>
			fr_visible = AccessTools.FieldRefAccess<Hediff, bool>(
			typeof(Hediff).GetField("visible", Consts.bindingflags));
#endif
		private static void UpdateHediff(
			float newValue, Need_Food need, Pawn pawn)
		{
			Settings s = PatchApplier.s;
			Pawn_HealthTracker health = pawn.health;
			Hediff hediff;
			if (newValue <= need.MaxLevel)
			{
#if (v1_2 || v1_3 || v1_4)
				if (pawnsWithFoodOverflow.Remove(pawn))
				{
					hediff = health.hediffSet.GetFirstHediffOfDef(Refs.FoodOverflow);
					if (hediff != null)
						health.RemoveHediff(hediff);
				}
#else
				if (pawnsWithFoodOverflow.Remove(pawn) &&
					health.hediffSet.TryGetHediff(Refs.FoodOverflow, out hediff))
					health.RemoveHediff(hediff);
#endif
				return;
			}
#if (v1_2 || v1_3 || v1_4)
			hediff = health.hediffSet.GetFirstHediffOfDef(Refs.FoodOverflow);
			if (hediff == null)
				hediff = health.AddHediff(Refs.FoodOverflow);
#else
			hediff = health.GetOrAddHediff(Refs.FoodOverflow);
#endif
			if (!pawnsWithFoodOverflow.TryGetValue(pawn, out float effectMultiplier) ||
				effectMultiplier < 0f)
			{
				if (!pawn.RaceProps.Humanlike)
					effectMultiplier = Setting_Food.EffectStat(StatName_Food.NonHumanMult);
				else if ((bool)(pawn.story?.traits?.HasTrait(Refs.Gourmand)))
					effectMultiplier = Setting_Food.EffectStat(StatName_Food.GourmandMult);
				else
					effectMultiplier = 1f;
				pawnsWithFoodOverflow[pawn] = effectMultiplier;
			}
			hediff.Severity = (need.CurLevelPercentage - 1) * effectMultiplier;
			if (!hediff.Visible && hediff.Severity > 
				(Setting_Food.EffectStat(StatName_Food.ShowHediffLvl) - 1f))
			{
#if (v1_2 || v1_3 || v1_4)
				fr_visible(hediff) = true;
#else
				hediff.SetVisible();
#endif
			}
		}
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo m_UpdateHediff = ((Action<float, Need_Food, Pawn>)UpdateHediff).Method;
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				if (state == 0 && i >= 1 &&			  // Haven't patched yet
					instructionList[i - 1].opcode == OpCodes.Sub &&
					codeInstruction.Calls(set_CurLevel)) // Vanilla is going to set updated CurLevel
				{
					state = 1;
					// Do checks in UpdateHediff() and apply hediff
					yield return new CodeInstruction(OpCodes.Dup);		// get a copy of new value
					yield return new CodeInstruction(OpCodes.Ldarg_0);	// get need
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldfld, f_needPawn);	// Get need.pawn
					yield return new CodeInstruction(OpCodes.Call, m_UpdateHediff); // UpdateHediff
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}