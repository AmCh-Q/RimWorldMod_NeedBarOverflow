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
using NeedBarOverflow.Needs;

namespace NeedBarOverflow.Patches
{
	// Need.CurLevel setter method usually clamps the level between 0 and Need.MaxLevel
	// This patch adjusts the apparent Need.MaxLevel fed to the clamp
	// This method is what allows for need bars overflowing
	public sealed class Need_CurLevel() : Patch_Single(
		original: typeof(Need).Setter(nameof(Need.CurLevel)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);

		public static float Adjusted_MaxLevel(Need need)
		{
			float originalMax = need.MaxLevel;
			if (!DisableNeedOverflow.Common.CanOverflow(need))
				return originalMax;
			Type type = need.GetType();
			float mult = Setting_Common.GetOverflow(type);
			if (mult < 1)
				return originalMax;
			if (type == typeof(Need_Food))
				return Mathf.Max(originalMax * mult,
					originalMax + Setting_Food.EffectStat(StatName_Food.OverflowBonus));
			return originalMax * mult;
		}

		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			MethodInfo m_Adjusted_MaxLevel
				= ((Func<Need, float>)Adjusted_MaxLevel).Method;
			ReadOnlyCollection<CodeInstruction> instructionList
				= instructions.ToList().AsReadOnly();
			int state = 0;
			Label floorLabel = ilg.DefineLabel();
			Label finishLabel = ilg.DefineLabel();
			for (int i = 0; i < instructionList.Count; i++)
			{
				// Pass current instruction normally
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;

				// If the code does not match all of below, continue
				if (!(
					// Not patched yet
					state == 0 &&
					// The newValue to be clamped is on top of stack
					codeInstruction.opcode == OpCodes.Ldarg_1 &&
					// The vanilla method would load 0f
					instructionList[i + 1].LoadsConstant(0f) &&
					// The vanilla method would load MaxLevel
					instructionList[i + 2].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 3].Calls(Refs.get_MaxLevel) &&
					// The vanilla method would call Clamp(newValue, 0f, MaxLevel)
					instructionList[i + 4].Calls(Refs.m_Clamp) &&
					// There is one more instruction after that
					i < instructionList.Count - 5))
					continue;

				// Enter patch
				state = 1;
				// 1st: if (newValue <= f_curLevelInt) (not increasing)
				// Load newValue
				yield return new CodeInstruction(OpCodes.Ldarg_1);
				// Load f_curLevelInt
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldfld, Refs.f_curLevelInt);
				// Skip if (newValue <= f_curLevelInt) (not increasing)
				yield return new CodeInstruction(OpCodes.Ble_S, floorLabel);

				// 2nd: if (newValue <= MaxLevel) (not overflowing vanilla max)
				// Load newValue
				yield return new CodeInstruction(OpCodes.Ldarg_1);
				// Load MaxLevel
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_MaxLevel);
				// Skip if (newValue <= MaxLevel) (not overflowing)
				yield return new CodeInstruction(OpCodes.Ble_S, finishLabel);

				// 3rd: Both increasing and overflowing
				// -> need clamping by adjusted max
				// Load the adjusted MaxLevel value
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, m_Adjusted_MaxLevel);
				// Get Min(newValue, adjusted MaxLevel)
				yield return new CodeInstruction(OpCodes.Call, Refs.m_Min);
				// Skip past zero flooring
				yield return new CodeInstruction(OpCodes.Br_S, finishLabel);

				// 4th: Not increasing
				// Need clamping by 0 min
				yield return new CodeInstruction(OpCodes.Ldc_R4, 0f).WithLabels(floorLabel);
				yield return new CodeInstruction(OpCodes.Call, Refs.m_Max);

				// Done, this is the instruction after everything is done
				yield return instructionList[i + 5].WithLabels(finishLabel);

				// Then skip the original methods
				//   as well as the original line after done
				i += 5;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
