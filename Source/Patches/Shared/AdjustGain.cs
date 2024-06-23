using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NeedBarOverflow.Patches
{
	public static class AdjustGain
	{
		public static readonly MethodInfo
			m_adjust = ((Delegate)AdjustMethod).Method;
		public static float AdjustMethod(float m, float multiplier, float curLevelPercentage)
			=> m / Mathf.Max((curLevelPercentage - 1f) * multiplier + 1f, 1f);
		public static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, Func<float> Multiplier)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				// This patch may be repeated
				if (i >= 1 && i < instructionList.Count - 1 &&  // Not beginning or end of instructions
					!instructionList[i - 1].Calls(Refs.get_CurLevel) && // The base amount is not on top of stack
					codeInstruction.opcode == OpCodes.Add &&      // The amount to gain is on top of stack
					instructionList[i + 1].Calls(Refs.set_CurLevel)) // In Vanilla, the amount after gain will be set
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, Multiplier.Method);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_CurLevelPercentage);
					yield return new CodeInstruction(OpCodes.Call, m_adjust);
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
