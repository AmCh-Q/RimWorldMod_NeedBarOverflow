using UnityEngine;
using Verse;

namespace NeedBarOverflow
{
	using static Patches.PatchApplier;
	public class NeedBarOverflow : Mod
	{
		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow constructor called");
			s = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				Debug.Message("NeedBarOverflow LongEvent called");
				Refs.Init();
				ApplyPatches();
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
