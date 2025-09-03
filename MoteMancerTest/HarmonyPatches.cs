using BepInEx;
using BepInEx.Logging;
using Data;
using DG.Tweening;
using HarmonyLib;
using Motemancer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Xml;
using UnityEngine;


namespace SandboxMode
{
    [BepInPlugin(GUID, NAME, VERSION)]

    public class SandboxModeMod : BaseUnityPlugin
    {
        
        private const string GUID = "nc.motemancermods.sandboxmode";
        private const string NAME = "sandbox mode enabler";
        private const string VERSION = "1.0.0.1";

        internal static ManualLogSource Log;

        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            Log = Logger;
            harmony.PatchAll();
        }


        [HarmonyPatch]
        public static class EnableSandboxMode
        {
            [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.SkipTutorial))]
            [HarmonyPostfix]
            public static void UnlockResearchAndSetCraftable()
            {
                Log.LogInfo("Unlocking Research and setting craft status");
                foreach (ResearchData research in DatabaseManager.I.m_researchDatabase.researches)
                {
                    ResearchBanner.m_showBanner = false;
                    {
                        ResearchManager.I.ConfirmCompleteResearch(research);
                    }
                    ResearchBanner.m_showBanner = true;
                }

                foreach(RecipeData recipe in DatabaseManager.I.m_recipeDatabase.recipes)
                {
                    recipe.m_notCraftable = false;
                }
            }
            [HarmonyPatch(typeof(GameMenu), nameof(GameMenu.StartLoad))]
            [HarmonyPostfix]
            public static void UnlockResearchAndSetCraftableOnLoad()
            {
                Log.LogInfo("Unlocking Research and setting craft status");
                foreach (ResearchData research in DatabaseManager.I.m_researchDatabase.researches)
                {
                    ResearchBanner.m_showBanner = false;
                    {
                        ResearchManager.I.ConfirmCompleteResearch(research);
                    }
                    ResearchBanner.m_showBanner = true;
                }

                foreach (RecipeData recipe in DatabaseManager.I.m_recipeDatabase.recipes)
                {
                    recipe.m_notCraftable = false;
                }
            }

            [HarmonyPatch(typeof(StructureUtils), nameof(StructureUtils.BlockOpposingPlaneBuilding))]
            [HarmonyPostfix]
            public static void BuildEverywhere(ref bool __result)
            {
                __result = false;
            }

            [HarmonyPatch(typeof(CraftCount), nameof(CraftCount.CalculateCraftableQuantity))]
            [HarmonyPrefix]
            public static bool MaxCraft(ref int __result)
            {
                __result = int.MaxValue;
                return false;
            }


            [HarmonyPatch(typeof(CraftingCorner), nameof(CraftingCorner.CraftItem))]
            [HarmonyPrefix]
            public static bool AddInventory(EntityData entity, int count)
            {
                Player.I.PlayerInventory.AddEntity(entity, count, false);
                return false;
            }

            [HarmonyPatch(typeof(StructurePreview), nameof(StructurePreview.SetImage))]
            [HarmonyPrefix]
            public static bool FixRotationNullError(StructureData structureData, int index, StructurePreview __instance)
            {
                if (index == -1)
                {
                    index = __instance.m_currentRotationIndex;
                }

                if (structureData.m_visuals[index].m_visual is null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
