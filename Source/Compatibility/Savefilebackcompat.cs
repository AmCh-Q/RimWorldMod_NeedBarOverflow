using System.IO;
using System.Xml;
using Verse;

namespace NeedBarOverflow
{
    internal static class Savefilebackcompat
    {
        public static bool ModifySettingFileClass(ModContentPack content)
        {
            string settingsFilePath = Path.Combine(
                GenFilePaths.ConfigFolderPath,
                GenText.SanitizeFilename($"Mod_{content.FolderName}_NeedBarOverflow.xml"));
            if (!File.Exists(settingsFilePath))
                return false;
            XmlDocument doc = new XmlDocument();
            doc.Load(settingsFilePath);
            XmlNode ClassAttribute =
                doc?.DocumentElement
                ?.SelectSingleNode("descendant::ModSettings")
                ?.Attributes
                ?.GetNamedItem("Class");
            if (ClassAttribute == null || 
                ClassAttribute.Value != "NeedBarOverflow.NeedBarOverflow_Settings")
                return false;
            ClassAttribute.Value = "NeedBarOverflow.Settings";
            doc.Save(settingsFilePath);
            return true;
        }
    }
}
