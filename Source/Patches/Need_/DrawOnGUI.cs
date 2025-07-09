using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;
using NeedBarOverflow.Needs;

namespace NeedBarOverflow.Patches
{
	[StaticConstructorOnStartup]
	public sealed class Need_DrawOnGUI() : Patch_Single(
		original: typeof(Need).Method(nameof(Need.DrawOnGUI)),
		prefix: PrefixMethod)
	{
		static Need_DrawOnGUI()
		{
#if l1_3 // 1.2 - 1.3 has this field as protected
			BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
#endif
			BarOverflowTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f, 0.75f));
		}

		public override void Toggle()
			=> Toggle(Setting_Common.AnyEnabled);

#if DrawOnGUI_UseTranspiler
		private static bool PrefixMethod()
			=> Event.current.type != EventType.Layout;
#endif

		// Need.DrawOnGUI usually expects the need level percentage to be between 0 and 1
		//   and may overflow otherwise
		// This patch fixes the visuals
		// It also force inserts extra tick marks per 100% level
		//   for needs with 1 unit as the Vanilla max
		// When it comes to food, it inserts a tick mark per 1 unit of food
		public static bool PrefixMethod(
			Need __instance,
			Rect rect,
			int maxThresholdMarkers,
			float customMargin,
			bool drawArrows,
			bool doTooltip
#if g1_3
			, Rect? rectForTooltip
#endif
#if g1_4
			, bool drawLabel
#endif
			)
		{
			// (Custom) Skip unnecessary draws
			if (Event.current.type == EventType.Layout)
				return false;

			// (Vanilla 1.2+) Adjust to max height
			if (rect.height > Need.MaxDrawHeight)
			{
				rect.height = Need.MaxDrawHeight;
				rect.y += (rect.height - Need.MaxDrawHeight) / 2f;
			}

			// (Vanilla 1.2+, separated) DrawHighlight
			{
#if l1_2
				Rect rectForTooltip = rect;
#else
				rectForTooltip ??= rect; // Vanilla: rect2
#endif
				if (Mouse.IsOver((Rect)rectForTooltip))
					DrawHighlight(__instance, doTooltip, (Rect)rectForTooltip);
			}

			// (Vanilla 1.2+, reordered) Set margins
			float verticalMargin = 14f; // Vanilla: num2
			if (rect.height < 50f)
				verticalMargin *= Mathf.InverseLerp(0f, 50f, rect.height);
			customMargin = ((customMargin >= 0f) ? customMargin : 29f); // Vanilla: num3
			Rect unShrunkRect = new( // Vanilla: rect3
				rect.x + customMargin,
				rect.y,
				rect.width - customMargin * 2f,
				rect.height - verticalMargin);

#if g1_4    // (Vanilla 1.2+, reordered) Draw labels
			if (drawLabel) // Always draw in 1.2-1.3
#endif
			{
				Text.Font = ((rect.height > 55f) ? GameFont.Small : GameFont.Tiny);
				Text.Anchor = TextAnchor.LowerLeft;
				Rect labelRect = new(
					rect.x + customMargin + rect.width * 0.1f,
					rect.y,
					rect.width - customMargin - rect.width * 0.1f,
					rect.height / 2f);
				Widgets.Label(labelRect, __instance.LabelCap);
				Text.Anchor = TextAnchor.UpperLeft;
				unShrunkRect.y += rect.height / 2f;
				unShrunkRect.height -= rect.height / 2f;
			}

#if l1_3    // (Vanilla 1.4+, separated, down supported to 1.2+) ShowDevGizmos
			if (Prefs.DevMode && DebugSettings.godMode)
#else
			if (DebugSettings.ShowDevGizmos)
#endif
				ShowDevGizmos(__instance, unShrunkRect);

			// (Custom) Get some common fields
			float maxLevel = __instance.MaxLevel;
			float curLevel = __instance.CurLevel;
			float curLevelPercentage = curLevel / maxLevel;

			// (Custom) Calculate some values
			Rect shrunkRect = unShrunkRect; // Vanilla: rect6
			float barShrinkFactor = 1f; // Vanilla: num4
			float prcntShrinkFactor = 1f; // New
			if (curLevel > maxLevel)
			{
				prcntShrinkFactor = maxLevel / curLevel;
				maxLevel = curLevel;
			}
			NeedDef def = __instance.def;
			if (maxLevel < 1f && def.scaleBar)
				barShrinkFactor = maxLevel;
			shrunkRect.width *= barShrinkFactor;

			// (Vanilla 1.2+, replaced) Draw fillable bar
			Rect barRect = FillableBar(shrunkRect, curLevelPercentage);

			// (Vanilla 1.2+) Draw arrows
			if (drawArrows)
				Widgets.FillableBarChangeArrows(shrunkRect, __instance.GUIChangeArrow);

			// (Vanilla 1.2+) Draw threshold percents
			List<float> threshPercents = fr_threshPercents(__instance);
			if (threshPercents != null)
			{
				int drawCount = Mathf.Min(threshPercents.Count, maxThresholdMarkers);
				for (int i = 0; i < drawCount; i++)
					d_DrawBarThreshold(__instance, barRect, threshPercents[i] * prcntShrinkFactor);
			}

			// (Vanilla 1.2+) Draw unit ticks
			// Removed checks (always runs)
			// I don't know why Vanilla's markers aren't perfectly aligned
			// So I need to shift x a little
			//#if l1_3
			//			if (def.scaleBar)
			//#else
			//			if (def.showUnitTicks)
			//#endif
			{
				barRect.x += 2f;
				for (float j = 1f, step = 1f; j < maxLevel; j += step)
				{
					if (j >= maxLevel * 0.0078125f)
						d_DrawBarDivision(__instance, barRect, j / maxLevel);
					if (j >= step * 10f)
						step *= 10.0f;
				}
				barRect.x -= 2f;
			}

			// (Vanilla 1.2+) Draw instant markers
			float curInstantLevelPercentage = __instance.CurInstantLevelPercentage;
			if (curInstantLevelPercentage >= 0f)
				d_DrawBarInstantMarkerAt(__instance, shrunkRect, curInstantLevelPercentage * prcntShrinkFactor);

			// (Vanilla 1.2+) Draw tutorial highlights
			if (!def.tutorHighlightTag.NullOrEmpty())
				UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);

			Text.Font = GameFont.Small;
			return false;
		}

