// using GameNetcodeStuff;
using HarmonyLib;
// using Unity.Netcode;
// using UnityEngine;

namespace StayLobby
{
    public static class Patches
    {
        public static bool gameStart { get; set; }

        public static string lobbyName { get; set; }

        // After FinishGeneratingLevel, game must started now.
        [HarmonyPriority(0)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
        private static void GameStarted()
        {
            gameStart = true;

            if (GameNetworkManager.Instance.currentLobby != null)
            {
                lobbyName = GameNetworkManager.Instance.currentLobby.Value.GetData("name");

                GameNetworkManager.Instance.currentLobby.Value.SetData("name", "[GameStarted] " + lobbyName);
                GameNetworkManager.Instance.SetLobbyJoinable(true);
                return;
            }
            StayLobbyPlugin.ManualLog.LogWarning("GameStarted: currentLobby is null");
        }

        [HarmonyPriority(0)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        private static void EndOfRound()
        {
            if (GameNetworkManager.Instance.currentLobby != null)
            {
                // GameNetworkManager.Instance.currentLobby.Value.SetData("name", "[EndOfRound] " + Patches.lobbyName);
                GameNetworkManager.Instance.currentLobby.Value.SetData("name", lobbyName ?? "");
                GameNetworkManager.Instance.SetLobbyJoinable(false);
                return;
            }
            StayLobbyPlugin.ManualLog.LogWarning("EndOfRound: currentLobby is null");
        }

        [HarmonyPriority(0)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "SetShipReadyToLand")]
        private static void ShipReadyToLand()
        {
            gameStart = false;

            /*
            if (GameNetworkManager.Instance.currentLobby != null)
            {
                GameNetworkManager.Instance.currentLobby.Value.SetData("name", Patches.lobbyName ?? "");
                return;
            }
            StayLobbyPlugin.ManualLog.LogWarning("ShipReadyToLand: currentLobby is null");
            */
        }

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "ConnectionApproval")]
        [HarmonyWrapSafe]
        [HarmonyPriority(0)]
        [HarmonyAfter(new string[] { "mattymatty.LobbyControl" })]
        private static void Postfix(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (request.ClientNetworkId != NetworkManager.Singleton.LocalClientId && (response.Reason == "Ship has already landed!" || response.Reason == "Game has already started!"))
            {
                if (gameStart)
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
        */
    }
}
