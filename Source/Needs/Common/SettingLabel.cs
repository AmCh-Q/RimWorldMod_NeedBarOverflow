using Verse;

namespace NeedBarOverflow.Needs
{
	public class SettingLabel
	{
		public readonly string name, label, tip;

		public string TranslatedLabel()
		  => label.MyTranslate();
		public string TranslatedLabel(NamedArgument arg)
		  => label.MyTranslate(arg);
		public string TranslatedTip()
		  => tip.MyTranslate();
		public string TranslatedTip(NamedArgument arg)
		  => tip.MyTranslate(arg);
		public SettingLabel(string typeName, string settingsName)
		{
			string text = typeName.NullOrEmpty() ? string.Empty : ".";
			name = settingsName.Replace('_', '.');
			label = "NBO." + typeName + text + name;
			tip = label + "_Tip";
		}
	}
}
