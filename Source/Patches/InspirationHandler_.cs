using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.InspirationHandler_
{
	using static Utility;
	using Needs;
	public static class StartInspirationMTBDays
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(InspirationHandler)
			.Getter("StartInspirationMTBDays");
		private static readonly TransIL transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
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
				yield return codeInstruction;
				if (codeInstruction.Calls(get_CurLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, m_Min);
				}
			}
			Debug.CheckTranspiler(state, state > 0);
		}
	}
}
