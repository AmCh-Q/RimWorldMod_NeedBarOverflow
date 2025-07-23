using NeedBarOverflow.Patches;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersionAttribute("1.4.1.0")]

namespace NeedBarOverflow
{
	public class NeedBarOverflow : Mod
	{
		private static Settings? settings;
		public static Settings Settings
			=> settings ??= ModInstance!.GetSettings<Settings>();
		public static NeedBarOverflow? ModInstance { get; private set; }

		public NeedBarOverflow(ModContentPack content) : base(content)
		{
			Debug.Message("NeedBarOverflow mod constructor called");
			ModInstance = this;
		}

		public override string SettingsCategory()
			=> Strings.Name.Translate();

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoWindowContents(inRect);
			base.DoSettingsWindowContents(inRect);
		}
	}
}
