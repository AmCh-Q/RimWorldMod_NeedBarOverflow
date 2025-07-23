using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using RimWorld;
using NeedBarOverflow.Needs;
using NeedBarOverflow.Patches;

namespace NeedBarOverflow.ModCompat
{
	// Character Editor
	// https://steamcommunity.com/sharedfiles/filedetails/?id=1874644848
	public sealed class Character_Editor_AAddNeed() : Patch_Multi(
#if v1_5
		// Version 1.5 only -- Mod used obfuscated method name
		original: ModsConfig.IsActive("void.charactereditor") ?
		[
			GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+f")?
			.MethodNullable(name: "a", parameters: [typeof(Need)]),
			GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+f")?
			.MethodNullable(name: "b", parameters: [typeof(Need)])
		] : [],
#else
		original: ModsConfig.IsActive("void.charactereditor") ?
		[
			GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+BlockNeeds")?
			.MethodNullable(name: "AAddNeed", parameters: [typeof(Need)]),
			GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+BlockNeeds")?
			.MethodNullable(name: "ASubNeed", parameters: [typeof(Need)])
		] : [],
#endif
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);
		private static float GetOffset()
		{
			bool ctrl = Helpers.CtrlDown;
			bool shift = Helpers.ShiftDown;
			if (shift && !ctrl)
				return 1f;
			if (ctrl && !shift)
				return 0.01f;
			return 0.1f;
		}
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (i < instructionList.Count - 2 &&
					codeInstruction.opcode == OpCodes.Ldc_R4 &&
					codeInstruction.operand.Equals(0.05f) &&
					instructionList[i + 2].Calls(Refs.set_CurLevelPercentage))
				{
					yield return new CodeInstruction(OpCodes.Call, ((Delegate)GetOffset).Method);
					continue;
				}
				yield return codeInstruction;
			}
		}
	}
}
