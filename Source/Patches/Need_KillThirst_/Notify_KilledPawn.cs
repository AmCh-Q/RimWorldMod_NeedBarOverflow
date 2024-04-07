using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_KillThirst_
{
	using static Utility;
	using Needs;
	using System;

	public static class Notify_KilledPawn
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_KillThirst)
			.Method(nameof(Need_KillThirst.Notify_KilledPawn));
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_KillThirst)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
				transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static float GainMultiplier() 
			=> NeedSetting<Need_KillThirst>.EffectStat(Strings.SlowGain);
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// Here Vanilla is setting the need level to 1, we change to adjusted instead
				if (state == 0 && i > 0 && i < instructionList.Count - 1 &&
					instructionList[i - 1].opcode == OpCodes.Ldarg_0 &&
					codeInstruction.LoadsConstant(1f) &&
					instructionList[i + 1].Calls(set_CurLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevel);
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Call, ((Func<float>)GainMultiplier).Method);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevelPercentage);
					yield return new CodeInstruction(OpCodes.Call, AdjustGain.adjust);
					yield return new CodeInstruction(OpCodes.Add);
				}
				else
					yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
