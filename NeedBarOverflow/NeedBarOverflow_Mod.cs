namespace NeedBarOverflow
{
    using HarmonyLib;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
	using System.Linq;
	using UnityEngine;
    using Verse;
    using C = NeedBarOverflow_Consts;
    using S = NeedBarOverflow_Settings;

    public class NeedBarOverflow : Mod
	{
		public static S s;
		private static readonly MethodInfo 
			get_MaxLevel = AccessTools.PropertyGetter(typeof(Need), nameof(Need.MaxLevel)),
			get_CurLevel = AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevel)),
			set_CurLevel = AccessTools.PropertySetter(typeof(Need), nameof(Need.CurLevel)),
			get_CurLevelPercentage = AccessTools.PropertyGetter(typeof(Need), nameof(Need.CurLevelPercentage)),
			set_CurLevelPercentage = AccessTools.PropertySetter(typeof(Need), nameof(Need.CurLevelPercentage)),
			DrawBarThreshold = AccessTools.Method(typeof(Need), "DrawBarThreshold"),
			DrawBarInstantMarkerAt = AccessTools.Method(typeof(Need), "DrawBarInstantMarkerAt"),
			clamp01Method = ((Func<float, float>)Mathf.Clamp01).Method,
			clampMethod = ((Func<float, float, float, float>)Mathf.Clamp).Method,
			maxMethod = ((Func<float, float, float>)Mathf.Max).Method,
			minMethod = ((Func<float, float, float>)Mathf.Min).Method,
			adjustDrain = ((Func<float, Need, int, float>)Adjust_NeedInterval_Drain).Method,
			adjustGain = ((Func<float, Need, int, float>)Adjust_NeedInterval_Gain).Method;
		private static readonly FieldInfo 
			settingsField = typeof(NeedBarOverflow).GetField("s"),
			statsAField = typeof(S).GetField("statsA"),
			needDef = AccessTools.Field(typeof(Need), nameof(Need.def)),
			scaleBarDef = AccessTools.Field(typeof(NeedDef), nameof(NeedDef.scaleBar)),
			needPawnField = AccessTools.Field(typeof(Need), "pawn"),
			curLevelIntField = AccessTools.Field(typeof(Need), "curLevelInt");
		public static HediffDef foodOverflow;
#if DEBUG
		private const string transpilerStateWarning = "[Need Bar Overflow]: Patch {0} is not fully applied (state: {1} < {2})";
