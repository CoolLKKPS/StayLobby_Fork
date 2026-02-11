using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace StayLobby
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StayLobbyPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            ManualLog = base.Logger;
            localizationManager = new LocalizationManager();
            Harmony.CreateAndPatchAll(typeof(Patches), null);
            GameObject listenerObj = new GameObject("StayLobbyEventListener");
            listenerObj.AddComponent<StayLobbyGameEventListener>();
            DontDestroyOnLoad(listenerObj);
            listenerObj.hideFlags = HideFlags.HideAndDontSave;
            Patches.InitializeFacilityMeltdownIntegration(base.Logger);
        }
        public static ManualLogSource ManualLog;

        public static LocalizationManager localizationManager;
    }
}
