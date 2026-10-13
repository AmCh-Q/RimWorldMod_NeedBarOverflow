using RimWorld;

namespace NeedBarOverflow.Patches;

// Some outlier pawns overflowing may skew the average result
// Enabling this does make the game slightly harder
// (maybe more balanced?)
// I am not sure if I should enable this patch yet...
#if false
public sealed class QuestPart_FactionGoodwillForMoodChange_AveragePawnMoodPercent()
	: Patch(transpiler: Add1UpperBound.d_transpiler)
{
	protected override void Initialize()
		=> Target ??= [typeof(QuestPart_FactionGoodwillForMoodChange)
		.Getter("AveragePawnMoodPercent")];
	public override void Toggle()
		=> Toggle(Setting_Common.Enabled(typeof(Need_Mood)));
}
#endif
