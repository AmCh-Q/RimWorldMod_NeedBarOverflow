using System.IO;
using System.Xml;
using Verse;

namespace NeedBarOverflow
{
	internal static class Savefilebackcompat
	{
		public static void ModifySettingFileClass(ModContentPack content)
		{
			string settingsFilePath = Path.Combine(
				GenFilePaths.ConfigFolderPath,
				GenText.SanitizeFilename($"Mod_{content.FolderName}_NeedBarOverflow.xml"));
			if (!File.Exists(settingsFilePath))
				return;
			XmlDocument doc = new XmlDocument();
			doc.Load(settingsFilePath);
			XmlNode Node_ModSettings = doc?["SettingsBlock"]?["ModSettings"];
			if (Node_ModSettings == null)
				return;
            XmlNode ClassAttribute = Node_ModSettings.Attributes?.GetNamedItem("Class");
			if (ClassAttribute != null &&
                ClassAttribute.Value == "NeedBarOverflow.NeedBarOverflow_Settings")
            {
                ClassAttribute.Value = "NeedBarOverflow.Settings";
                doc.Save(settingsFilePath);
				Settings.migrateSettings = 1;
                return;
            }
            if (Node_ModSettings["enabledA"] != null)
                Settings.migrateSettings = 1;
		}
	}
}
