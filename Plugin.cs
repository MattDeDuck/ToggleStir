using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ToggleStir
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }
        public static string pluginLoc = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static float speedSetting;
        public static string toggleButton;
        public JObject settingsObj;

        public static bool isActive = false;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Log = this.Logger;

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        public void Update()
        {
            List<Potion.UsedComponent> cauldronIngredients = Managers.Potion.usedComponents;
            if (cauldronIngredients.Count > 0)
            {
                // Get the toggle button
                settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                toggleButton = (string)settingsObj["toggleButton"];

                if (Input.GetKeyDown(toggleButton))
                {
                    isActive = !isActive;
                }
            }

            if(isActive)
            {
                // Get the speed setting
                settingsObj = JObject.Parse(File.ReadAllText(pluginLoc + "/settings.json"));
                speedSetting = (float)settingsObj["speed"];

                // Stir the cauldron
                float num = speedSetting * Managers.RecipeMap.indicatorSettings.indicatorSpeed;
                if (num > Mathf.Epsilon)
                {
                    Managers.RecipeMap.indicator.lengthToDeleteFromPath += num;
                    if (Managers.Potion.potionStartedAt == null)
                    {
                        Managers.Potion.potionStartedAt = Managers.RecipeMap.currentMap;
                    }
                    if (Managers.Potion.usedComponents.Count > 0)
                    {
                        Books.GoalsBook.GoalsLoader.GetGoalByName("MoveIndicator", true).ProgressIncrement(1);
                    }
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(IndicatorMapItem), "OnIndicatorRuined")]
        public static void OnIndicatorRuined_Prefix()
        {
            // If potion fails...turn the toggle off
            isActive = false;
        }
    }
}