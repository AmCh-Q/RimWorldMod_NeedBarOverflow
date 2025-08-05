#if g1_4
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_KillThirst_Notify_KilledPawn() : Patch_Single(
		original: typeof(Need_KillThirst)
			.Method(nameof(Need_KillThirst.Notify_KilledPawn)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_KillThirst)));
		private static IEnumerable<CodeInstruction> TranspilerMethod(
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
					instructionList[i + 1].Calls(Refs.set_CurLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_CurLevel);
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Ldc_I4, (int)StatName_DrainGain.SlowGain);
					yield return new CodeInstruction(OpCodes.Call,
						((Func<int, float>)OverflowStats_DrainGain<Need_KillThirst>.EffectStat).Method);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_CurLevelPercentage);
					yield return new CodeInstruction(OpCodes.Call, AdjustGain.m_adjust);
					yield return new CodeInstruction(OpCodes.Add);
				}
				else
				{
					yield return codeInstruction;
				}
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
#endif
