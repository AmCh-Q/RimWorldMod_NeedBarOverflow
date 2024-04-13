using RimWorld;
using Verse;

namespace NeedBarOverflow
{
	internal static class Refs
	{
		public static bool initialized = false;
		public static TraitDef Gourmand;
		public static PawnCapacityDef Eating;
		public static HediffDef FoodOverflow;
		//public static Func<Thing, bool> VFEAncients_HasPower;
		public static void Init()
		{
			InitDef(ref Gourmand, nameof(Gourmand));
			InitDef(ref Eating, nameof(Eating));
			InitDef(ref FoodOverflow, nameof(FoodOverflow));
			//VFEAncients();
			initialized = true;
		}
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
		/*
		// VFE Anvients patch is disabled 
		// because the "Disable Need Overflow with some apparel" setting should be enough
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
