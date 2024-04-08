namespace NeedBarOverflow
{
    public enum StatNames
    {
        FastDrain = 0,
        SlowGain = 1,
        OverflowBonus = 0,
        DisableEating = 1,
        NonHumanMult = 2,
        GourmandMult = 3,
        ShowHediffLvl = 4,
    }
    public static class Strings
	{
		public const string
			Space = " ",
			Name = "NBO.Name",
            RestartReq = "NBO.RestartReq_tip",
			RestartNReq = "NBO.RestartNReq_tip",

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
