using System.Reflection;

namespace NeedBarOverflow
{
	public static class Consts
	{
		internal const BindingFlags bindingflags
			= BindingFlags.Instance | BindingFlags.Static
			| BindingFlags.Public | BindingFlags.NonPublic;
	}
	public static class Strings
	{
		public const string
			Space = " ",
			Name = "NBO.Name",
			RestartReq = "NBO.RestartReq_Tip",
			RestartNReq = "NBO.RestartNReq_Tip",

			overflow = nameof(overflow),
			overflowStats = nameof(overflowStats),
			healthStats = nameof(healthStats),
			disablingDefs = nameof(disablingDefs),

			ShowHiddenSettings = nameof(ShowHiddenSettings),
			OverfEnabled = nameof(OverfEnabled),
			OverfPerc = nameof(OverfPerc),

			NoOverf_ = nameof(NoOverf_),

			HealthDetails = nameof(HealthDetails),
			HealthEnable_ = nameof(HealthEnable_),
			HealthStat_ = nameof(HealthStat_),
			ConsumeThing = nameof(ConsumeThing),
			FoodFull = "NBO.Disabled_FoodFull";
	}
}
