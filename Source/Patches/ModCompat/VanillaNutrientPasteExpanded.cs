#if g1_3
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using NeedBarOverflow.Needs;
using NeedBarOverflow.Patches;

namespace NeedBarOverflow.ModCompat
{
	// Vanilla Nutrient Paste Expanded
	// https://steamcommunity.com/sharedfiles/filedetails/?id=2920385763
	public sealed class VanillaNutrientPasteExpanded() : Patch_Single(
		original: Helpers
			.TypeByName("VNPE.Building_Dripper")?
			.MethodNullable("TickRare"),
		postfix: PostfixMethod)
	{
		public static readonly AccessTools.FieldRef<object, CompFacility>? fr_facilityComp;
		static VanillaNutrientPasteExpanded()
		{
			FieldInfo? f_facilityComp = Helpers.TypeByName("VNPE.Building_Dripper")?.GetField("facilityComp", Consts.bindAll);
			if (f_facilityComp is null)
				fr_facilityComp = null;
			else
				fr_facilityComp = AccessTools.FieldRefAccess<object, CompFacility>(f_facilityComp);
		}
		public override void Toggle()
			=> Toggle(Setting_Common.Enabled(typeof(Need_Food)) && fr_facilityComp is not null);
		private static void PostfixMethod(object __instance)
		{
			if (__instance is null || fr_facilityComp is null)
				return;
			List<Thing>? LinkedBuildings = fr_facilityComp(__instance)?.LinkedBuildings;
			if (LinkedBuildings is null)
				return;
			foreach (Thing linkedThing in LinkedBuildings)
			{
				if (linkedThing is not Building_Bed bed)
					continue;
				foreach (Pawn occupant in bed.CurOccupants)
				{
					Need_Food? need_food = occupant.needs?.food;
					if (need_food is null)
						continue;
					if (need_food.CurLevelPercentage <= 1f)
						continue;
					Debug.Message($"VNPE: Readjusting {occupant.Name}'s food need to 100%");
					need_food.CurLevelPercentage = 1f;
				}
			}
		}
	}
}
#endif