#endif
		public NeedBarOverflow(ModContentPack content) : base(content)
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: NeedBarOverflow constructor called");
#endif
			s = GetSettings<S>();
			LongEventHandler.QueueLongEvent(delegate
			{
				foodOverflow = HediffDef.Named("FoodOverflow");
				HediffComp_FoodOverflow.gourmand = TraitDef.Named("Gourmand");
				s.ApplyFoodHediffSettings();
			}, "NeedBarOverflow.Mod.ctor", false, null);
		}
		public static void GenUI_Prefix(ref float pct)
		{
			pct = Mathf.Clamp01(pct);
		}
		/*private static readonly MethodInfo DrawBarThreshold = AccessTools.Method(typeof(Need), "DrawBarThreshold");
		private static readonly MethodInfo DrawBarDivision = AccessTools.Method(typeof(Need), "DrawBarDivision");
		private static readonly MethodInfo DrawBarInstantMarkerAt = AccessTools.Method(typeof(Need), "DrawBarInstantMarkerAt");
		public static void DrawOnGUI_PatchS(Rect rect3, Need n, bool drawArrows, List<float> threshPercents)
        {
			Rect rect6 = rect3;
			float num4 = 1f;
			float mult = 1f;
			float max = n.MaxLevel;
			float cur = n.CurLevel;
			if (max < cur)
			{
				mult = max / cur;
				if (1f < cur)
					mult *= max;
				else
					mult *= mult;
				max = cur;
			}
			if (n.def.scaleBar && max < 1f)
				num4 = max;
			rect6.width *= num4;
			mult *= num4;
			Rect barRect = Widgets.FillableBar(rect6, cur / max);
			if (drawArrows)
				Widgets.FillableBarChangeArrows(rect6, n.GUIChangeArrow);
			if (threshPercents != null)
				for (int i = 0; i < threshPercents.Count; i++)
					DrawBarThreshold.Invoke(n, new object[2] { barRect, threshPercents[i] * mult } );
			if (n.MaxLevel == 1f || n.def.showUnitTicks)
				for (float j = 1; j < max; j++)
					DrawBarDivision.Invoke(n, new object[2] { barRect, j / max * num4 });
			float curInstantLevelPercentage = n.CurInstantLevelPercentage;
			if (curInstantLevelPercentage >= 0f)
				DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
		}*/
		public static IEnumerable<CodeInstruction> DrawOnGUI_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
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
				if (state < 2 && i > 0 &&
					codeInstruction.opcode == OpCodes.Stloc_S &&
					instructionList[i - 1].LoadsConstant(1.0))
				{
					state = 1;
					//	float num4 = 1f;
					num4Idx = codeInstruction.operand;
					yield return codeInstruction;
					continue;
				}
				if (state > 0 && i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Ldarg_0)
				{
					if (instructionList[i + 1].LoadsField(needDef))
					{
						if (state == 1 && i < instructionList.Count - 2 &&
							instructionList[i + 2].LoadsField(scaleBarDef))
						{
							state = 2;
							//0								#Consumed at #Pop or #End
							yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
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
							//1								#Pop
							yield return new CodeInstruction(OpCodes.Pop);
							//0		tmp1 = max / cur;
							yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
							yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex);
							//2								consumed at #Mul
							yield return new CodeInstruction(OpCodes.Div);
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
							//2		max = cur;
							yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex).WithLabels(jumpLabels[2]);
							yield return new CodeInstruction(OpCodes.Stloc_S, max.LocalIndex);
							//2		tmp1 *= tmp2;			#Mul
							yield return new CodeInstruction(OpCodes.Mul);
							//1		mult = tmp1;			#End
							yield return new CodeInstruction(OpCodes.Stloc_S, mult.LocalIndex).WithLabels(jumpLabels[0]);
							//0	Done
							yield return codeInstruction;
							continue;
						}
						if (state == 4 && i < instructionList.Count - 4 &&
							instructionList[i + 2].opcode == OpCodes.Ldfld &&
							instructionList[i + 3].opcode == OpCodes.Brfalse_S)
						{
							state = 5;
							//	if (n.def.scaleBar && max < 1f)
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Callvirt, get_MaxLevel);
							yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
							yield return new CodeInstruction(OpCodes.Beq_S, jumpLabels[3]);
							yield return codeInstruction;
							yield return instructionList[i + 1];
							yield return instructionList[i + 2];
							yield return instructionList[i + 3];
							yield return instructionList[i + 4].WithLabels(jumpLabels[3]);
							i += 4;
							continue;
						}
					}
					if (state > 1 && instructionList[i + 1].Calls(get_MaxLevel))
					{
						// n.MaxLevel => max;
						yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
						i++;
						continue;
					}
					if (state > 1 && instructionList[i + 1].Calls(get_CurLevelPercentage))
					{
						// n.CurLevelPercentage => cur / max;
						yield return new CodeInstruction(OpCodes.Ldloc_S, cur.LocalIndex);
						yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
						yield return new CodeInstruction(OpCodes.Div);
						i++;
						continue;
					}
				}
				if (state == 2 &&
					codeInstruction.opcode == OpCodes.Ldloc_S &&
					codeInstruction.OperandIs(num4Idx))
				{
					state = 3;
					//	mult *= num4;
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
					(instructionList[i + 2].Calls(DrawBarThreshold) ||
					instructionList[i + 2].Calls(DrawBarInstantMarkerAt)))
				{
					state += 1;
					//	DrawBarThreshold.Invoke(n, new object[2] { barRect, threshPercents[i] * mult });
					//	DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Ldloc_S, mult.LocalIndex);
					continue;
				}
				if (state == 6 &&
					codeInstruction.Calls(DrawBarInstantMarkerAt) &&
					instructionList[i - 1].opcode == OpCodes.Mul)
				{
					state = 7;
					//	DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Call, clamp01Method);
					yield return codeInstruction;
					continue;
				}
				yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 7;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "DrawOnGUI_Transpiler", state, expectedState));
