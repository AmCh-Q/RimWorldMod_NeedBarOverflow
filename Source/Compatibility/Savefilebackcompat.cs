#if l1_5
using System.IO;
using System.Xml;
using Verse;

namespace NeedBarOverflow
{
	internal static class Savefilebackcompat
	{
		// Old settings file compatibility, removed for 1.6
		public static void ModifySettingFileClass(ModContentPack content)
		{
			string settingsFilePath = Path.Combine(
				GenFilePaths.ConfigFolderPath,
				GenText.SanitizeFilename($"Mod_{content.FolderName}_NeedBarOverflow.xml"));
			if (!File.Exists(settingsFilePath))
				return;
			XmlDocument doc = new();
			StreamReader streamReader = new(settingsFilePath);
			doc.Load(XmlReader.Create(streamReader));
			streamReader.Dispose();
			XmlNode? Node_ModSettings = doc["SettingsBlock"]?["ModSettings"];
			if (Node_ModSettings is null)
				return;
			XmlNode? ClassAttribute = Node_ModSettings.Attributes?.GetNamedItem("Class");
			if (ClassAttribute?.Value == "NeedBarOverflow.NeedBarOverflow_Settings")
			{
				ClassAttribute.Value = "NeedBarOverflow.Settings";
				doc.Save(settingsFilePath);
				Settings.migrateSettings = 1;
				return;
			}
			if (Node_ModSettings["enabledA"] is not null)
				Settings.migrateSettings = 1;
		}
	}
}
#endif
