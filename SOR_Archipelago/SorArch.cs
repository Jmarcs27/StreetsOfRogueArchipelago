using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using BepInEx.Configuration;
using SOR_Archipelago.Archipelago;
using SOR_Archipelago.Utils;
using System.Reflection;
using System.CodeDom;
using static UnityEngine.UI.Image;
using System.Xml.Linq;
using UnityEngine.Rendering;
using Google2u;


// Project uses BepInEx

namespace SOR_Archipelago
{
    [BepInPlugin(pluginGuid,pluginName,pluginVersion)]
    public class SorArch : BaseUnityPlugin
    {
        public const string pluginGuid = "jmarcs.streetsofrogue.archipelago";
        public const string pluginName = "Street of Rogue Archipelago Mod";
        public const string pluginVersion = "0.0.0.1";

        public const string ModDisplayInfo = pluginName + " v" + pluginVersion;
        private const string APDisplayInfo = "Archipelago v" + ArchipelagoClient.APVersion;
        public static ManualLogSource BepinLogger;
        public static ArchipelagoClient ArchipelagoClient;

        public List<String> unlockedCharacters = new List<String>(); 
        public int nuggetsCollected = 0;
        public int nuggetsSpent = 0;
        public bool loadedNuggets = false;

        public static SorArch Instance { get; private set; }

        private ConfigEntry<string> configUri;
        private ConfigEntry<string> configSlotName;
        private ConfigEntry<string> configPassword;

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            BepinLogger = Logger;
            ArchipelagoClient = new ArchipelagoClient();
            ArchipelagoConsole.Awake();
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loadeded!");

            // SOR patches initialization logic
            PatchPrefix(typeof(Unlocks), "DoUnlock", "Unlocks_DoUnlock_Patch");
            PatchPrefix(typeof(Unlocks), "IsUnlocked", "Unlocks_IsUnlocked_Patch");
            PatchPrefix(typeof(Unlocks), "AddNuggets", "Unlocks_AddNuggets_Patch");
            // PatchPrefix(typeof(Unlocks), "Awake2", "Unlocks_Start_Patch");
            // PatchPrefix(typeof(StatsScreen), "Start", "StatsScreen_Awake_Patch");

            configUri = Config.Bind("General.ArchipelagoData",      // The section under which the option is shown
                                         "Uri",  // The key of the configuration option in the configuration file
                                         "localhost", // The default value
                                         "The archieplago address. Example: archipelago.gg:38281"); // Description of the option to show in the config file

            configSlotName = Config.Bind("General.ArchipelagoData",
                                                "SlotName",
                                                "Player1",
                                                "The play slot name");
            configPassword = Config.Bind("General.ArchipelagoData",
                                                "Password",
                                                "",
                                                "The password for the archipelago address if applicable");

            ArchipelagoClient.ServerData.Uri = configUri.Value;
            ArchipelagoClient.ServerData.SlotName = configSlotName.Value;
            ArchipelagoClient.ServerData.Password = configPassword.Value;
        }

        // Takes the file type, function name, and patch function name to add the patch as a prefix to the function
        private void PatchPrefix(System.Type o_original, string o_name, string p_name)
        {
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(o_original, o_name);
            MethodInfo patch = AccessTools.Method(typeof(Patches), p_name);
            harmony.Patch(original, new HarmonyMethod(patch));
        }

        private void OnGUI()
        {
            // show the mod is currently loaded in the corner
            GUI.Label(new Rect(16, 16, 300, 20), ModDisplayInfo);
            ArchipelagoConsole.OnGUI();

            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (ArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;

                statusMessage = " Status: Connected";
                GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(16, 50, 300, 20), APDisplayInfo + statusMessage);
                GUI.Label(new Rect(16, 70, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 90, 150, 20), "Player Name: ");
                GUI.Label(new Rect(16, 110, 150, 20), "Password: ");

                ArchipelagoClient.ServerData.Uri = GUI.TextField(new Rect(150, 70, 150, 20),
                    ArchipelagoClient.ServerData.Uri);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150, 90, 150, 20),
                    ArchipelagoClient.ServerData.SlotName);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150, 110, 150, 20),
                    ArchipelagoClient.ServerData.Password);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(16, 130, 100, 20), "Connect") &&
                    !ArchipelagoClient.ServerData.SlotName.IsNullOrWhiteSpace())
                {
                    // Sets the new connection defaults in config
                    configUri.Value = ArchipelagoClient.ServerData.Uri;
                    configSlotName.Value = ArchipelagoClient.ServerData.SlotName;
                    configPassword.Value = ArchipelagoClient.ServerData.Password;
                    ArchipelagoClient.Connect();
                }
            }
            // this is a good place to create and add a bunch of debug buttons
        }

        public IEnumerator WaitForStartup()
        {
            //yield return new WaitUntil(() => gc.loadComplete);
            yield return new WaitForSeconds(4);
        }

        public void HandleItem(string item)
        {
            if (item == "Nuggets")
            {
                nuggetsCollected += 5;
                SorArch.BepinLogger.LogInfo("New nugget count: " + nuggetsCollected);
            }
            else
            {
                if (!unlockedCharacters.Contains(item))
                    unlockedCharacters.Add(item);
            }
        }

        public void HandleVictory()
        {
            ArchipelagoClient.VictoryCheck();
        }
    }
}
