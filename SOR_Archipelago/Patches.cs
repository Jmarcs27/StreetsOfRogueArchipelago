using Google2u;
using System;
using System.Collections.Generic;

namespace SOR_Archipelago
{
    public class Patches
    {
        // Used to unlock things for other players
        public static bool Unlocks_DoUnlock_Patch(GameController ___gc, string unlockName, string unlockType)
        {
            SorArch.BepinLogger.LogInfo("playing as: " + ___gc.playerAgent.agentName);
            if (unlockType == "BigQuest" && !unlockName.Contains(___gc.playerAgent.agentName))
            {
                SorArch.BepinLogger.LogError("Attempted to complete: " + unlockName + ", but currently playing as: " + ___gc.playerAgent.agentName);
                return true;
            }
            SorArch.ArchipelagoClient.LocationCompleted(unlockName);
            SorArch.BepinLogger.LogInfo("Location completed: " + unlockName);
            // Handles checking archipelago completion
            SorArch.Instance.HandleVictory();
            if (unlockType == "Agent")
                return false;
            return true;
        }

        // Overrides the unlocked characters in character select to only include those unlocked via archipelago
        public static bool Unlocks_IsUnlocked_Patch(GameController ___gc, string unlockName, string unlockType, ref bool __result)
        {

            if (SorArch.Instance == null || ___gc == null)
            {
                SorArch.BepinLogger.LogError("SorArch instance or ___gc object is null in Unlocks_IsUnlocked_Patch when unlocking: " + unlockName);
                return false;
            }

            int nuggetsSpent = 0;
            foreach (Unlock unlock in ___gc.sessionDataBig.unlocks)
            {
                // Handles getting nuggets already spent
                if (unlock.unlockType == "Trait" && unlock.unlocked == true && unlock.cost != 0)
                    nuggetsSpent += unlock.cost;
            }
            ___gc.sessionDataBig.nuggets = SorArch.Instance.nuggetsCollected - nuggetsSpent;

            // Handles actual character unlocks
            if (unlockType == "Agent" && SorArch.Instance.unlockedCharacters.Contains(unlockName))
            {
                List<String> dlc = new List<String>() { "Alien", "MechPilot", "Demolitionist", "Guard", "Courier", "Bouncer" };
                if (!___gc.dcContent && dlc.Contains(unlockName))
                { // Prevents dlc character from being unlocked if the dlc is not owned. Never disable!
                    SorArch.BepinLogger.LogInfo("No DLC = No DLC Characters!");
                    __result = false;
                }
                else
                {
                    __result = true;
                }
                    
                return false;
            }
            else if (unlockType == "Agent")
            {
                __result = false;
                return false;
            }

            return true;
        }
        
        // Overrides adding nuggets to only include those unlocked via archipelago
        public static bool Unlocks_AddNuggets_Patch(GameController ___gc, Unlocks __instance, int numNuggets)
        {
            SorArch.BepinLogger.LogInfo("You are playing: " + ___gc.playerAgent.agentName);
            /*
            int nuggets = SorArch.Instance.nuggetsCollected;

            if (nuggets > 0)
            {
                ___gc.sessionDataBig.nuggets += nuggets;
                SorArch.Instance.nuggetsCollected = 0;
            }
            */
            return false;
        }
        
        public static bool Unlocks_Start_Patch(GameController ___gc)
        {
            if(___gc == null)
            {
                SorArch.BepinLogger.LogInfo("___gc null in Unlock.cs Start function.");
                return false;
            }

            SorArch.BepinLogger.LogInfo("Unlock Start Function Called, AKA nugget time.");
            int nuggets = SorArch.Instance.nuggetsCollected;

            if (nuggets != 0 && SorArch.Instance.loadedNuggets != false)
            {
                ___gc.sessionDataBig.nuggets = nuggets;
                SorArch.Instance.loadedNuggets = true;
            }
            else
            {
                ___gc.sessionDataBig.nuggets += nuggets;
            }
            SorArch.Instance.nuggetsCollected = 0;
            return false;
        }
    }
}
