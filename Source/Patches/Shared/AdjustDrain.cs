using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
	using static Utility;
	public static class AdjustDrain
	{
		public static readonly MethodInfo
			adjust = ((Func<float, float, float, float>)Adjust).Method;
		public static float Adjust(float m, float multiplier, float curLevelPercentage)
			=> m * Mathf.Max((curLevelPercentage - 1f) * multiplier + 1f, 1f);
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, Func<float> DrainMultiplier)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				// This patch may be repeated
				if (i < instructionList.Count - 1 &&			// Not end of instructions
					codeInstruction.opcode == OpCodes.Sub &&	// The amount to drain is on top of stack
					instructionList[i + 1].Calls(set_CurLevel)) // In Vanilla, the amount after drain will be set 
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, DrainMultiplier.Method);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevelPercentage);
					yield return new CodeInstruction(OpCodes.Call, adjust);
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
