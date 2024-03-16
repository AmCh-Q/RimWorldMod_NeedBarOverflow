namespace NeedBarOverflow
{
	using HarmonyLib;
    using RimWorld;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Linq;
    using System.Linq.Expressions;
    using UnityEngine;
    using Verse;
    using C = NeedBarOverflow_Consts;
    using S = NeedBarOverflow_Settings;
	using D = NeedBarOverflow_Debug;

	public class NeedBarOverflow : Mod
	{
		public static S s;
		private static readonly PropertyInfo 
			p_MaxLevel = AccessTools.Property(typeof(Need), nameof(Need.MaxLevel)),
			p_CurLevel = AccessTools.Property(typeof(Need), nameof(Need.CurLevel)),
			p_CurLevelPercentage = AccessTools.Property(typeof(Need), nameof(Need.CurLevelPercentage));
		private static readonly MethodInfo
			m_DrawBarThreshold = AccessTools.Method(typeof(Need), "DrawBarThreshold"),
			m_DrawBarInstantMarkerAt = AccessTools.Method(typeof(Need), "DrawBarInstantMarkerAt"),
			m_clamp01 = ((Func<float, float>)Mathf.Clamp01).Method,
			m_clamp = ((Func<float, float, float, float>)Mathf.Clamp).Method,
			m_max = ((Func<float, float, float>)Mathf.Max).Method,
			m_min = ((Func<float, float, float>)Mathf.Min).Method,
			m_adjustDrain = ((Func<float, Need, int, float>)Adjust_NeedInterval_Drain).Method,
			m_adjustGain = ((Func<float, Need, int, float>)Adjust_NeedInterval_Gain).Method;
		private static readonly FieldInfo 
			f_settings = typeof(NeedBarOverflow).GetField("s"),
			f_statsA = typeof(S).GetField(nameof(S.statsA)),
			f_needDef = AccessTools.Field(typeof(Need), nameof(Need.def)),
			f_scaleBarDef = AccessTools.Field(typeof(NeedDef), nameof(NeedDef.scaleBar)),
			f_needPawn = AccessTools.Field(typeof(Need), "pawn"),
			f_curLevelInt = AccessTools.Field(typeof(Need), "curLevelInt");
		public static HediffDef foodOverflow;
		private static Func<Thing,bool> VFEAncients_HasPower;

        public NeedBarOverflow(ModContentPack content) : base(content)
		{
			D.Message("NeedBarOverflow constructor called");
			s = GetSettings<S>();
			NeedBarOverflow_Patches.s = s;
			LongEventHandler.QueueLongEvent(delegate
			{
				foodOverflow = HediffDef.Named("FoodOverflow");
				HediffComp_FoodOverflow.gourmand = TraitDef.Named("Gourmand");
                NeedBarOverflow_Patches.ApplyPatches();
                s.ApplyFoodHediffSettings();
                s.ApplyFoodDisablingSettings<ThingDef>(C.ThingDef);
                s.ApplyFoodDisablingSettings<HediffDef>(C.HediffDef);

                // VFE-Ancients Compatibility
                if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Ancients"))
				{
                    Type PowerWorker_Hunger = AccessTools.TypeByName("VFEAncients.PowerWorker_Hunger");
					if (PowerWorker_Hunger != null)
                    {
                        MethodInfo m_VFEAncients_HasPower = AccessTools.Method(
							"VFEAncients.HarmonyPatches.Helpers:HasPower",
                            new[] { typeof(Thing) },
                            new[] { PowerWorker_Hunger }
							);
                        if (m_VFEAncients_HasPower != null)
                            VFEAncients_HasPower = (Func<Thing, bool>)Delegate.CreateDelegate(
								Expression.GetFuncType(new[] { typeof(Thing), typeof(bool) }), 
								null, m_VFEAncients_HasPower, false);
                    }
                    if (VFEAncients_HasPower != null)
                        D.Message("Loaded VFEAncients Compatibility Patch Successfully");
                    else
                        D.Message("Loading VFEAncients Compatibility Patch Failed");
                }
            }, "NeedBarOverflow.Mod.ctor", false, null);
		}
		public static void GenUI_Prefix(ref float pct) => pct = Mathf.Clamp01(pct);

		/*
			private static readonly MethodInfo m_DrawBarThreshold = AccessTools.Method(typeof(Need), "DrawBarThreshold");
			private static readonly MethodInfo m_DrawBarDivision = AccessTools.Method(typeof(Need), "DrawBarDivision");
			private static readonly MethodInfo m_DrawBarInstantMarkerAt = AccessTools.Method(typeof(Need), "m_DrawBarInstantMarkerAt");
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
						m_DrawBarThreshold.Invoke(n, new object[2] { barRect, threshPercents[i] * mult } );
				if (n.MaxLevel == 1f || n.def.showUnitTicks)
					for (float j = 1; j < max; j++)
						m_DrawBarDivision.Invoke(n, new object[2] { barRect, j / max * num4 });
				float curInstantLevelPercentage = n.CurInstantLevelPercentage;
				if (curInstantLevelPercentage >= 0f)
					m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
			}
		*/
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
					if (instructionList[i + 1].LoadsField(f_needDef))
					{
						if (state == 1 && i < instructionList.Count - 2 &&
							instructionList[i + 2].LoadsField(f_scaleBarDef))
						{
							state = 2;
							//0								#Consumed at #Pop or #End
							yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
							//1	float max = n.MaxLevel;
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Callvirt, p_MaxLevel.GetGetMethod());
							yield return new CodeInstruction(OpCodes.Dup);  // consumed at #Bge_Un_S
							yield return new CodeInstruction(OpCodes.Stloc_S, max.LocalIndex);
							//2	float cur = n.CurLevel;
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Callvirt, p_CurLevel.GetGetMethod());
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
							yield return new CodeInstruction(OpCodes.Callvirt, p_MaxLevel.GetGetMethod());
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
					if (state > 1 && instructionList[i + 1].Calls(p_MaxLevel.GetGetMethod()))
					{
						// n.MaxLevel => max;
						yield return new CodeInstruction(OpCodes.Ldloc_S, max.LocalIndex);
						i++;
						continue;
					}
					if (state > 1 && instructionList[i + 1].Calls(p_CurLevelPercentage.GetGetMethod()))
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
					(instructionList[i + 2].Calls(m_DrawBarThreshold) ||
					instructionList[i + 2].Calls(m_DrawBarInstantMarkerAt)))
				{
					state += 1;
					//	m_DrawBarThreshold.Invoke(n, new object[2] { barRect, threshPercents[i] * mult });
					//	m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Ldloc_S, mult.LocalIndex);
					continue;
				}
				if (state == 6 &&
					codeInstruction.Calls(m_DrawBarInstantMarkerAt) &&
					instructionList[i - 1].opcode == OpCodes.Mul)
				{
					state = 7;
					//	m_DrawBarInstantMarkerAt.Invoke(n, new object[2] { rect3, Mathf.Clamp01(curInstantLevelPercentage * mult) });
					yield return new CodeInstruction(OpCodes.Call, m_clamp01);
					yield return codeInstruction;
					continue;
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 7);
		}
		public static float Adjust_NeedInterval_Drain(float m, Need n, int c)
		{
			float overflowAmount = n.CurLevelPercentage - 1f;
			IntVec2 v;
			if (overflowAmount > 0 && s.enabledA[c] && s.enabledB[v = C.V(c, 1)])
				return m * (s.statsB[v] * overflowAmount + 1f);
			return m;
		}
		public static float Adjust_NeedInterval_Gain(float m, Need n, int c)
		{
			float overflowAmount = n.CurLevelPercentage - 1f;
			IntVec2 v;
			if (overflowAmount > 0 && s.enabledA[c] && s.enabledB[v = C.V(c, 2)])
				return m / (s.statsB[v] * overflowAmount + 1f);
			return m;
		}
		private static bool Check_Pawn_Race(Pawn p)
        {
            string defName = p?.kindDef?.race?.defName;
            if (defName.NullOrEmpty())
                return true;
            if (s.foodDisablingDefs_set[C.ThingDef].Contains(defName.ToLowerInvariant()))
                return false;
			return true;
        }
		private static bool Check_Pawn_Health(Pawn p)
        {
            List<Hediff> hediffs = p?.health?.hediffSet.hediffs;
			if (hediffs.NullOrEmpty())
				return true;
            foreach (Hediff hediff in hediffs)
			{
				string defName = hediff?.def?.defName;
				if (defName.NullOrEmpty())
					return true;
				if (s.foodDisablingDefs_set[C.HediffDef].Contains(defName.ToLowerInvariant()))
					return false;
			}
			return true;
		}
		public static float Adjust_MaxLevel(float m, Need n)
		{
			int i = C.needTypes.GetValueOrDefault(n.GetType(), C.DefaultNeed);
			if (!s.enabledA[i])
				return m;
            switch (i)
            {
                case C.Food:
					if (s.enabledB_Override.Any(s => s) ||
                        VFEAncients_HasPower != null)
                    {
                        Pawn p = (Pawn)f_needPawn.GetValue(n);
                        if ((s.enabledB_Override[C.ThingDef] && !Check_Pawn_Race(p)) ||
                            (s.enabledB_Override[C.HediffDef] && !Check_Pawn_Health(p)) ||
							(VFEAncients_HasPower != null && VFEAncients_HasPower.Invoke(p)))
                            return m;
                    }
                    return Mathf.Max(m * s.statsA[i], m + s.statsB[C.V(C.Food, 1)]);
                default:
                    return m * s.statsA[i];
            }
		}
		public static IEnumerable<CodeInstruction> CurLevel_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
            Label skipAdjustLabel = ilg.DefineLabel();
            Label needAdjustLabel = ilg.DefineLabel();
            MethodInfo adjustMax = ((Func<float, Need, float>)Adjust_MaxLevel).Method;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
                // If state == 1, then we've just done the patching, insert one ending jump label
                if (state == 1)
                {
                    state++;
                    yield return codeInstruction.WithLabels(skipAdjustLabel);
					continue;
                }
                // If state > 1 we have done the patching, pass instruction normally
                // or if the instruction is not to get the original max value, pass it normally
                if (state > 1 || !codeInstruction.Calls(p_MaxLevel.GetGetMethod()))
                {
                    yield return codeInstruction;
					continue;
                }
                state++;
                // First check if f_curLevelInt is less than the new value (from Ldarg_1)
                yield return new CodeInstruction(OpCodes.Ldfld, f_curLevelInt);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Blt_S, needAdjustLabel);

                // If it is not, that means the new value did not increase
                // Then set max value to infinity and skip the adjust process
                yield return new CodeInstruction(OpCodes.Ldc_R4, float.PositiveInfinity);
                yield return new CodeInstruction(OpCodes.Br_S, skipAdjustLabel);

                // If it is, then go adjust the max value
                // Adjust the max value by calling Adjust_MaxLevel(originalMaxValue, need)
                yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(needAdjustLabel);
                yield return codeInstruction;
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, adjustMax);
            }
			D.CheckTranspiler(state, 2);
		}
		public static IEnumerable<CodeInstruction> CurLevelPercentage_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (codeInstruction.Calls(p_CurLevelPercentage.GetGetMethod()))
                {
					state++;
					yield return new CodeInstruction(OpCodes.Call, m_clamp01);
				}
			}
			D.CheckTranspiler(state, 1);
		}
		public static IEnumerable<CodeInstruction> Clamp01_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (!codeInstruction.Calls(m_clamp01))
				{
					yield return codeInstruction;
					continue;
				}
				// stackTop, before ops: the value to be clamped
				// vanilla, after ops: value clamped to 0-1
				// patched, after ops: value clamped to 0-statsA[S.patchParamInt[0]]
				// S.patchParamInt[0] is a constant, statsA is a static array
				state++;
				yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
				yield return new CodeInstruction(OpCodes.Ldsfld, f_settings);
				yield return new CodeInstruction(OpCodes.Ldfld, f_statsA);
				yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]);
				yield return new CodeInstruction(OpCodes.Ldelem_R4);
				yield return new CodeInstruction(OpCodes.Call, m_clamp);
			}
			D.CheckTranspiler(state, 1);
		}
		public static void NutritionWanted_Postfix(ref float __result)
        {
			if (__result < 0)
				__result = 0;
		}
		public static void AddHumanlikeOrders_Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (!s.enabledA[C.Food] || !s.enabledB[C.V(C.Food, 1)])
				return;
			Need_Food need_Food = pawn.needs?.food;
			if (need_Food == null
				|| need_Food.CurCategory > HungerCategory.Fed
				|| need_Food.CurLevel < s.statsB[C.V(C.Food, 2)] * need_Food.MaxLevel)
				return;
			IntVec3 c = IntVec3.FromVector3(clickPos);
			if (c.ContainsStaticFire(pawn.Map))
				return;
			HashSet<string> ingestOrders = new HashSet<string>();
			foreach (FloatMenuOption opt in opts)
			{
				if (opt.action == null)
					continue;
				if (ingestOrders.Count == 0)
				{
                    List<Thing> thingList = c.GetThingList(pawn.Map);
					if (thingList.NullOrEmpty())
						return;
					foreach (Thing thing in thingList)
					{
						ThingDef thingDef = thing.def;
						if (!thingDef.IsNutritionGivingIngestible
							|| !pawn.RaceProps.CanEverEat(thing))
							continue;
						string ingestCommand = thingDef.ingestible.ingestCommandString;
						string ingestAction;
                        if (ingestCommand.NullOrEmpty())
                            ingestAction = "ConsumeThing".Translate(thing.LabelShort, thing);
						else
                            ingestAction = ingestCommand.Formatted(thing.LabelShort);
						if (!ingestOrders.Contains(ingestAction))
							ingestOrders.Add(ingestAction);
                    }
                    if (ingestOrders.Count == 0)
                        return;
                }
				string label = opt.Label;
                if (ingestOrders.Any(s => label.StartsWith(s)))
                {
                    opt.Label = label + ": " + "NBO.Disabled_FoodFull".Translate();
                    opt.action = null;
                }
			}
		}
		public static void WillIngestFromInventoryNow_Postfix(Pawn pawn, Thing inv, ref bool __result)
        {
            __result &= !s.enabledA[C.Food]
                || !s.enabledB[C.V(C.Food, 1)]
                || !inv.def.IsNutritionGivingIngestible
				|| pawn.needs?.food == null
                || pawn.needs.food.CurCategory > HungerCategory.Fed
                || pawn.needs.food.CurLevel < s.statsB[C.V(C.Food, 2)] * pawn.needs.food.MaxLevel;
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
					codeInstruction.Calls(p_CurLevel.GetSetMethod()))
				{
					state++;
                    // If value <= maxLevel, skip and set it to curLevel directly
                    // otherwise do rest of checks in Food_NeedInterval_Inject and apply hediff
                    yield return new CodeInstruction(OpCodes.Dup); // get a copy of value
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt, p_MaxLevel.GetGetMethod());
					yield return new CodeInstruction(OpCodes.Ble_S, jumpLabel);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, f_needPawn);
					yield return new CodeInstruction(OpCodes.Call, inject);
					yield return codeInstruction.WithLabels(jumpLabel);
				}
				else
				{
					yield return codeInstruction;
				}
			}
			D.CheckTranspiler(state, 1);
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
					instructionList[i + 3].Calls(p_CurLevel.GetGetMethod()) &&
					instructionList[i + 4].opcode == OpCodes.Sub &&
					instructionList[i + 5].Calls(m_min) &&
					instructionList[i + 6].opcode == OpCodes.Starg_S)
				{
					state = 1;
					i += 6;
					continue;
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 1);
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
				if (codeInstruction.Calls(p_CurLevel.GetGetMethod()))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, m_clamp01);
				}

			}
			D.CheckTranspiler(state, 1);
		}
