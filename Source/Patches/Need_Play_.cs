#if !v1_2 && !v1_3
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_Play_
{
	using static Utility;
	using Needs;
	public static class Play
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need_Play)
			.Method(nameof(Need_Play.Play));
		private static readonly TransILG transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Play)));
		public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			Label jumpLabel = ilg.DefineLabel();
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				// Skip the Clamp part
				if (state == 0 && i < instructionList.Count - 5 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 2].Calls(get_CurLevelPercentage) &&
					instructionList[i + 3].Calls(m_Clamp01) &&
					instructionList[i + 4].Calls(set_CurLevelPercentage))
				{
					state = 1;
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Call, m_CanOverflow);
					yield return new CodeInstruction(OpCodes.Brtrue_S, jumpLabel);
					yield return instructionList[i + 1];
					yield return new CodeInstruction(OpCodes.Dup);
					yield return instructionList[i + 2];
					yield return instructionList[i + 3];
					yield return instructionList[i + 4];
					yield return instructionList[i + 5].WithLabels(jumpLabel);
					i += 5;
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
#endif