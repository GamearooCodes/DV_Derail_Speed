using System;
using System.IO;
using System.Reflection;
using DV;
using DV.Customization;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace Derial_Speed
{
    internal static class Main
    {
        private static bool enabled;
        private static UnityModManager.ModEntry myModEntry;
        private static Harmony myHarmony;

        private const float SpeedLimit = 120f; // Speed threshold for derailing
        private static float originalThreshold;
        private static bool derailEnabled = false; // Tracks derail state

        public static ModConfig config;

        // Token: 0x0400000C RID: 12
        public static readonly string configFilePath = Path.Combine(UnityModManager.modsPath, "Derial_Speed/Derial_Speed.json");

        private enum DebugLevel
        {
            None,
            Error,
            Warning,
            Info,
            Debug
        }

        private static DebugLevel currentDebugLevel = DebugLevel.Debug;

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
            GUILayout.Label("Derial_Speed", Array.Empty<GUILayoutOption>());
            string text = GUILayout.TextField(Main.config.Derial_Speed.ToString(), new GUILayoutOption[] { GUILayout.Width(300f) });

            // Check if the input text has changed
            bool flag = text != Main.config.Derial_Speed.ToString();

            // If changed, try to parse the input string as a float and update Derial_Speed
            if (flag)
            {
                if (float.TryParse(text, out float newSpeed))
                {
                    Main.config.Derial_Speed = newSpeed;
                   
                }
                else
                {
                    // Handle invalid input (e.g., non-numeric input)
                    GUILayout.Label("Invalid input! Please enter a valid number.", Array.Empty<GUILayoutOption>());
                }
            }

            bool flag4 = GUILayout.Button("Save Configuration", Array.Empty<GUILayoutOption>());
            if (flag4)
            {
                try
                {
                    Main.config.Save(Main.configFilePath);
                    modEntry.Logger.Log("Configuration saved successfully.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Failed to save configuration: " + ex.Message);
                }
            }

            GUILayout.EndVertical();



        }

            private static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                myModEntry = modEntry;
                modEntry.OnToggle = OnToggle;
                modEntry.OnUnload = OnUnload;
                modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
                myHarmony = new Harmony(myModEntry.Info.Id);
                myHarmony.PatchAll(Assembly.GetExecutingAssembly());
                Main.config = ModConfig.Load(Main.configFilePath);
                originalThreshold = Globals.G.GameParams.defaultStressThreshold;

                modEntry.OnUpdate = OnGameUpdate; // Hook into UnityModManager's update loop
                Log("Mod Loaded Successfully!", DebugLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Failed to load {myModEntry.Info.DisplayName}: {ex.Message}", DebugLevel.Error);
                myHarmony?.UnpatchAll(myModEntry.Info.Id);
                return false;
            }
            return true;
        }

        private static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            myHarmony?.UnpatchAll(myModEntry.Info.Id);
             // Restore the derail threshold when unloading
            Log("Mod Unloaded Successfully!", DebugLevel.Info);
            return true;
        }


       

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value == enabled) return true;

            enabled = value;
            if (enabled)
            {
                myHarmony.PatchAll(Assembly.GetExecutingAssembly());
                Log("Mod Enabled!", DebugLevel.Info);
            }
            else
            {
                myHarmony.UnpatchAll(myModEntry.Info.Id);
                
                Log("Mod Disabled!", DebugLevel.Info);
            }
            return true;
        }

        private static float elapsedTime = 0f;  // Elapsed time tracker
        private static float checkInterval = 5f; // 5 seconds interval

        private static void CheckSpeedAndSayHello()
        {
            if (!enabled) return;

            // Get the player's current car
            TrainCar currentCar = PlayerManager.Car;
            float speed = currentCar != null ? Mathf.Abs(currentCar.GetForwardSpeed()) * 3.6f : 0f; // Default to 0 if no car is found
            
            // Log the speed for debugging
         //  Log(currentCar == null ? "Player's current car is null. Defaulting speed to 0." : $"Current car speed: {speed}", DebugLevel.Debug);
            
            // Check if the speed is above 120 and log "Hello!"
            if (speed > Main.config.Derial_Speed)
            {
                Globals.G.GameParams.DerailStressThreshold = Globals.G.GameParams.defaultStressThreshold;
                // Replace with Debug.Log("Hello!") if no UI message system is implemented
               // Log($"Speed is above 120: {speed}", DebugLevel.Debug);
            } else
            {
                Globals.G.GameParams.DerailStressThreshold = float.PositiveInfinity;
            }
        }


        private static void ShowMessage(string message)
        {
            Debug.Log(message); // Simple message in the Unity console

            // If you want an on-screen message, use Unity UI
            // Implement a method to display messages on-screen
        }

        private static void Log(string message, DebugLevel level)
        {
            if (level <= currentDebugLevel)
            {
                switch (level)
                {
                    case DebugLevel.Error:
                        myModEntry.Logger.Error(message);
                        break;
                    case DebugLevel.Warning:
                        myModEntry.Logger.Warning(message);
                        break;
                    case DebugLevel.Info:
                    case DebugLevel.Debug:
                        myModEntry.Logger.Log(message);
                        break;
                }
            }
        }

        // Called periodically by UnityModManager
        private static void OnGameUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (enabled)
            {
                elapsedTime += dt; // Increment elapsed time by deltaTime (the time passed in seconds)

                if (elapsedTime >= checkInterval) // Check if 5 seconds have passed
                {
                    elapsedTime = 0f; // Reset the timer
                    CheckSpeedAndSayHello(); // Call your function
                }
            }
        }
    }
}