#if (v1_3 || v1_4 || v1_5)
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
                    instructionList[i - 1].Calls(p_CurLevelPercentage.GetGetMethod()))
                {
					state = 1;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Call, m_max);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Div);
					yield return new CodeInstruction(OpCodes.Stloc_S, perc.LocalIndex);
				}
				yield return codeInstruction;
				if (state > 0 && state < 3 && i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Ldc_R4 &&
					instructionList[i + 1].Calls(m_DrawBarThreshold))
                {
					state++;
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Mul);
				}
				if (state == 3 && i < instructionList.Count - 1 && 
					codeInstruction.Calls(m_DrawBarThreshold))
                {
					state++;
					i++;
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Ble_S, end);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldloc_S, perc.LocalIndex);
					yield return new CodeInstruction(OpCodes.Call, m_DrawBarThreshold);
					yield return instructionList[i].WithLabels(end);
				}
			}
			D.CheckTranspiler(state, 4);
		}
#endif
#if (v1_4 || v1_5)
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
					instructionList[i + 1].Calls(p_CurLevel.GetSetMethod()))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Callvirt, p_CurLevel.GetGetMethod());
					yield return codeInstruction;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, C.KillThirst);
					yield return new CodeInstruction(OpCodes.Call, m_adjustGain);
					yield return new CodeInstruction(OpCodes.Add);
				}
				else
					yield return codeInstruction;
			}
			D.CheckTranspiler(state, 1);
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
					instructionList[i + 3].Calls(p_CurLevel.GetGetMethod()) &&
					instructionList[i + 4].opcode == OpCodes.Sub &&
					instructionList[i + 5].Calls(m_min) &&
					instructionList[i + 6].opcode == OpCodes.Starg_S &&
					instructionList[i + 7].opcode == OpCodes.Ldarg_0)
				{
					state = 1;
					i += 7;
					yield return instructionList[i].WithLabels(codeInstruction.ExtractLabels());
					continue;
				}
				if (state == 1 &&
					codeInstruction.StoresField(f_curLevelInt) &&
					instructionList[i - 1].opcode == OpCodes.Add)
				{
					state = 2;
					yield return new CodeInstruction(OpCodes.Callvirt, p_CurLevel.GetSetMethod());
					continue;
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 2);
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
				if (state == 0 && codeInstruction.Calls(p_CurLevelPercentage.GetGetMethod()))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Call, clamp01Method);
				}
			}
			D.CheckTranspiler(state, 1);
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
					instructionList[i + 2].Calls(p_CurLevelPercentage.GetGetMethod()) &&
					instructionList[i + 3].Calls(m_clamp01) &&
					instructionList[i + 4].Calls(p_CurLevelPercentage.GetSetMethod()))
				{
					state++;
					i += 4;
					continue;
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 1);
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
					instructionList[i + 1].Calls(p_CurLevel.GetSetMethod()))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]);
					yield return new CodeInstruction(OpCodes.Call, m_adjustDrain);
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 1);
		}
		public static IEnumerable<CodeInstruction> Gain_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (i > 0 && i < instructionList.Count - 1 &&
					codeInstruction.opcode == OpCodes.Add &&
					instructionList[i + 1].Calls(p_CurLevel.GetSetMethod()) &&
					!instructionList[i - 1].Calls(p_CurLevel.GetGetMethod()))
				{
					state++;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4, S.patchParamInt[0]);
					yield return new CodeInstruction(OpCodes.Call, m_adjustGain);
				}
				yield return codeInstruction;
			}
			D.CheckTranspiler(state, 1);
		}
		public static IEnumerable<CodeInstruction> RemoveLastMin_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();
			int state = 0;
			int lastIdxBeforeLdc = -1;
			for (int i = instructionList.Count - 2; i > 0; i--)
			{
				if (instructionList[i].LoadsConstant(1.0) &&
					instructionList[i + 1].Calls(m_min))
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
			D.CheckTranspiler(state, 2);
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
			D.Message("HediffCompProperties_FoodOverflow constructor called");
			compClass = typeof(HediffComp_FoodOverflow);
		}
	}
	public class HediffComp_FoodOverflow : HediffComp
	{
		public static S s;
		private static readonly MethodInfo IsFrozen = AccessTools.PropertyGetter(typeof(Need_Food), "IsFrozen");
		private static readonly FieldInfo visible = AccessTools.Field(typeof(Hediff), "visible");
		public static TraitDef gourmand;
		public static HashSet<Pawn> pawnsWithFoodOverflow = new HashSet<Pawn>();
		public float effectMultiplier = 0f;
		public HediffCompProperties_FoodOverflow Props => (HediffCompProperties_FoodOverflow)props;
		public HediffComp_FoodOverflow()
		{
			D.Message("HediffComp_FoodOverflow constructor called");
			if (s == null)
				s = NeedBarOverflow.s;
		}
        public override void CompPostTick(ref float severityAdjustment)
		{
			if (!Pawn.IsHashIntervalTick(150))
				return;
			Need_Food need;
			if ((need = Pawn.needs?.food) == null ||
				(bool)IsFrozen.Invoke(need, null) ||
				!s.enabledA[C.Food] ||
				!s.FoodOverflowAffectHealth)
			{
                Pawn.health.RemoveHediff(parent);
				return;
			}
			if (Pawn.IsHashIntervalTick(3600) || effectMultiplier <= 0f)
			{
				if (!Pawn.RaceProps.Humanlike)
					effectMultiplier = s.statsB[C.V(C.Food, 3)];
				else if(gourmand != null && (bool)(Pawn.story?.traits?.HasTrait(gourmand)))
					effectMultiplier = s.statsB[C.V(C.Food, 4)];
				else
					effectMultiplier = 1f;
			}
			float severity = (need.CurLevelPercentage - 1) * effectMultiplier;
			parent.Severity = severity;
			bool shouldBeVisible = severity > (s.statsB[C.V(C.Food, 5)] - 1f);
			if (parent.Visible != shouldBeVisible)
				visible.SetValue(parent, shouldBeVisible);
		}
#if (v1_2 || v1_3 || v1_4)
		// Removed as of 1.5
		public override void Notify_PawnDied() => Pawn.health.RemoveHediff(parent);
#else
        // New since 1.5
        public override void Notify_PawnDied(DamageInfo? _, Hediff __) => Pawn.health.RemoveHediff(parent);
#endif
        public override void CompPostPostRemoved() => pawnsWithFoodOverflow.Remove(Pawn);
	}
}
