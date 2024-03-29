using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow.Patches
{
	using P = PatchApplier;
	using C = Consts;
	using static Utility;
	public static class AdjustGain
	{
		public static readonly MethodInfo
			adjust = ((Func<float, Need, int, float>)Adjust).Method;
		public static float Adjust(float m, Need n, int c) 
			=> m / (P.s.statsB[C.V(c, 2)] * Mathf.Max(n.CurLevelPercentage - 1f, 0f) + 1f);
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, int parameter_need)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				// This patch may be repeated
				if (i >= 1 && i < instructionList.Count - 1 &&	// Not beginning or end of instructions
					codeInstruction.opcode == OpCodes.Add &&	  // The amount to gain is on top of stack
					instructionList[i + 1].Calls(set_CurLevel) && // In Vanilla, the amount after gain will be set
					!instructionList[i - 1].Calls(get_CurLevel))  // The base amount is not on top of stack
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, parameter_need); // Load class idx of need
					yield return new CodeInstruction(OpCodes.Call, adjust);	 // Adjust amount to gain, next gain normally
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
