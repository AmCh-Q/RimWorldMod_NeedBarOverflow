#if g1_4
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches;

public sealed class Need_Play_Play() : Patch_Single(
	original: typeof(Need_Play).Method(nameof(Need_Play.Play)),
	transpiler: TranspilerMethod)
{
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Play)));
	private static IEnumerable<CodeInstruction> TranspilerMethod(
		IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
	{
		ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
		int state = 0;
		Label jumpLabel = ilg.DefineLabel();
		for (int i = 0; i < instructionList.Count; i++)
		{
			CodeInstruction codeInstruction = instructionList[i];
			// Skip the Clamp part
			if (state == 0 && i < instructionList.Count - 5 &&
				codeInstruction.opcode == OpCodes.Ldarg_0 &&
				instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
				instructionList[i + 2].Calls(Refs.get_CurLevelPercentage) &&
				instructionList[i + 3].Calls(Refs.m_Clamp01) &&
				instructionList[i + 4].Calls(Refs.set_CurLevelPercentage))
			{
				state = 1;
				yield return codeInstruction;
				yield return new CodeInstruction(OpCodes.Call, Refs.m_CanOverflow);
				yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabel);
				yield return instructionList[i + 1];
				yield return new CodeInstruction(OpCodes.Dup);
				yield return instructionList[i + 2];
				yield return instructionList[i + 3];
				yield return instructionList[i + 4];
				yield return instructionList[i + 5].WithLabels(jumpLabel);
				i += 5;
				continue;
			}
			yield return codeInstruction;
		}
		Debug.CheckTranspiler(state, 1);
	}
}

#endif
