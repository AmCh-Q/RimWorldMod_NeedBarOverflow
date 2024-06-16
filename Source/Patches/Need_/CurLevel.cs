using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_
{
	// Need.CurLevel setter method usually clamps the level between 0 and Need.MaxLevel
	// This patch adjusts the apparent Need.MaxLevel fed to the clamp
	// This method is what allows for need bars overflowing
	using static Utility;
	using Needs;
	public static class CurLevel
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need)
			.Setter(nameof(Need.CurLevel));
		private static readonly TransILG transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
		public static void Toggle(bool enable)
		{
			if (enable)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static float Adjusted_MaxLevel(Need n)
		{
			float m = n.MaxLevel;
			if (!Setting_Common.CanOverflow(n))
				return m;
			Type type = n.GetType();
			float mult = Setting_Common.Overflow(type);
			if (mult < 1)
				return m;
			if (type == typeof(Need_Food))
				return Mathf.Max(m * mult, m + Setting_Food.EffectStat(StatName_Food.OverflowBonus));
			return m * mult;
		}
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			MethodInfo m_Adjusted_MaxLevel
				= ((Func<Need, float>)Adjusted_MaxLevel).Method;
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			Label skipAdjustLabel = ilg.DefineLabel();
			Label needAdjustLabel = ilg.DefineLabel();
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				if (state == 0 && i >= 1 && i < instructionList.Count - 4 &&
					// Haven't patched yet, and not at very beginning or end
					// The new value to be clamped is on top of stack
					instructionList[i - 1].opcode == OpCodes.Ldarg_1 &&
					// The vanilla method would load 0d
					codeInstruction.LoadsConstant(0d) &&
					// The vanilla method would load MaxLevel
					instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 2].Calls(get_MaxLevel) && 
					// The vanilla method would call Clamp(new value, 0f, MaxLevel)
					instructionList[i + 3].Calls(m_Clamp) &&
					// There is an instruction after everything is done
					//   (It's stfld but what it is doesn't matter)
					instructionList.Count > i + 4)
				{
					state = 1;
					// First check if f_curLevelInt < new value
					// Load f_curLevelInt
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, f_curLevelInt);
					// Load new value
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					// Jump if f_curLevelInt < new value
					yield return new CodeInstruction(OpCodes.Blt_S, needAdjustLabel); 

					// Case 1: f_curLevelInt >= new value
					//   that means the new value did not increase
					yield return codeInstruction; // Load 0f
					// Get Max(the new value, 0f) on stack
					yield return new CodeInstruction(OpCodes.Call, m_Max);
					// Skip to end
					yield return new CodeInstruction(OpCodes.Br_S, skipAdjustLabel);

					// Case 2: f_curLevelInt < new value
					//   that means the new value increased and need clamping
					// Load the adjusted MaxLevel value
					yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(needAdjustLabel);
					yield return new CodeInstruction(OpCodes.Call, m_Adjusted_MaxLevel);
					// Get Min(the new value, adjusted MaxLevel)
					yield return new CodeInstruction(OpCodes.Call, m_Min);

					// Done, this is the instruction after everything is done
					yield return instructionList[i + 4].WithLabels(skipAdjustLabel);
					// Then skip the original methods
					//   as well as the original line after done
					i += 4;
					continue;
				}
				// Normal instuctions outside of portion of interest, pass normally
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
