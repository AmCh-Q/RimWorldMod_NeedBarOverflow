using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Food_NeedInterval() : Patch_Single(
		original: typeof(Need_Food).Method(nameof(Need_Food.NeedInterval)),
		transpiler: TranspilerMethod)
	{
		public static readonly Dictionary<Pawn, float> pawnsWithFoodOverflow = [];

#if l1_4
		private static readonly AccessTools.FieldRef<Hediff, bool>
			fr_visible = AccessTools.FieldRefAccess<Hediff, bool>(
			typeof(Hediff).GetField("visible", Consts.bindAll));
#endif

		public override void Toggle()
			=> Toggle(Setting_Food.AffectHealth);
		public override void Toggle(bool enable)
		{
			base.Toggle(enable);
			if (Current.ProgramState == ProgramState.Playing)
				ResetHediff();
		}
		public static void ResetHediff()
		{
			pawnsWithFoodOverflow.Clear();
			foreach (Pawn pawn in Find.WorldPawns.AllPawnsAliveOrDead.Where(HasHediff))
				pawnsWithFoodOverflow.TryAdd(pawn, -1);

			foreach (Map map in Find.Maps)
			{
				foreach (Pawn pawn in map.mapPawns.AllPawns.Where(HasHediff))
					pawnsWithFoodOverflow.TryAdd(pawn, -1);
			}

			Pawn[] pawnArr = [.. pawnsWithFoodOverflow.Keys];
			if (PatchApplier.Patched(typeof(Need_Food_NeedInterval)))
			{
				foreach (Pawn pawn in pawnArr)
					UpdateHediff(pawn);
			}
			else
			{
				foreach (Pawn pawn in pawnArr)
					RemoveHediff(pawn, false);
				pawnsWithFoodOverflow.Clear();
			}
		}
		private static bool HasHediff(Pawn pawn)
			=> pawn.health?.hediffSet.HasHediff(ModDefOf.FoodOverflow) ?? false;
		private static void RemoveHediff(Pawn pawn, bool removeFromList = true)
		{
#if l1_4
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.FoodOverflow);
			if (hediff is not null)
				pawn.health.RemoveHediff(hediff);
#else
			if (pawn.health.hediffSet.TryGetHediff(ModDefOf.FoodOverflow, out Hediff hediff))
				pawn.health.RemoveHediff(hediff);
#endif
			if (removeFromList)
				pawnsWithFoodOverflow.Remove(pawn);
		}
		public static void UpdateHediff(Pawn pawn)
		{
			if (pawn.needs?.food is not Need_Food need
				|| pawn.Destroyed)
			{
				RemoveHediff(pawn);
				return;
			}
			pawnsWithFoodOverflow[pawn] = -1f;
			UpdateHediff(need.CurLevel, need, pawn);
		}
		private static void UpdateHediff(
			float newValue, Need_Food need, Pawn pawn)
		{
			if (NeedBarOverflow.settings is not Settings s)
				return;
			Pawn_HealthTracker health = pawn.health;
			Hediff hediff;
			if (newValue <= need.MaxLevel
				|| !Setting_Common.CanOverflow(need, pawn))
			{
				if (newValue > need.MaxLevel)
					need.CurLevel = need.MaxLevel;
				RemoveHediff(pawn);
				return;
			}
			if (!pawnsWithFoodOverflow.TryGetValue(pawn, out float effectMultiplier) ||
				effectMultiplier < 0f)
			{
				if (!pawn.RaceProps.Humanlike)
					effectMultiplier = Setting_Food.EffectStat(StatName_Food.NonHumanMult);
				else if (pawn.story?.traits?.HasTrait(ModDefOf.Gourmand) ?? false)
					effectMultiplier = Setting_Food.EffectStat(StatName_Food.GourmandMult);
				else
					effectMultiplier = 1f;
				pawnsWithFoodOverflow[pawn] = effectMultiplier;
			}
#if l1_4
			hediff = health.hediffSet.GetFirstHediffOfDef(ModDefOf.FoodOverflow);
			hediff ??= health.AddHediff(ModDefOf.FoodOverflow);
#else
			hediff = health.GetOrAddHediff(ModDefOf.FoodOverflow);
#endif
			hediff.Severity = (need.CurLevelPercentage - 1) * effectMultiplier;
			if (!hediff.Visible && hediff.Severity >
				(Setting_Food.EffectStat(StatName_Food.ShowHediffLvl) - 1f))
			{
#if l1_4
				fr_visible(hediff) = true;
#else
				hediff.SetVisible();
#endif
			}
		}
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo m_UpdateHediff = ((Action<float, Need_Food, Pawn>)UpdateHediff).Method;
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				if (state == 0 && i >= 1 &&        // Haven't patched yet
					instructionList[i - 1].opcode == OpCodes.Sub &&
					codeInstruction.Calls(Refs.set_CurLevel)) // Vanilla is going to set updated CurLevel
				{
					state = 1;
					// Do checks in UpdateHediff() and apply hediff
					yield return new CodeInstruction(OpCodes.Dup);    // get a copy of new value
					yield return new CodeInstruction(OpCodes.Ldarg_0);  // get need
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldfld, Refs.f_needPawn);    // Get need.pawn
					yield return new CodeInstruction(OpCodes.Call, m_UpdateHediff); // UpdateHediff
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
