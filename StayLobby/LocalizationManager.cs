using Newtonsoft.Json;
using System;
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
                Directory.CreateDirectory(Path.GetDirectoryName(cfgPath));
                var defaultcfg = new LocalizationFile
                {
                    selectedLanguage = "en_US",
                    availableLanguage = new SortedDictionary<string, string>()
                };
                File.WriteAllText(cfgPath, JsonConvert.SerializeObject(defaultcfg, Formatting.Indented));
            }

            LocalizationFile cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<LocalizationFile>(File.ReadAllText(cfgPath));
                if (cfg == null)
                {
                    throw new JsonException("Deserialized object is null");
                }
            }
            catch (Exception)
            {
                cfg = new LocalizationFile
                {
                    selectedLanguage = "en_US",
                    availableLanguage = new SortedDictionary<string, string>()
                };
                File.WriteAllText(cfgPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }

            var selectedLanguage = cfg.selectedLanguage;
            var availableLanguages = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.GetFiles(Path.Combine(dirPath, "Lang"), "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!name.Equals("Language", System.StringComparison.OrdinalIgnoreCase))
                    availableLanguages.Add(name);
            }
            if (cfg.availableLanguage == null)
                cfg.availableLanguage = new SortedDictionary<string, string>();
            bool hasChanges = false;
            if (string.IsNullOrEmpty(cfg.selectedLanguage))
            {
                cfg.selectedLanguage = "en_US";
                hasChanges = true;
                selectedLanguage = cfg.selectedLanguage;
            }
            foreach (var lang in availableLanguages)
            {
                if (!cfg.availableLanguage.ContainsKey(lang))
                {
                    cfg.availableLanguage[lang] = lang;
                    hasChanges = true;
                }
            }
            var keysToRemove = new List<string>();
            foreach (var lang in cfg.availableLanguage.Keys)
            {
                if (!availableLanguages.Contains(lang))
                {
                    keysToRemove.Add(lang);
                }
            }
            foreach (var lang in keysToRemove)
            {
                cfg.availableLanguage.Remove(lang);
                hasChanges = true;
            }
            if (hasChanges)
            {
                File.WriteAllText(cfgPath, JsonConvert.SerializeObject(cfg, Formatting.Indented));
            }

            if (string.IsNullOrEmpty(selectedLanguage) || !availableLanguages.Contains(selectedLanguage))
            {
                selectedLanguage = GetLanguageFromCulture();
            }

            var langPath = Path.Combine(dirPath, "Lang", selectedLanguage + ".json");
            if (!File.Exists(langPath))
            {
                langPath = Path.Combine(dirPath, "Lang", "en_US.json");
            }
            try
            {
                strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(langPath));
            }
            catch (Exception)
            {
                langPath = Path.Combine(dirPath, "Lang", "en_US.json");
                try
                {
                    strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(langPath));
                }
                catch (Exception)
                {
                    strings = new Dictionary<string, string>();
                }
            }
        }

        private static string GetLanguageFromCulture()
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            var ietfTag = culture.IetfLanguageTag;
            if (ietfTag.StartsWith("zh-Hans", System.StringComparison.OrdinalIgnoreCase) ||
                ietfTag.StartsWith("zh-CHS", System.StringComparison.OrdinalIgnoreCase))
            {
                return "zh_Hans";
            }
            return "en_US";
        }

        /*
        private static void SerializeFormatting(string path, object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.IndentChar = ' ';
                jsonWriter.Indentation = 2;
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, value);
                File.WriteAllText(path, stringWriter.ToString());
            }
        }
        */

        public string GetString(string key)
        {
            if (strings.TryGetValue(key, out string value))
                return value;
            return $"{key}";
        }
    }

    public class LocalizationFile
    {
        public string selectedLanguage { get; set; }

        public SortedDictionary<string, string> availableLanguage { get; set; }
    }
}