#if !v1_2
using HarmonyLib;
using NeedBarOverflow.Needs;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NeedBarOverflow.Patches
{
	public sealed class Need_Suppression_DrawSuppressionBar() : Patch_Single(
		original: typeof(Need_Suppression)
			.Method(nameof(Need_Suppression.DrawSuppressionBar)),
		transpiler: TranspilerMethod)
	{
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Suppression)));
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			MethodInfo m_DrawBarThreshold
				= typeof(Need).Method("DrawBarThreshold");
			ReadOnlyCollection<CodeInstruction> instructionList
				= Add1UpperBound.TranspilerMethod(instructions).ToList().AsReadOnly();
			int state = 0;
			Label end = ilg.DefineLabel();
			LocalBuilder perc = ilg.DeclareLocal(typeof(float));
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i > 0 &&
					instructionList[i - 1].Calls(Refs.get_CurLevelPercentage))
				{
					state = 1;
					// Optain the shrink factor for the SuppressionBar
					// perc = 1f / Mathf.Max(1f, CurLevelPercentage)
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, Refs.m_Max);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Div);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
				}
				yield return codeInstruction;
				if ((state == 1 || state == 2) &&
					i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Ldc_R4 &&
					instructionList[i + 1].Calls(m_DrawBarThreshold))
				{
					state++;
					// Shrink the two bars my multipling perc
					// The top of the stack is the Vanilla SuppressionBar percentage
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Mul);
				}
				if (state == 3 && i < instructionList.Count - 1 &&
					codeInstruction.Calls(m_DrawBarThreshold))
				{
					// After drawing the two shrunken bars
					state = 4;
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ble_S, end); // Skip if not overflowing

					// Draw an additional suppression bar if > 1f
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, m_DrawBarThreshold);

					// Add ending label to the instruction after everything's done
					i++;
					yield return instructionList[i].WithLabels(end);
				}
			}
			Debug.CheckTranspiler(state, 4);
		}
	}
}
#endif
