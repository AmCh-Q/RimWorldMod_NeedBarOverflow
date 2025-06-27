using HarmonyLib;
using NeedBarOverflow.Needs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches.ModCompat
{
	// Character Editor
	// https://steamcommunity.com/sharedfiles/filedetails/?id=1874644848
	public sealed class Character_Editor_AFullNeeds() : Patch_Single(
#if v1_5
		// Version 1.5 only -- Mod used obfuscated method name
		original: Helpers
			.TypeByName("CharacterEditor.CEditor+EditorUI+f")?
			.MethodNullable(name: "a", parameters: []),
#else
		original: Helpers
			.TypeByName("CharacterEditor.CEditor+EditorUI+BlockNeeds")?
			.MethodNullable(name: "AFullNeeds", parameters: []),
#endif
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
		private static float GetMax()
			=> Helpers.ShiftDown ? 10f : 1f;
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Ldc_R4 &&
					codeInstruction.operand.Equals(1.0f) &&
					instructionList[i + 1].Calls(Refs.set_CurLevelPercentage))
				{
					yield return new CodeInstruction(OpCodes.Call, ((Delegate)GetMax).Method);
					continue;
				}
				yield return codeInstruction;
			}
		}
	}
}
