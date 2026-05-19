using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using EnhancedScrollerDemos.SnappingDemo;
using SOR_Archipelago.Utils;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

namespace SOR_Archipelago.Archipelago
{
    public class ArchipelagoClient
    {
        public const string APVersion = "0.5.0";
        private const string Game = "Streets of Rogue";

        public static bool Authenticated;
        private bool attemptingConnection;

        public static ArchipelagoData ServerData = new ArchipelagoData();
        private DeathLinkHandler DeathLinkHandler;
        private ArchipelagoSession session;

        private List<String> conditions = new List<String>();
        Dictionary<string, object> slotData = new Dictionary<string, object>();

        /// <summary>
        /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            if (Authenticated || attemptingConnection) return;

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
                SetupSession();
            }
            catch (Exception e)
            {
                SorArch.BepinLogger.LogError(e);
            }

            TryConnect();
        }

        /// <summary>
        /// add handlers for Archipelago events
        /// </summary>
        private void SetupSession()
        {
            session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += OnSessionErrorReceived;
            session.Socket.SocketClosed += OnSessionSocketClosed;
        }

        /// <summary>
        /// attempt to connect to the server with our connection info
        /// </summary>
        private void TryConnect()
        {
            try
            {
                // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
                ThreadPool.QueueUserWorkItem(
                    _ => HandleConnectResult(
                        session.TryConnectAndLogin(
                            Game,
                            ServerData.SlotName,
                            ItemsHandlingFlags.AllItems,
                            new Version(APVersion),
                            password: ServerData.Password,
                            requestSlotData: ServerData.NeedSlotData
                        )));
            }
            catch (Exception e)
            {
                SorArch.BepinLogger.LogError(e);
                HandleConnectResult(new LoginFailure(e.ToString()));
                attemptingConnection = false;
            }
        }

        /// <summary>
        /// handle the connection result and do things
        /// </summary>
        /// <param name="result"></param>
        private void HandleConnectResult(LoginResult result)
        {
            string outText;
            if (result.Successful)
            {
                var success = (LoginSuccessful)result;

                ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
                slotData = success.SlotData;
                Authenticated = true;

                DeathLinkHandler = new DeathLinkHandler(session.CreateDeathLinkService(), ServerData.SlotName);
#if NET35
                session.Locations.CompleteLocationChecksAsync(null, ServerData.CheckedLocations.ToArray());
#else
                session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
#endif
                outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

                ArchipelagoConsole.LogMessage(outText);
            }
            else
            {
                var failure = (LoginFailure)result;
                outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
                outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

                SorArch.BepinLogger.LogError(outText);

                Authenticated = false;
                Disconnect();
            }

            ArchipelagoConsole.LogMessage(outText);
            attemptingConnection = false;
        }

        /// <summary>
        /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
        /// </summary>
        private void Disconnect()
        {
            SorArch.BepinLogger.LogDebug("disconnecting from server...");
#if NET35
            session?.Socket.Disconnect();
#else
            session?.Socket.DisconnectAsync();
#endif
            session = null;
            Authenticated = false;
        }

        public void SendMessage(string message)
        {
            session.Socket.SendPacketAsync(new SayPacket { Text = message });
        }

        /// <summary>
        /// we received an item so reward it here
        /// </summary>
        /// <param name="helper">item helper which we can grab our item from</param>
        private void OnItemReceived(ReceivedItemsHelper helper)
        {
            SorArch.BepinLogger.LogInfo("OnItemReceived Called!");

            while (helper.Any())
            {
                var receivedItem = helper.DequeueItem();

                string item = receivedItem.ItemName;
                if (item == null) continue;
                if (helper.Index <= ServerData.Index)
                {
                    if (item != "Nugget")
                        SorArch.Instance.HandleItem(item);
                    continue;
                }
                
                SorArch.BepinLogger.LogInfo("Item Recieved: " + receivedItem.ItemName + ". Current helper index is: " + helper.Index + ". Current server index is: " + ServerData.Index);
                ServerData.Index = helper.Index;

                // rewards the item here
                SorArch.Instance.HandleItem(item);
                
                // TODO if items can be received while in an invalid state for actually handling them, they can be placed in a local
                // queue/collection to be handled later
            }
        }

        public List<string> getItemsRecieved()
        {
            List<long> items = session.Items.AllItemsReceived.Select(item => item.ItemId).ToList();
            SorArch.BepinLogger.LogInfo("Number of items already received: " + items.Count);
            foreach (int s in items) SorArch.BepinLogger.LogInfo("All items recieved: " + s);
            return null;
        }

        /// <summary>
        /// mark location as completed
        /// </summary>
        /// <param name="_location">the location name to be marked as completed</param>
        public void LocationCompleted(string _location)
        {
            // Gets items archipelago id. Prints an error if not found.
            long location = session.Locations.GetLocationIdFromName("Streets of Rogue", _location);
            if (location < 0)
            {
                SorArch.BepinLogger.LogError("Could not find thislocation: " + _location);
                return;
            }

            // TODO add try catch in case connection is lost. Add location to queue for reconnect.
            session.Locations.CompleteLocationChecks(location);
            SorArch.BepinLogger.LogError("Location sent to archipelago server: " + _location);
        }

        /// <summary>
        /// something went wrong with our socket connection
        /// </summary>
        /// <param name="e">thrown exception from our socket</param>
        /// <param name="message">message received from the server</param>
        private void OnSessionErrorReceived(Exception e, string message)
        {
            SorArch.BepinLogger.LogError(e);
            ArchipelagoConsole.LogMessage(message);
        }

        /// <summary>
        /// something went wrong closing our connection. disconnect and clean up
        /// </summary>
        /// <param name="reason"></param>
        private void OnSessionSocketClosed(string reason)
        {
            SorArch.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }

        public void VictoryCheck()
        {
            bool mayorOnly = Convert.ToBoolean(slotData["mayor_only"]);
            bool becomeMayor = Convert.ToBoolean(slotData["become_mayor"]);
            int bqCompleted = Convert.ToInt32(slotData["big_quests_completed"]);
            int achievementsCompleted = Convert.ToInt32(slotData["achievements_completed"]);

            bool becameMayor = session.Locations.AllLocationsChecked.ToList().Contains(session.Locations.GetLocationIdFromName("Streets of Rogue", "CompleteLevel6"));
            if (mayorOnly)
            {
                if (becameMayor)
                {
                    SorArch.BepinLogger.LogInfo("Game completed! Goa5l is now set as achieved. Congrats on becoming mayor! Thank you for playing my mod<3");
                    session.SetGoalAchieved();
                }
                return;
            }

            List<long> locations = session.Locations.AllLocationsChecked.ToList();
            foreach (long location in locations)
            {
                SorArch.BepinLogger.LogInfo("Location Checked: " + session.Locations.GetLocationNameFromId(location, "Streets of Rogue"));
            }

            int bq = 0;
            int achievements = 0;
            foreach (long location in locations)
            {
                string l = session.Locations.GetLocationNameFromId(location, "Streets of Rogue");
                if (l.Contains("_BQ"))
                    bq++;
                else
                    achievements++;
            }

            if (bq >= bqCompleted && achievements >= achievementsCompleted)
            {
                if (becomeMayor)
                {
                    if (becameMayor)
                    {
                        SorArch.BepinLogger.LogInfo("Game completed! Goal is now set as achieved. Congrats on completeing all your quests and becomings mayor! Thank you for playing my mod<3");
                        session.SetGoalAchieved();
                    }
                    return;
                }
                SorArch.BepinLogger.LogInfo("Game completed! Goal is now set as achieved. Congrats on completeing all your quests! Thank you for playing my mod<3");
                session.SetGoalAchieved();
            }

        }
    }
}