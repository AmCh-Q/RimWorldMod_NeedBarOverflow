
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
    using C = Constants;
    using S = Settings;
	using D = Debug;

	public class NeedBarOverflow : Mod
	{
		public static S s;
		public static HediffDef foodOverflow;
		public static Func<Thing,bool> VFEAncients_HasPower;

        public NeedBarOverflow(ModContentPack content) : base(content)
		{
			D.Message("NeedBarOverflow constructor called");
			s = GetSettings<S>();
			PatchApplier.s = s;
			LongEventHandler.QueueLongEvent(delegate
			{
				foodOverflow = HediffDef.Named("FoodOverflow");
				HediffComp_FoodOverflow.gourmand = TraitDef.Named("Gourmand");
                PatchApplier.ApplyPatches();
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
		public override string SettingsCategory() => "NBO.Name".Translate();
		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			s.DoWindowContents(inRect);
		}
	}
}
