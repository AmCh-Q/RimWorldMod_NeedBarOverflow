using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace NeedBarOverflow.Patches
{
    // In many parts of the game (especiall when drawing UI), the game expects Need.CurLevelPercentage to be between 0-1
    // If Need.CurLevelPercentage > 1, the UI may be drawn out of the box
    // This patch inserts a Mathf.Min(value,1f) call after Need.CurLevelPercentage to correct that

    using static Utility;
	public static class Add1UpperBound
	{
		public static readonly
			TransIL transpiler = Transpiler;
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (state == 0 && codeInstruction.Calls(get_CurLevelPercentage))
				{
					state = 1;
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, m_Min);
				}
			}
			Debug.CheckTranspiler(state, 1);
		}
	}
}