#endif
		}
		public static float Adjust_MaxLevel(float m, Need n)
		{
			int i = C.needTypes.GetValueOrDefault(n.GetType(), C.Default);
			if (!s.enabledA[i])
				return m;
			if (i == C.Food)
				return Mathf.Max(m * s.statsA[i], m + s.statsB[new IntVec2(C.Food, 1)]);
			return m * s.statsA[i];
		}
		public static float Adjust_NeedInterval_Drain(float m, Need n, int c)
		{
			float overflowAmount = n.CurLevelPercentage - 1f;
			IntVec2 v;
			if (overflowAmount > 0 && s.enabledA[c] && s.enabledB[v = new IntVec2(c, 1)])
				return m * (s.statsB[v] * overflowAmount + 1f);
			return m;
		}
		public static float Adjust_NeedInterval_Gain(float m, Need n, int c)
		{
			float overflowAmount = n.CurLevelPercentage - 1f;
			IntVec2 v;
			if (overflowAmount > 0 && s.enabledA[c] && s.enabledB[v = new IntVec2(c, 2)])
				return m / (s.statsB[v] * overflowAmount + 1f);
			return m;
		}
		public static IEnumerable<CodeInstruction> CurLevel_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			MethodInfo adjust = ((Func<float, Need, float>)Adjust_MaxLevel).Method;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (codeInstruction.Calls(get_MaxLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, adjust);
				}
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "CurLevel_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> CurLevelPercentage_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			MethodInfo clamp01Method = ((Func<float, float>)Mathf.Clamp01).Method;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (codeInstruction.Calls(get_CurLevelPercentage))
                {
					state++;
					yield return new CodeInstruction(OpCodes.Call, clamp01Method);
				}
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "CurLevelPercentage_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> Clamp01_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (codeInstruction.Calls(clamp01Method))
				{
					// stackTop, before ops: the value to be clamped
					// vanilla, after ops: value clamped to 0-1
					// patched, after ops: value clamped to 0-statsA[S.patchParamInt[0]]
					// S.patchParamInt[0] is a constant, statsA is a static array
					state++;
					yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
					yield return new CodeInstruction(OpCodes.Ldsfld, settingsField);
					yield return new CodeInstruction(OpCodes.Ldfld, statsAField);
					yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]);
					yield return new CodeInstruction(OpCodes.Ldelem_R4);
					yield return new CodeInstruction(OpCodes.Call, clampMethod);
				}
				else
				{
					yield return codeInstruction;
				}
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Clamp01_Transpiler", state, expectedState));
#endif
		}
		public static void NutritionWanted_Postfix(ref float __result)
        {
			if (__result < 0)
				__result = 0;
		}
		public static void AddHumanlikeOrders_Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (!s.enabledA[C.Food] || !s.enabledB[new IntVec2(C.Food, 1)])
				return;
			Need_Food need_Food = pawn.needs?.food;
			if (need_Food == null
				|| need_Food.CurCategory > HungerCategory.Fed
				|| need_Food.CurLevel < s.statsB[new IntVec2(C.Food, 2)] * need_Food.MaxLevel)
				return;
			IntVec3 c = IntVec3.FromVector3(clickPos);
			foreach (FloatMenuOption opt in opts)
			{
				if (opt.action == null)
					continue;
				foreach (Thing thing in c.GetThingList(pawn.Map))
				{
					if (pawn.RaceProps.CanEverEat(thing) && thing.def.IsNutritionGivingIngestible)
					{
						string value = "ConsumeThing".Translate(thing.LabelShort, thing);
						if (opt.Label.Contains(value))
						{
							opt.Label = opt.Label + ": " + "NBO.Disabled_FoodFull".Translate();
							opt.action = null;
							break;
						}
					}
				}
			}
		}
		public static void WillIngestFromInventoryNow_Postfix(Pawn pawn, Thing inv, ref bool __result)
		{
			__result &= !__result
			|| !s.enabledA[C.Food]
			|| !s.enabledB[new IntVec2(C.Food, 1)]
			|| !inv.def.IsNutritionGivingIngestible
			|| pawn.needs.food.CurCategory > HungerCategory.Fed
			|| pawn.needs.food.CurLevel < s.statsB[new IntVec2(C.Food, 2)] * pawn.needs.food.MaxLevel;
		}
		public static void Food_NeedInterval_Inject(Pawn pawn)
		{
			if (HediffComp_FoodOverflow.pawnsWithFoodOverflow.Contains(pawn) ||
				!s.enabledA[C.Food] || !s.FoodOverflowAffectHealth)
				return;
			HediffComp_FoodOverflow.pawnsWithFoodOverflow.Add(pawn);
			if (foodOverflow != null && pawn.health.hediffSet.GetFirstHediffOfDef(foodOverflow) == null)
				pawn.health.AddHediff(foodOverflow);
		}
		public static IEnumerable<CodeInstruction> Food_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			Label jumpLabel = ilg.DefineLabel();
			MethodInfo inject = ((Action<Pawn>)Food_NeedInterval_Inject).Method;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 &&
					codeInstruction.Calls(set_CurLevel))
				{
					state++;
					// If curLevel <= maxLevel, skip
					// otherwise do rest of checks in Food_NeedInterval_Inject and apply hediff
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, get_MaxLevel);
					yield return new CodeInstruction(OpCodes.Ble_S, jumpLabel);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, needPawnField);
					yield return new CodeInstruction(OpCodes.Call, inject);
					yield return codeInstruction.WithLabels(jumpLabel);
				}
				else
				{
					yield return codeInstruction;
				}
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Food_NeedInterval_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> Rest_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (i > 0 && i < instructionList.Count - 1 &&
					instructionList[i + 1].Calls(set_CurLevel))
				{
					if (codeInstruction.opcode == OpCodes.Sub &&
						s.patches_Session[C.RestDrain])
					{
						state++;
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldc_I4, C.Rest);
						yield return new CodeInstruction(OpCodes.Call, adjustDrain);
					}
					else if (codeInstruction.opcode == OpCodes.Add &&
						s.patches_Session[C.RestGain] &&
						!instructionList[i - 1].Calls(get_CurLevel))
					{
						state++;
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldc_I4, C.Rest);
						yield return new CodeInstruction(OpCodes.Call, adjustGain);
					}
				}
				yield return codeInstruction;
			}