#if v1_2 // 1.2 had class "TexButton" as "internal", so aquire it (too small, not worth reflection)
		public static Texture2D
			Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus"),
			Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");
#endif
#if l1_3 // 1.2 - 1.3 has this field as protected
		public static Texture2D BarFullTexHor;
#endif  // Lower transparency but otherwise the same
		public static Texture2D BarOverflowTexHor;

#if g1_5 && DEBUG // Won't use this if not debugging
		public static Action<Need, float> d_OffsetDebugPercent
			= (Action<Need, float>)Delegate.CreateDelegate(
				typeof(Action<Need, float>),
				typeof(Need).Method("OffsetDebugPercent")
			);
#endif
		public static AccessTools.FieldRef<Need, List<float>>
			fr_threshPercents
			= AccessTools.FieldRefAccess<Need, List<float>>
			(typeof(Need).Field("threshPercents"));

		public static Action<Need, Rect, float>
			d_DrawBarThreshold
			= (Action<Need, Rect, float>)
			Delegate.CreateDelegate(
				typeof(Action<Need, Rect, float>),
				typeof(Need).Method("DrawBarThreshold")),
			d_DrawBarDivision
			= (Action<Need, Rect, float>)
			Delegate.CreateDelegate(
				typeof(Action<Need, Rect, float>),
				typeof(Need).Method("DrawBarDivision")),
			d_DrawBarInstantMarkerAt
			= (Action<Need, Rect, float>)
			Delegate.CreateDelegate(
				typeof(Action<Need, Rect, float>),
				typeof(Need).Method("DrawBarInstantMarkerAt"));

		// Same implementation in Vanilla's code in DrawOnGUI
		// Just split out for readability
		public static void DrawHighlight(Need __instance, bool doTooltip, Rect highlightRect)
		{
			Widgets.DrawHighlight(highlightRect);
			if (!doTooltip)
				return;
			TooltipHandler.TipRegion(highlightRect,
				new TipSignal(__instance.GetTipString, highlightRect.GetHashCode()));
		}

		// Helper function (1) for ShowDevGizmos()
		public static float GetDevGizmosFloat()
		{
			bool ctrl = Helpers.CtrlDown;
			bool shift = Helpers.ShiftDown;
			if (shift && !ctrl)
				return 1f;
			if (ctrl && !shift)
				return 0.01f;
			return 0.1f;
		}

		// Helper function (2) for ShowDevGizmos()
		public static string GetDevGizmosStr(bool add)
		{
			bool ctrl = Helpers.CtrlDown;
			bool shift = Helpers.ShiftDown;
			if (shift && !ctrl)
				return add ? "+ 100%" : "- 100%";
			if (ctrl && !shift)
				return add ? "+ 1%" : "- 1%";
			return add ? "+ 10%" : "- 10%";
		}

		// Same implementation in Vanilla's code in DrawOnGUI
		// Just split out for readability
		public static void ShowDevGizmos(Need __instance, Rect rect3)
		{
			float lineHeight = Text.LineHeight;
			Rect plusRect = new( // Vanilla: rect4
				rect3.xMax - lineHeight,
				rect3.y - lineHeight,
				lineHeight, lineHeight);

#if v1_2
			if (Widgets.ButtonImage(plusRect.ContractedBy(4f), Plus))
#else
			if (Widgets.ButtonImage(plusRect.ContractedBy(4f), TexButton.Plus))
#endif
			{
#if l1_4 || !DEBUG
				__instance.CurLevelPercentage += GetDevGizmosFloat();
#else           // Vanilla does above anyways, so use above if not debugging
				d_OffsetDebugPercent(__instance, GetDevGizmosFloat());
#endif
			}
			if (Mouse.IsOver(plusRect))
				TooltipHandler.TipRegion(plusRect, GetDevGizmosStr(true));

			Rect minusRect = new( // Vanilla: rect5
				plusRect.xMin - lineHeight,
				rect3.y - lineHeight,
				lineHeight, lineHeight);
#if v1_2
			if (Widgets.ButtonImage(minusRect.ContractedBy(4f), Minus))
#else
			if (Widgets.ButtonImage(minusRect.ContractedBy(4f), TexButton.Minus))
#endif
			{
#if l1_4 || !DEBUG
				__instance.CurLevelPercentage -= GetDevGizmosFloat();
#else           // Vanilla does above anyways, so use above if not debugging
				d_OffsetDebugPercent(__instance, -GetDevGizmosFloat());
#endif
			}
			if (Mouse.IsOver(minusRect))
				TooltipHandler.TipRegion(minusRect, GetDevGizmosStr(false));
		}

		// A replacement implementation of Widgets.FillableBar
		// In order to handle need overflows
		public static Rect FillableBar(Rect rect, float curLevelPercentage)
		{
			Rect shrunkRect = rect;
			bool doBorder = rect.height > 15f && rect.width > 20f;
			if (doBorder)
			{
				GUI.DrawTexture(rect, BaseContent.BlackTex);
				shrunkRect = rect.ContractedBy(3f);
			}
			bool drawOverflow = curLevelPercentage > 1f;
			// Draw non-overflowing part of the bar
			float fillWidth = shrunkRect.width;
			if (curLevelPercentage < 1f)
				fillWidth *= curLevelPercentage;
			else if (drawOverflow)
				fillWidth /= curLevelPercentage;
			Rect fillRect = shrunkRect;
			fillRect.width = fillWidth;
#if l1_3
			GUI.DrawTexture(fillRect, BarFullTexHor);
#else
			GUI.DrawTexture(fillRect, Widgets.BarFullTexHor);
#endif
			// If no draw overflow & already drawn border, skip
			if (!drawOverflow && doBorder)
				return shrunkRect;
			fillRect.x += fillWidth;
			fillRect.width = shrunkRect.width - fillWidth;
			GUI.DrawTexture(fillRect, drawOverflow ? BarOverflowTexHor : BaseContent.BlackTex);
			return shrunkRect;
		}

