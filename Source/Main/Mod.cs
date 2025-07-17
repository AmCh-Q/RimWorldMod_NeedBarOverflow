using NeedBarOverflow.Patches;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersionAttribute("1.3.5.0")]

namespace NeedBarOverflow
{
	public class NeedBarOverflow : Mod
	{
		public static Settings? settings;
		public static bool Initialized => settings is not null;

		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow mod constructor called");
			settings = GetSettings<Settings>();
		}

		public override string SettingsCategory()
			=> Strings.Name.Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			settings?.DoWindowContents(inRect);
			base.DoSettingsWindowContents(inRect);
		}
	}
}