#if DEBUG
			int expectedState = (s.patches_Session[C.RestDrain] ? 1 : 0) + (s.patches_Session[C.RestGain] ? 1 : 0);
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Rest_NeedInterval_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> GainJoy_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i < instructionList.Count - 6 &&
					codeInstruction.opcode == OpCodes.Ldarg_1 &&
					instructionList[i + 1].opcode == OpCodes.Ldc_R4 &&
					instructionList[i + 2].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 3].Calls(get_CurLevel) &&
					instructionList[i + 4].opcode == OpCodes.Sub &&
					instructionList[i + 5].Calls(minMethod) &&
					instructionList[i + 6].opcode == OpCodes.Starg_S)
				{
					state = 1;
					i += 6;
					continue;
				}
				yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "GainJoy_Transpiler", state, expectedState));
#endif
		}
		public static void GainJoy_Prefix(Need_Joy __instance, ref float amount)
		{
			amount = Adjust_NeedInterval_Gain(amount, __instance, C.Joy);
		}
		public static IEnumerable<CodeInstruction> StartInspirationMTBDays_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (codeInstruction.Calls(get_CurLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, clamp01Method);
				}

			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "GainJoy_Prefix", state, expectedState));
#endif
		}
#if !v1_2
		public static IEnumerable<CodeInstruction> DrawSuppressionBar_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			Label end = ilg.DefineLabel();
			LocalBuilder perc = ilg.DeclareLocal(typeof(float));
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i > 0 &&
                    instructionList[i - 1].Calls(get_CurLevelPercentage))
                {
					state = 1;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, maxMethod);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Div);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
				}
				yield return codeInstruction;
				if (state > 0 && state < 3 && i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Ldc_R4 &&
					instructionList[i + 1].Calls(DrawBarThreshold))
                {
					state++;
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Mul);
				}
				if (state == 3 && i < instructionList.Count - 1 && 
					codeInstruction.Calls(DrawBarThreshold))
                {
					state++;
					i++;
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ble_S, end);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Call, DrawBarThreshold);
					yield return instructionList[i].WithLabels(end);
				}
			}
#if DEBUG
			const int expectedState = 4;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "DrawSuppressionBar_Transpiler", state, expectedState));
#endif
		}
#endif
#if !v1_2 && !v1_3
		public static IEnumerable<CodeInstruction> KillThirst_KilledPawn_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i > 0 && i < instructionList.Count - 1 &&
					instructionList[i - 1].opcode == OpCodes.Ldarg_0 &&
					codeInstruction.LoadsConstant(1d) &&
					instructionList[i + 1].Calls(set_CurLevel))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Callvirt, get_CurLevel);
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, C.KillThirst);
					yield return new CodeInstruction(OpCodes.Call, adjustGain);
					yield return new CodeInstruction(OpCodes.Add);
				}
				else
					yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "KillThirst_KilledPawn_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> Learn_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i < instructionList.Count - 7 &&
					codeInstruction.opcode == OpCodes.Ldarg_1 &&
					instructionList[i + 1].LoadsConstant(1d) &&
					instructionList[i + 2].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 3].Calls(get_CurLevel) &&
					instructionList[i + 4].opcode == OpCodes.Sub &&
					instructionList[i + 5].Calls(minMethod) &&
					instructionList[i + 6].opcode == OpCodes.Starg_S &&
					instructionList[i + 7].opcode == OpCodes.Ldarg_0)
				{
					state = 1;
					i += 7;
					yield return instructionList[i].WithLabels(codeInstruction.ExtractLabels());
					continue;
				}
				if (state == 1 &&
					codeInstruction.StoresField(curLevelIntField) &&
					instructionList[i - 1].opcode == OpCodes.Add)
				{
					state = 2;
					yield return new CodeInstruction(OpCodes.Callvirt, set_CurLevel);
					continue;
				}
				yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 2;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Learn_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> DrawLearning_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			MethodInfo clamp01Method = ((Func<float, float>)Mathf.Clamp01).Method;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (state == 0 && codeInstruction.Calls(get_CurLevelPercentage))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, clamp01Method);
				}
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "DrawLearning_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> Play_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (state == 0 && i < instructionList.Count - 5 &&
					codeInstruction.opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 1].opcode == OpCodes.Ldarg_0 &&
					instructionList[i + 2].Calls(get_CurLevelPercentage) &&
					instructionList[i + 3].Calls(clamp01Method) &&
					instructionList[i + 4].Calls(set_CurLevelPercentage))
				{
					state++;
					i += 4;
					continue;
				}
				yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Play_Transpiler", state, expectedState));
