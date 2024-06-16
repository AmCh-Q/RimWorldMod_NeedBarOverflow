using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, m_CanOverflow);
				yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabels[0]);
				yield return codeInstruction;
				yield return new CodeInstruction(OpCodes.Br_S, jumpLabels[1]);
				yield return new CodeInstruction(OpCodes.Ldc_R4, 0f).WithLabels(jumpLabels[0]);
				yield return new CodeInstruction(OpCodes.Call, get_MaxValue);
				yield return new CodeInstruction(OpCodes.Call, m_Clamp);
				yield return instructionList[i + 1].WithLabels(jumpLabels[1]);
				i++;
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
