using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace StayLobby
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StayLobbyPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            ManualLog = base.Logger;
            Harmony.CreateAndPatchAll(typeof(Patches), null);
        }
        public static ManualLogSource ManualLog;
    }
}
