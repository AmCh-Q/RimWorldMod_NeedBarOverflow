using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NeedBarOverflow.Patches;

// For some need bars (such as rest, recreation, etc.)
//   If the need bar is overflowing
//   Use this patch to boost the drain (decrease) rate
public static class AdjustDrain
{
	// Primarily for use in TranspilerMethod under this class
	//   Can also be used if external methods increments/decrements a need
	public static readonly MethodInfo
		m_adjust = ((Delegate)AdjustMethod).Method;

	// Formula to adjust the offset
	//   Higher percentage -> higher offset drain
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float AdjustMethod(float m, float multiplier, float curLevelPercentage)
		=> m * Mathf.Max((curLevelPercentage - 1f) * multiplier + 1f, 1f);

	// Multiplier needs to be a static method which gets multiplier value from config
	public static IEnumerable<CodeInstruction> TranspilerMethod(
		IEnumerable<CodeInstruction> instructions, Func<float> Multiplier)
	{
		ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
		int state = 0;
		for (int i = 0; i < instructionList.Count; i++)
		{
			CodeInstruction codeInstruction = instructionList[i];
			if (// Not end of instructions
				i < instructionList.Count - 1 &&
				// The amount to drain is on top of stack
				codeInstruction.opcode == OpCodes.Sub &&
				// In Vanilla, the amount after drain will be set
				instructionList[i + 1].Calls(Refs.set_CurLevel))
			{
				// Here we've reached the portion of code to patch
				// This patch may be repeated
				state++; // increment counter of how many times the patch ran

				// We modify the amount to drain ("offset") on top of stack
				// offset = AdjustMethod(offset, Multiplier(), Need.CurLevelPercentage)
				yield return new CodeInstruction(OpCodes.Call, Multiplier.Method);
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_CurLevelPercentage);
				yield return new CodeInstruction(OpCodes.Call, m_adjust);
			}
			yield return codeInstruction;
		}
		// Check that the patch has been applied at least once
		Debug.CheckTranspiler(state, state > 0);
	}
}
