using System.Reflection;

namespace NeedBarOverflow;

public static class Consts
{
	internal const BindingFlags
		bindAll
			= BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.NonPublic,
		bindPublicInstance
			= BindingFlags.Instance | BindingFlags.Public,
		bindPublicStatic
			= BindingFlags.Static | BindingFlags.Public,
		bindNonpubInstance
			= BindingFlags.Instance | BindingFlags.NonPublic,
		bindNonPubStatic
			= BindingFlags.Static | BindingFlags.NonPublic;
}

public static class Strings
{
	public const string
		Name = "NBO.Name",
		RestartReq_Tip = "NBO.RestartReq_Tip",
		RestartNReq_Tip = "NBO.RestartNReq_Tip",
		NoOverf_ListDefs = "NBO.NoOverf.ListDefs",
		NoOverf_ListDefs_Tip = "NBO.NoOverf.ListDefs_Tip",
		NoOverf_NoDefsFound = "NBO.NoOverf.NoDefsFound",

		overflow = nameof(overflow),
		overflowStats = nameof(overflowStats),
		healthStats = nameof(healthStats),
		disablingDefs = nameof(disablingDefs),
		checkIntervalTicks = nameof(checkIntervalTicks),
		membership = nameof(membership),

		ShowSettings = nameof(ShowSettings),
		ShowHiddenSettings = nameof(ShowHiddenSettings),
		OverfEnabled = nameof(OverfEnabled),
		OverfPerc = nameof(OverfPerc),

		NoOverf = nameof(NoOverf),
		AllowOverf = nameof(AllowOverf),

		HealthDetails = nameof(HealthDetails),
		HealthEnable_ = nameof(HealthEnable_),
		HealthStat_ = nameof(HealthStat_),
		ConsumeThing = nameof(ConsumeThing),
		FoodFull = "NBO.Disabled_FoodFull",

		NoOverf_OfVFEAPower = "NBO.NoOverf.OfVFEAPower";

	public static readonly string[]
		NoOverf_Of = [
			"NBO.NoOverf.OfRace",
			"NBO.NoOverf.OfApparel",
			"NBO.NoOverf.OfHediff",
			"NBO.NoOverf.OfGene",
		];
}