#if DrawOnGUI_UseTranspiler
		// Need.DrawOnGUI usually expects the need level percentage to be between 0 and 1
		//   and may overflow otherwise
		// This patch fixes the visuals
		// It also force inserts extra tick marks per 100% level
		//   for needs with 1 unit as the Vanilla max
		// When it comes to food, it inserts a tick mark per 1 unit of food
		//
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
		private static IEnumerable<CodeInstruction> TranspilerMethod(
			IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			MethodInfo
				m_DrawBarThreshold = typeof(Need).Method("DrawBarThreshold")!,
				m_DrawBarInstantMarkerAt = typeof(Need).Method("DrawBarInstantMarkerAt")!;
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
#if v1_4
				if (state == 0 && i > 0 && i < instructionList.Count - 2 &&
					instructionList[i - 1].Calls(Refs.get_CurLevelPercentage) &&
					codeInstruction.LoadsConstant(0.1d) &&
					(instructionList[i + 1].opcode == OpCodes.Add ||
					instructionList[i + 1].opcode == OpCodes.Sub) &&
					instructionList[i + 2].Calls(Refs.set_CurLevelPercentage))
				{
					yield return new CodeInstruction(OpCodes.Call, ((Func<float>)GetDevGizmosFloat).Method);
					continue;
				}
#endif
#if g1_4
				if (codeInstruction.opcode == OpCodes.Ldstr &&
					codeInstruction.operand.Equals("+ 10%"))
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
					yield return new CodeInstruction(OpCodes.Call, ((Delegate)GetDevGizmosStr).Method);
					continue;
				}
				if (codeInstruction.opcode == OpCodes.Ldstr &&
					codeInstruction.operand.Equals("- 10%"))
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4_0);
					yield return new CodeInstruction(OpCodes.Call, ((Delegate)GetDevGizmosStr).Method);
					continue;
				}
