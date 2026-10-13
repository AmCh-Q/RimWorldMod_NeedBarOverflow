#if g1_3
using HarmonyLib;
using NeedBarOverflow.Needs;
using NeedBarOverflow.Patches;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace NeedBarOverflow.ModCompat;

// Vanilla Nutrient Paste Expanded
// https://steamcommunity.com/sharedfiles/filedetails/?id=2920385763
public sealed class VanillaNutrientPasteExpanded() : Patches.Patch(
	postfix: PostfixMethod)
{
	protected override void Initialize()
	{
		if (Target is not null)
			return;
		if (!ModsConfig.IsActive("VanillaExpanded.VNutrientE"))
			return;
		Type? type = GenTypes.GetTypeInAnyAssembly("VNPE.Building_Dripper");
		if (type is null)
		{
			Debug.Warning("VNPE.Building_Dripper is null");
			return;
		}
		Target = [type.Method("StartInspirationMTBDays")];
	}
	public static readonly AccessTools.FieldRef<object, CompFacility>? fr_facilityComp;
	static VanillaNutrientPasteExpanded()
	{
		if (!ModsConfig.IsActive("VanillaExpanded.VNutrientE"))
			return;
		FieldInfo? f_facilityComp
			= GenTypes.GetTypeInAnyAssembly("VNPE.Building_Dripper")?
			.GetField("facilityComp", Consts.bindAll);
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

#endif
