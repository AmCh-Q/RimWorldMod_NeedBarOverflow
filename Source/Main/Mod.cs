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
			Savefilebackcompat.ModifySettingFileClass(content);
			s = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				Debug.Message("NeedBarOverflow LongEvent called");
				Refs.Init();
				Needs.Setting_Common.DisablingDefs.LoadDisabledDefs();
				Needs.Setting_Food.ApplyFoodHediffSettings();
                ApplyPatches();
                if (Settings.migrateSettings == 2)
                    s.Write();
            }, "NeedBarOverflow.Mod.ctor", false, null);
		}
		public override string SettingsCategory() => Strings.Name.Translate();
		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			s.DoWindowContents(inRect);
		}
	}
}
