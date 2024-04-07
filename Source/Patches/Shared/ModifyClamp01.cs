using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
	// Many methods in the vanilla game uses Mathf.ModifyClamp01 to clamp the needs
	// This patch replaces them with a more general Mathf.Clamp with adjusted upper bounds
	using static Utility;
	public static class ModifyClamp01
	{
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, Func<float> MaxValue)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (!codeInstruction.Calls(m_Clamp01))
				{
					yield return codeInstruction;
					continue;
				}
				// In this case, we've reached the portion of code to patch
				// This patch may be repeated

				// stackTop, before ops: the value to be clamped
				// vanilla, after ops: value clamped to 0-1
				// patched, after ops: value clamped to 0-MaxValue
				state++;
				yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
				yield return new CodeInstruction(OpCodes.Call, MaxValue.Method);
				yield return new CodeInstruction(OpCodes.Call, m_Clamp);
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