#endif
		}
#endif
		public static IEnumerable<CodeInstruction> Drain_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (i > 0 && i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Sub &&
					instructionList[i + 1].Calls(set_CurLevel) &&
					s.patches_Session[S.patchParamInt[0]])
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[1]);
					yield return new CodeInstruction(OpCodes.Call, adjustDrain);
				}
				yield return codeInstruction;
			}
#if DEBUG
			const int expectedState = 1;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "Drain_NeedInterval_Transpiler", state, expectedState));
#endif
		}
		public static IEnumerable<CodeInstruction> RemoveLastMin_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			int lastIdxBeforeLdc = -1;
			for (int i = instructionList.Count - 2; i > 0; i--)
			{
				if (instructionList[i].LoadsConstant(1.0) &&
					instructionList[i + 1].Calls(minMethod))
				{
					state++;
					lastIdxBeforeLdc = i - 1;
					break;
				}
			}
			for (int i = 0; i < instructionList.Count; i++)
			{
				yield return instructionList[i];
				if (i == lastIdxBeforeLdc)
				{
					state++;
					i += 2;
				}
			}
#if DEBUG
			const int expectedState = 2;
			if (state < expectedState)
				Log.Warning(string.Format(transpilerStateWarning, "RemoveLastMin_Transpiler", state, expectedState));
#endif
		}
		public override string SettingsCategory() => "NBO.Name".Translate();
		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			s.DoWindowContents(inRect);
		}
	}
	public class HediffCompProperties_FoodOverflow : HediffCompProperties
	{
		public HediffCompProperties_FoodOverflow()
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: HediffCompProperties_FoodOverflow constructor called");
#endif
			compClass = typeof(HediffComp_FoodOverflow);
		}
	}
	public class HediffComp_FoodOverflow : HediffComp
	{
		public static S s;
		private static readonly MethodInfo IsFrozen = AccessTools.PropertyGetter(typeof(Need_Food), "IsFrozen");
		public static TraitDef gourmand;
		public static HashSet<Pawn> pawnsWithFoodOverflow = new HashSet<Pawn>();
		public float effectMultiplier = 0f;
		public Pawn pawn;
		public HediffCompProperties_FoodOverflow Props => (HediffCompProperties_FoodOverflow)props;
		public HediffComp_FoodOverflow()
		{
#if DEBUG
			Log.Message("[Need Bar Overflow]: HediffComp_FoodOverflow constructor called");
#endif
			if (s == null)
				s = NeedBarOverflow.s;
		}
        public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			if (pawn == null)
				pawn = Pawn;
			int hash = pawn.HashOffsetTicks();
			if (hash % 150 != 0)
				return;
			Need_Food need;
			if (!s.enabledA[C.Food] ||
				!s.FoodOverflowAffectHealth ||
				(need = pawn.needs?.food) == null ||
				(bool)IsFrozen.Invoke(need, null))
			{
				pawn.health.RemoveHediff(parent);
				return;
			}
			if (effectMultiplier <= 0f || hash % 3600 == 0)
			{
				if (!pawn.RaceProps.Humanlike)
					effectMultiplier = s.statsB[new IntVec2(C.Food, 3)];
				else if(gourmand != null && (bool)(pawn.story?.traits?.HasTrait(gourmand)))
					effectMultiplier = s.statsB[new IntVec2(C.Food, 4)];
				else
					effectMultiplier = 1f;
			}
			parent.Severity = (need.CurLevelPercentage - 1) * effectMultiplier;
		}
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
			pawnsWithFoodOverflow.Remove(Pawn);
		}
		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			Pawn.health.RemoveHediff(parent);
		}
	}
}