#endif
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
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_MaxLevel);
					yield return new CodeInstruction(OpCodes.Dup);  // consumed at #Bge_Un_S
					yield return new CodeInstruction(OpCodes.Stloc_S, max.LocalIndex);
					//2	float cur = n.CurLevel;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_CurLevel);
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

					// Reset max to the real maximum, which is CurLevel
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
					instructionList[i + 1].Calls(Refs.get_MaxLevel))
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
					instructionList[i + 1].Calls(Refs.get_CurLevelPercentage))
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
				if ((state == 3 || state == 6) && i < instructionList.Count - 2 &&
					codeInstruction.opcode == OpCodes.Ldloc_S &&
					codeInstruction.OperandIs(num4Idx) &&
					instructionList[i + 1].opcode == OpCodes.Mul &&
					(instructionList[i + 2].Calls(m_DrawBarThreshold) ||
					instructionList[i + 2].Calls(m_DrawBarInstantMarkerAt)))
				{
					// Stage 4 and Stage 7
					state++;
					// When drawing bars & markers, replace access to num4 with mult
					// Note that percentages are basically "value / MaxLevel"
					// Case 1.1: mult = MaxLevel					  , drawn = value
					// Case 1.2: mult = 1f							  , drawn = value / MaxLevel		   <= value
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
					yield return new CodeInstruction(OpCodes.Callvirt, Refs.get_MaxLevel);
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
				if (state == 5 && i > 0 &&
					instructionList[i - 2].opcode == OpCodes.Ldarg_0 &&
					instructionList[i - 1].Calls(Refs.get_MaxLevel) &&
					codeInstruction.opcode == OpCodes.Blt_S)
				{
					// Stage 6
					state = 6;
					// Limit the number of showUnitTicks to 10 max
					yield return new CodeInstruction(OpCodes.Ldc_R4, 11f);
					yield return new CodeInstruction(OpCodes.Call, Refs.m_Min);
					yield return codeInstruction;
					continue;
				}
				if (state == 7 && i > 1 &&
					instructionList[i - 1].opcode == OpCodes.Mul &&
					codeInstruction.Calls(m_DrawBarInstantMarkerAt))
				{
					// Stage 8
					state = 8;
					//  When drawing instant markers, it is possible that curInstantLevelPercentage > CurLevel
					//	resulting in drawn > 1, so we cap it
					//	m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.ModifyClamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, Refs.m_Min);
					yield return codeInstruction;
					continue;
				}
				yield return codeInstruction;
			}
			Debug.CheckTranspiler(state, 8);
		}
#endif
	}
}
