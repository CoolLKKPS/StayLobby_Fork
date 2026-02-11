using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace StayLobby
{
    public static class Patches
    {
        // After FinishGeneratingLevel, game must started now.
        [HarmonyPriority(0)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
        private static void StartOfRoundCalled()
        {
            StayLobbyGameEventListener.StartOfRoundCalled = true;
        }

        [HarmonyPriority(0)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        private static void EndOfRoundCalled()
        {
            StayLobbyGameEventListener.EndOfRoundCalled = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
        [HarmonyWrapSafe]
        [HarmonyPriority(0)]
        [HarmonyAfter(new string[] { "mattymatty.LobbyControl" })]
        private static void Postfix(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (((NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsHost) || StartOfRound.Instance.IsHost) && request.ClientNetworkId != NetworkManager.Singleton.LocalClientId && (response.Reason == "Ship has already landed!" || response.Reason == "Game has already started!"))
            {
                if (StayLobbyGameEventListener.InGame)
                {
                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    int num = 0;
                    int num2 = 0;
                    foreach (PlayerControllerB playerControllerB in allPlayerScripts)
                    {
                        if (playerControllerB.isHostPlayerObject || playerControllerB.actualClientId != 0UL)
                        {
                            num++;
                            if (playerControllerB.isPlayerDead)
                            {
                                num2++;
                            }
                        }
                    }
                    int num3 = (int)(TimeOfDay.Instance.normalizedTimeOfDay * (60f * (float)TimeOfDay.Instance.numberOfHours)) + 360;
                    int num4 = (int)Mathf.Floor((float)(num3 / 60));
                    bool flag = false;
                    if (num4 > 12)
                    {
                        flag = true;
                        num4 %= 12;
                    }
                    num3 %= 60;
                    if (flag)
                    {
                        num4 += 12;
                    }
                    string text = "Ship has already landed!\r\n{0}\r\nAlive: {1}/{2}\r\nTime: {3}:{4}";
                    object[] array2 = new object[5];
                    int num5 = 0;
                    SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
                    array2[num5] = ((currentLevel != null) ? currentLevel.PlanetName : null);
                    array2[1] = num - num2;
                    array2[2] = num;
                    array2[3] = num4.ToString("00");
                    array2[4] = num3.ToString("00");
                    response.Reason = string.Format(text, array2);
                    response.Approved = false;
                }
                else
                {
                    response.Reason = "";
                    response.Approved = true;
                }
            }
        }

        private static Action registeredMeltdownCallback;

        public static void InitializeFacilityMeltdownIntegration(ManualLogSource logger)
        {
            try
            {
                if (Type.GetType("FacilityMeltdown.MeltdownPlugin, FacilityMeltdown") == null)
                {
                    logger.LogInfo("Could not find FacilityMeltdown mod, skipping");
                }
                else
                {
                    logger.LogInfo("FacilityMeltdown detected, hooking into meltdown events...");
                    Type type = Type.GetType("FacilityMeltdown.API.MeltdownAPI, FacilityMeltdown");
                    if (type == null)
                    {
                        logger.LogWarning("Could not find MeltdownAPI type");
                    }
                    else
                    {
                        MethodInfo method = type.GetMethod("RegisterMeltdownListener", BindingFlags.Static | BindingFlags.Public);
                        if (method == null)
                        {
                            logger.LogWarning("Could not find RegisterMeltdownListener method in MeltdownAPI");
                        }
                        else
                        {
                            logger.LogInfo("Found RegisterMeltdownListener method in MeltdownAPI");
                            Action action = new Action(() => OnFacilityMeltdownStarted(logger));
                            method.Invoke(null, new object[] { action });
                            registeredMeltdownCallback = action;
                            logger.LogInfo("Successfully registered as a meltdown listener!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to hook into FacilityMeltdown: " + ex.Message);
                logger.LogDebug("Exception details: " + ex.ToString());
            }
        }

        private static void OnFacilityMeltdownStarted(ManualLogSource logger)
        {
            logger.LogInfo("FacilityMeltdown meltdown event detected! Triggering...");
            StayLobbyGameEventListener.OnFacilityMeltdownStarted?.Invoke();
        }
    }
}
