using HarmonyLib;
using NeedBarOverflow.Patches;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace NeedBarOverflow.ModCompat;

// Character Editor
// https://steamcommunity.com/sharedfiles/filedetails/?id=1874644848
public sealed class Character_Editor_AAddNeed() : Patches.Patch(
	transpiler: TranspilerMethod)
{
	protected override void Initialize()
	{
		if (Target is not null)
			return;
		if (!ModsConfig.IsActive("void.charactereditor"))
			return;
#if v1_5
		// Version 1.5 only -- Mod used obfuscated method name
		Type? type = GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+f");
		if (type is null)
		{
			Debug.Warning("CharacterEditor.CEditor+EditorUI+f is null");
			return;
		}
		Target =
		[
			type.Method(name: "a", parameters: [typeof(Need)]),
			type.Method(name: "b", parameters: [typeof(Need)]),
		];
#else
		Type? type = GenTypes.GetTypeInAnyAssembly("CharacterEditor.CEditor+EditorUI+BlockNeeds");
		if (type is null)
		{
			Debug.Warning("CharacterEditor.CEditor+EditorUI+f is null");
			return;
		}
		Target =
		[
			type.Method(name: "AAddNeed", parameters: [typeof(Need)]),
			type.Method(name: "ASubNeed", parameters: [typeof(Need)]),
		];
#endif
	}

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
