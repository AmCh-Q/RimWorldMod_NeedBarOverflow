using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NeedBarOverflow
{
    [DefOf]
    public static class ModDefOf
    {
        public static TraitDef Gourmand;
        public static PawnCapacityDef Eating;
        public static HediffDef FoodOverflow;
#if v1_4 || v1_5
        [MayRequireBiotech]
        public static ThoughtDef IngestedHemogenPack;
#endif
        static ModDefOf()
            => DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));
    }

    internal static class Refs
	{
		public static bool initialized = false;
        public static void Init()
		{
            initialized = true;
        }
        // public static Func<Thing, bool> VFEAncients_HasPower;
        /*
		private static void InitDef<T>(
			ref T def, string defName, 
			bool force = true) where T : Def
		{
			if (def == null)
			{
				def = DefDatabase<T>.GetNamed(defName);
				if (def == null && force)
					Debug.Warning(string.Concat(
						"Reference ", typeof(T).Name, 
						Strings.Space, defName, 
						" expected but failed to load."));
			}
        }
		private static void VFEAncients()
		{
			// VFE-Ancients Compatibility
			if (VFEAncients_HasPower != null)
				return;
			if (!ModLister.HasActiveModWithName("Vanilla Factions Expanded - Ancients"))
				return;
			Type PowerWorker_Hunger 
				= AccessTools.TypeByName("VFEAncients.PowerWorker_Hunger");
#if !DEBUG
			if (PowerWorker_Hunger == null)
				return;
#endif
			MethodInfo m_VFEAncients_HasPower = AccessTools.Method(
				"VFEAncients.HarmonyPatches.Helpers:HasPower",
				new[] { typeof(Thing) },
				new[] { PowerWorker_Hunger });
#if !DEBUG
			if (m_VFEAncients_HasPower == null)
				return;
#endif
			VFEAncients_HasPower = (Func<Thing, bool>)Delegate.CreateDelegate(
				Expression.GetFuncType(new[] { typeof(Thing), typeof(bool) }),
				null, m_VFEAncients_HasPower, false);
		}
		*/
    }
}
