using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StayLobby
{
    public class StayLobbyGameEventListener : MonoBehaviour
    {
        public static StayLobbyGameEventListener Instance { get; private set; }

        private void Awake()
        {
            this.GameEventListenerLogger = BepInEx.Logging.Logger.CreateLogSource("StayLobbyGameEventListener");
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            CheckShipInOrbit();
            CheckStartOfRound();
            CheckTimeOfDaySundown();
            CheckShipLeavingAlertCalled();
            CheckEndOfRound();
        }

        private T UpdateCached<T>(string key, T currentValue, T defaultValue)
        {
            object obj;
            if (!this.previousValues.TryGetValue(key, out obj))
            {
                obj = defaultValue;
            }
            this.previousValues[key] = currentValue;
            return (T)((object)obj);
        }

        private void GetLobbyName()
        {
            if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost)
            {
                GameEventListenerLogger.LogDebug("Getting lobby name");
                lobbyName = GameNetworkManager.Instance.currentLobby.Value.GetData("name");
                GameEventListenerLogger.LogDebug($"Lobby name obtained: {lobbyName}");
            }
        }

        private void RestoreLobbyName()
        {
            if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost)
            {
                GameEventListenerLogger.LogDebug($"Restoring lobby name to: {lobbyName}");
                GameNetworkManager.Instance.currentLobby.Value.SetData("name", lobbyName ?? "");
                GameEventListenerLogger.LogDebug("Lobby name restored");
            }
        }

        private void CheckShipInOrbit()
        {
            bool flag = StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase;
            bool flag2 = this.UpdateCached<bool>("ShipInOrbit", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListenerLogger.LogDebug("Ship is in orbit");
                StartOfRoundCalled = false;
                InGame = false;
                EndOfRoundCalled = false;
                PreventNormalUpdate = false;
            }
        }

        private void CheckStartOfRound()
        {
            bool flag = StartOfRound.Instance != null && StartOfRoundCalled;
            bool flag2 = this.UpdateCached<bool>("StartOfRound", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListenerLogger.LogDebug("Start of round");
                StartOfRoundCalled = false;
                InGame = true;
                GameNetworkManager.Instance.SetLobbyJoinable(true);
                if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost)
                {
                    GetLobbyName();
                    string originalName = lobbyName;
                    string newName = "[DAY] " + originalName;
                    GameNetworkManager.Instance.currentLobby.Value.SetData("name", newName);
                    GameEventListenerLogger.LogDebug($"Lobby name changed to '{newName}'");
                }
            }
        }

        private void CheckTimeOfDaySundown()
        {
            if (TimeOfDay.Instance == null)
            {
                return;
            }
            float normalizedTime = TimeOfDay.Instance.normalizedTimeOfDay;
            bool flag = normalizedTime > 0.63f;
            bool flag2 = UpdateCached<bool>("TimeOfDaySundown", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListenerLogger.LogDebug("Time of day is sundown");
                if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost && !PreventNormalUpdate)
                {
                    string originalName = lobbyName;
                    string newName = "[NIGHT] " + originalName;
                    GameNetworkManager.Instance.currentLobby.Value.SetData("name", newName);
                    GameEventListenerLogger.LogDebug($"Lobby name changed to '{newName}'");
                }
            }
        }

        private void CheckShipLeavingAlertCalled()
        {
            bool flag = TimeOfDay.Instance != null && TimeOfDay.Instance.shipLeavingAlertCalled;
            bool flag2 = this.UpdateCached<bool>("ShipLeavingAlertCalled", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListenerLogger.LogDebug("Ship leaving alert called");
                if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost && !PreventNormalUpdate)
                {
                    string originalName = lobbyName;
                    string newName = "[FINAL] " + originalName;
                    GameNetworkManager.Instance.currentLobby.Value.SetData("name", newName);
                    GameEventListenerLogger.LogDebug($"Lobby name changed to '{newName}'");
                    PreventNormalUpdate = true;
                }
            }
        }

        private static void OnMeltdownStarted()
        {
            if (GameNetworkManager.Instance?.currentLobby != null && StartOfRound.Instance != null && StartOfRound.Instance.IsHost && !PreventNormalUpdate)
            {
                string originalName = lobbyName;
                string newName = "[MELTDOWN] " + originalName;
                GameNetworkManager.Instance.currentLobby.Value.SetData("name", newName);
                Instance.GameEventListenerLogger.LogDebug($"Lobby name changed to '{newName}'");
                PreventNormalUpdate = true;
            }
        }

        private void CheckEndOfRound()
        {
            bool flag = StartOfRound.Instance != null && EndOfRoundCalled;
            bool flag2 = this.UpdateCached<bool>("EndOfRound", flag, false);
            if (flag == flag2)
            {
                return;
            }
            if (flag)
            {
                GameEventListenerLogger.LogDebug("End of round");
                EndOfRoundCalled = false;
                GameNetworkManager.Instance.SetLobbyJoinable(false);
                RestoreLobbyName();
            }
        }

        private ManualLogSource GameEventListenerLogger;

        public static string lobbyName { get; set; }

        private readonly Dictionary<string, object> previousValues = new Dictionary<string, object>();

        public static bool StartOfRoundCalled { get; set; }

        public static bool InGame { get; set; }

        public static bool EndOfRoundCalled { get; set; }

        public static bool PreventNormalUpdate { get; set; }

        public static Action OnFacilityMeltdownStarted = OnMeltdownStarted;
    }
}