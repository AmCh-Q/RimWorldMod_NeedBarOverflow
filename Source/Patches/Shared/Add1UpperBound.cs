using HarmonyLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	// In many parts of the game (especiall when drawing UI), the game expects Need.CurLevelPercentage to be between 0-1
	// If Need.CurLevelPercentage > 1, the UI may be drawn out of the box
	// This patch inserts a Mathf.Min(value,1f) call after Need.CurLevelPercentage to correct that

	public static class Add1UpperBound
	{
		public static readonly Delegate d_transpiler = TranspilerMethod;
		public static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (state == 0 && codeInstruction.Calls(Refs.get_CurLevelPercentage))
				{
					state = 1;
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, Refs.m_Min);
				}
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
