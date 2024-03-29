using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using RimWorld;

namespace NeedBarOverflow.Patches.Need_
{
	using static Utility;
	public static class DrawOnGUI
	{
		public static HarmonyPatchType? patched;
		public static readonly MethodBase original
			= typeof(Need)
			.Method(nameof(Need.DrawOnGUI));
		private static readonly Func<bool> prefix = Prefix;
		private static readonly TransILG transpiler = Transpiler;
		public static void Toggle()
			=> Toggle(PatchApplier.s.AnyPatchEnabled);
		public static void Toggle(bool enable)
		{
			if (enable)
				Patch(ref patched, original: original,
					prefix: prefix,
					transpiler: transpiler);
			else
				Unpatch(ref patched, original: original);
		}
		private static bool Prefix() => Event.current.type != EventType.Layout;
		// Need.DrawOnGUI usually expects the need level percentage to be between 0 and 1
		//   and may overflow otherwise
		// This patch fixes the visuals
		// It also force inserts extra tick marks per 100% level
		//   for needs with 1 unit as the Vanilla max
		// When it comes to food, it inserts a tick mark per 1 unit of food

		// This patch is very long and hard to read, unfortunately
		// I've added many comments explaining each step
		// The patch needs to handle different food cases,
		//   since the food bar would be shorter if MaxLevel< 1 unit of nutrition:
		//	 Case 1: MaxLevel >= CurLevel
		//	 Case 1.1: 1 unit of nutrition > MaxLevel >= CurLevel
		//	 Case 1.2: 1 unit of nutrition <= MaxLevel and CurLevel <= MaxLevel
		//	 Case 2: MaxLevel < CurLevel and CurLevel > 1 unit of nutrition
		//	 Case 2.1: MaxLevel < 1 unit of nutrition < CurLevel
		//	 Case 2.2: 1 unit of nutrition <= MaxLevel < CurLevel
		//	 Case 3: MaxLevel < CurLevel <= 1 unit of nutrition
		private static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			MethodInfo
				m_DrawBarThreshold = typeof(Need).Method("DrawBarThreshold"),
				m_DrawBarInstantMarkerAt = typeof(Need).Method("DrawBarInstantMarkerAt");
			FieldInfo
				f_needDef = typeof(Need).Field(nameof(Need.def)),
				f_scaleBar = typeof(NeedDef).Field(nameof(NeedDef.scaleBar));
			ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
			int state = 0;
			Label[] jumpLabels = new Label[4];
			for (int i = 0; i < 4; i++)
				jumpLabels[i] = ilg.DefineLabel();
			object num4Idx = 5;
			LocalBuilder max = ilg.DeclareLocal(typeof(float));
			LocalBuilder cur = ilg.DeclareLocal(typeof(float));
			LocalBuilder mult = ilg.DeclareLocal(typeof(float));
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i > 0 &&
					instructionList[i - 1].LoadsConstant(1d) &&
					codeInstruction.opcode == OpCodes.Stloc_S) // The first 1d constant should be assigned to num4
				{
					// Stage 1: Obtain the local index of the local variable "num4"
					// float num4 = 1f;
					state = 1;
					// It should be 5 but just in case
					num4Idx = codeInstruction.operand;
					yield return codeInstruction;
					continue;
				}
				if (state == 1 && i < instructionList.Count - 2 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 1].LoadsField(f_needDef) &&
					instructionList[i + 2].LoadsField(f_scaleBar))
				{
					// Stage 2: Calculate the shrink factor "mult" for the drawn bar, and get the max between MaxLevel and CurLevel
					// if (def.scaleBar && MaxLevel < 1f)
					state = 2;
					//0								#Consumed at #Pop or #End
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);

					// Case 1: MaxLevel >= CurLevel, skip and set mult to 1f
					//1	float max = n.MaxLevel;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_MaxLevel);
					yield return new CodeInstruction(OpCodes.Dup);  // consumed at #Bge_Un_S
					yield return new CodeInstruction(OpCodes.Stloc_S, max.LocalIndex);
					//2	float cur = n.CurLevel;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevel);
					yield return new CodeInstruction(OpCodes.Dup);  // consumed at #Bge_Un_S
					yield return new CodeInstruction(OpCodes.Stloc_S, cur.LocalIndex);
					//3	if (max < cur)				#Bge_Un_S
					yield return new CodeInstruction(OpCodes.Bge_Un_S, jumpLabels[0]);

					// Here means MaxLevel < CurLevel, overflowing
					// Case 2 or 3
					//1								#Pop
					yield return new CodeInstruction(OpCodes.Pop);
					//0		tmp1 = max / cur;
					yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex);
					//2								consumed at #Mul
					yield return new CodeInstruction(OpCodes.Div);

					// Two possible cases here:
					// Case 2: 1f < CurLevel and MaxLevel < CurLevel
					// Case 3: MaxLevel < CurLevel <= 1f
					//1		if (1f < cur)
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex);
					yield return new CodeInstruction(OpCodes.Bge_Un_S, jumpLabels[1]);
					//1			tmp2 = max;			consumed at #Mul
					yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
					yield return new CodeInstruction(OpCodes.Br_S, jumpLabels[2]);
					//1		else
					//			tmp2 = tmp1;		consumed at #Mul
					yield return new CodeInstruction(OpCodes.Dup).WithLabels(jumpLabels[1]);

					// ResetHediff max to the real maximum, which is CurLevel
					//2		max = cur;
					yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex).WithLabels(jumpLabels[2]);
					yield return new CodeInstruction(OpCodes.Stloc_S, max.LocalIndex);

					// Case 1: mult = 1f
					// Case 2: mult = MaxLevel * MaxLevel / CurLevel
					// Case 3: mult = MaxLevel * MaxLevel / CurLevel / CurLevel
					//2		tmp1 *= tmp2;			#Mul
					yield return new CodeInstruction(OpCodes.Mul);
					//1		mult = tmp1;			#End
					yield return new CodeInstruction(OpCodes.Stloc_S, mult.LocalIndex).WithLabels(jumpLabels[0]);
					//0	Done
					yield return codeInstruction;
					continue;
				}
				if (state > 1 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 && 
					instructionList[i + 1].Calls(get_MaxLevel))
				{
					// Stage 2+
					// Once max is calculated, whenever accessing MaxLevel;
					// Instead of directly getting original MaxLevel
					// Get the real max, which is MaxLevel in Case 1 and CurLevel in Case 2 or 3
					yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
					i++;
					continue;
				}
				if (state > 1 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 && 
					instructionList[i + 1].Calls(get_CurLevelPercentage))
				{
					// Stage 2+
					// Once max is calculated, whenever accessing CurLevelPercentage;
					// Instead of directly getting original CurLevelPercentage
					// Get the apparent percentage, which is CurLevel/MaxLevel in Case 1 and 1f in Case 2 or 3
					yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
					yield return new CodeInstruction(OpCodes.Div);
					i++;
					continue;
				}
				if (state == 2 &&
					codeInstruction.opcode == OpCodes.Ldloc_S &&
					codeInstruction.OperandIs(num4Idx))
				{
					// Stage 3
					state = 3;
					// By vanilla code and stage2+, num4 = min(max, 1f)
					// We multiply the result into mult:
					// Case 1.1: num4 = MaxLevel, mult = MaxLevel
					// Case 1.2: num4 = 1f	  , mult = 1f
					// Case 2:   num4 = 1f	  , mult = MaxLevel * MaxLevel / CurLevel
					// Case 3:   num4 = CurLevel, mult = MaxLevel * MaxLevel / CurLevel
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldloc_S, mult.LocalIndex);
					yield return new CodeInstruction(OpCodes.Mul);
					yield return new CodeInstruction(OpCodes.Stloc_S, mult.LocalIndex);
					continue;
				}
				if ((state == 3 || state == 5) && i < instructionList.Count - 2 &&
					codeInstruction.opcode == OpCodes.Ldloc_S &&
					codeInstruction.OperandIs(num4Idx) &&
					instructionList[i + 1].opcode == OpCodes.Mul &&
					(instructionList[i + 2].Calls(m_DrawBarThreshold) ||
					instructionList[i + 2].Calls(m_DrawBarInstantMarkerAt)))
				{
					// Stage 4 and Stage 6
					state++;
					// When drawing bars & markers, replace access to num4 with mult
					// Note that percentages are basically "value / MaxLevel"
					// Case 1.1: mult = MaxLevel					  , drawn = value
					// Case 1.2: mult = 1f							, drawn = value / MaxLevel		   <= value
					// Case 2:   mult = MaxLevel * MaxLevel / CurLevel, drawn = value * MaxLevel / CurLevel < value
					// Case 3:   mult = MaxLevel * MaxLevel / CurLevel, drawn = value * MaxLevel / CurLevel < value
					//	m_DrawBarThreshold.Invoke(n, new object[2] { barRect, threshPercents[i] * mult });
					//	m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.ModifyClamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Ldloc_S, mult.LocalIndex);
					continue;
				}
				if (state == 4 && i < instructionList.Count - 4 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 1].LoadsField(f_needDef) &&
					instructionList[i + 2].opcode == OpCodes.Ldfld &&
					instructionList[i + 3].opcode == OpCodes.Brfalse_S)
				{
					// Stage 5
					state = 5;
					// Most needs (other than food) has a MaxLevel of 1f, their showUnitTicks would be false
					// We force showUnitTicks to be true in that case
					//	if (n.def.scaleBar && max < 1f)
					//  if (def.showUnitTicks)
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_MaxLevel);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Beq_S, jumpLabels[3]);
					// If MaxLevel == 1f, jump straight into the draw unit ticks section
					yield return codeInstruction;
					yield return instructionList[i + 1];
					yield return instructionList[i + 2];
					yield return instructionList[i + 3];
					yield return instructionList[i + 4].WithLabels(jumpLabels[3]);
					i += 4;
					continue;
				}
				if (state == 6 && i > 1 &&
					instructionList[i - 1].opcode == OpCodes.Mul &&
					codeInstruction.Calls(m_DrawBarInstantMarkerAt))
				{
					// Stage 7
					state = 7;
					//  When drawing instant markers, it is possible that curInstantLevelPercentage > CurLevel
					//	resulting in drawn > 1, so we cap it
					//	m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.ModifyClamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, m_Min);
					yield return codeInstruction;
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 7);
		}
	}
}
