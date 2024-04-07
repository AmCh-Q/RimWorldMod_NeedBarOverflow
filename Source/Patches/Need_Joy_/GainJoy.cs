using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Joy_
{
	using static Utility;
	using Needs;
	using System;

	public static class GainJoy
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Joy)
			.Method(nameof(Need_Joy.GainJoy));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_Joy)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// In this case, we've reached the portion of code to patch
				if (state == 0 && i >= 1 && i < instructionList.Count - 4 &&// Haven't Patched yet, and not at the end of instructions
					instructionList[i - 1].opcode == OpCodes.Ldarg_1 &&		// Vanilla would load amount of joy to add
					codeInstruction.LoadsConstant(1d) &&					// Vanilla would load const 1f
					instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 2].Calls(get_CurLevel) &&			// Vanilla would get CurLevel
					instructionList[i + 3].opcode == OpCodes.Sub &&			// Vanilla would calculate 1f - CurLevel
					instructionList[i + 4].Calls(m_Min))					// Vanilla would calculate Min(amount, 1f - CurLevel)
				{
					state = 1;
					// Load the setting max joy instead of 1f
					// So that Vanilla will calculate Min(amount, MaxJoy - CurLevel) instead
					yield return new CodeInstruction(OpCodes.Call, ((Func<float>)NeedSetting<Need_Joy>.MaxValue).Method);
					// Skip the load Constant
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
