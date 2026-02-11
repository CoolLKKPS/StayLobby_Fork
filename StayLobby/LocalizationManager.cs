using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace StayLobby
{
    public class LocalizationManager
    {
        private Dictionary<string, string> strings;

        public LocalizationManager()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var dirPath = Path.GetDirectoryName(assemblyLocation);
            var cfgPath = Path.Combine(dirPath, "Lang", "Language.json");
            if (!File.Exists(cfgPath))
            {
                strings = new Dictionary<string, string>();
                return;
            }
            var cfgJson = File.ReadAllText(cfgPath);
            var cfg = JsonConvert.DeserializeObject<LocaleCfg>(cfgJson);
            string selectedLanguage = cfg.selectedLanguage;
            if (string.IsNullOrEmpty(selectedLanguage))
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture.Name;
                if (culture.StartsWith("zh"))
                    selectedLanguage = "zh_Hans";
                else
                    selectedLanguage = "en_US";
            }
            var langPath = Path.Combine(dirPath, "Lang", selectedLanguage + ".json");
            if (!File.Exists(langPath))
            {
                langPath = Path.Combine(dirPath, "Lang", "en_US.json");
            }
            var langJson = File.ReadAllText(langPath);
            strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(langJson);
        }

        public string GetString(string key)
        {
            if (strings.TryGetValue(key, out string value))
                return value;
            return $"{key}";
        }
    }

    public class LocaleCfg
    {
        public string selectedLanguage { get; set; }

        public Dictionary<string, string> availableLanguage { get; set; }
    }
}