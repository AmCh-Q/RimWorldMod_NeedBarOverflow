#if g1_4
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches;

// This is technically not needed since the value of
// JobGiver_GetDeathrest.maxNeedPercent is always 0.05 in vanilla
//   and it's unreasonable to be 1f
// But in case another mod modifies this value to be 1f
//   and expects 1f to be max, we might need this patch
// Also warning: if this is enabled
//   then JobGiver_GetDeathrest.TryGiveJob might also need to be patched
#if false
internal class JobGiver_GetDeathrest_GetPriority()
	: Patch_Single(
	original: typeof(JobGiver_GetDeathrest)
		.Method(nameof(JobGiver_GetDeathrest.GetPriority)),
	transpiler: TranspilerMethod)
{
	public override void Toggle()
		=> Toggle(ModsConfig.BiotechActive &&
			Setting_Common.Enabled(typeof(Need_Deathrest)));

	private static IEnumerable<CodeInstruction> TranspilerMethod(
		IEnumerable<CodeInstruction> instructions)
	{
		ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
		FieldInfo f_maxNeedPercent = typeof(JobGiver_GetDeathrest)
			.Field(nameof(JobGiver_GetDeathrest.maxNeedPercent));

		int state = 0;
		for (int i = 0; i < instructionList.Count; i++)
		{
			CodeInstruction codeInstruction = instructionList[i];
			yield return codeInstruction;
			if (state == 0 && i < instructionList.Count - 4 &&
				instructionList[i + 4].LoadsField(f_maxNeedPercent) &&
				codeInstruction.Branches(out Label? jumpDestination) &&
				jumpDestination is not null)
			{
				state = 1;
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldfld, f_maxNeedPercent);
				yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
				yield return new CodeInstruction(OpCodes.Beq_S, jumpDestination);
			}
		}
		Debug.CheckTranspiler(state, state > 0);
	}
}
#endif
#endif
