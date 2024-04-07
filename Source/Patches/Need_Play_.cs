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
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Common.Enabled(typeof(Need_Play)));
        public static void Toggle(bool enabled)
		{
			if (enabled)
				Patch(ref patched, original: original,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
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
					i += 4;
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}