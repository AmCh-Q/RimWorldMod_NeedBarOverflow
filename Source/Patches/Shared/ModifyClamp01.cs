using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches;

// Many methods in the vanilla game uses Mathf.ModifyClamp01 to clamp the needs
// This patch replaces them with a more general Mathf.Clamp with adjusted upper bounds
public static class ModifyClamp01
{
	public static readonly Delegate
		d_transpiler = TranspilerMethod;
	public static IEnumerable<CodeInstruction> TranspilerMethod(
		IEnumerable<CodeInstruction> instructions, ILGenerator ilg, MethodInfo get_MaxValue)
	{
		ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
		int state = 0;
		Label[] jumpLabels = new Label[2];
		for (int i = 0; i < 2; i++)
			jumpLabels[i] = ilg.DefineLabel();
		for (int i = 0; i < instructionList.Count; i++)
		{
			CodeInstruction codeInstruction = instructionList[i];
			if (!codeInstruction.Calls(Refs.m_Clamp01))
			{
				yield return codeInstruction;
				continue;
			}
			// In this case, we've reached the portion of code to patch
			// This patch may be repeated
			state++; // increment counter of how many times the patch ran

			// stackTop, before ops: the value to be clamped
			// value = CanOverflow(this) ? Clamp(value, 0f, MaxValue) : Clamp01(value)
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Call, Refs.m_CanOverflow);
			yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabels[0]);
			yield return codeInstruction;
			yield return new CodeInstruction(OpCodes.Br_S, jumpLabels[1]);
			yield return new CodeInstruction(OpCodes.Ldc_R4, 0f).WithLabels(jumpLabels[0]);
			yield return new CodeInstruction(OpCodes.Call, get_MaxValue);
			yield return new CodeInstruction(OpCodes.Call, Refs.m_Clamp);
			yield return instructionList[i + 1].WithLabels(jumpLabels[1]);
			i++;
		}
		// Check that the patch has been applied at least once
		Debug.CheckTranspiler(state, state > 0);
	}
}
