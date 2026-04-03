using Data;
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using System.Linq;

public class SandboxModePatcher
    {
        private List<string> uncraftableRecipes = new List<string>();
        private HarmonySupport m_harmonySupport;

        public void OnEnable(Dictionary<string, object> dependencies)
        {
            if (!dependencies.TryGetValue("harmony_support", out var harmonySupportObj))
            {
                throw new Exception("harmony_support not found");
            }

            m_harmonySupport = (HarmonySupport)harmonySupportObj;
            m_harmonySupport.PatchAll("SandboxMode", typeof(SandboxMode).Assembly);
            foreach (RecipeData recipe in DatabaseManager.I.m_recipeDatabase.recipes)
            {  
                if (recipe.m_notCraftable)
                {
                    recipe.m_notCraftable = false;                    
                    uncraftableRecipes.Add(recipe.name);
                    Debug.Log($"set craftable {recipe.name}");
                }
                
            }
            Debug.Log($"onenable called, {uncraftableRecipes.Count} recipes were set");
    }

        public void OnDisable()
        {
            Debug.Log($"ondisable called, {uncraftableRecipes.Count} recipes to unset");
            foreach (RecipeData recipe in DatabaseManager.I.m_recipeDatabase.recipes)
            {
                
                if (uncraftableRecipes.Contains(recipe.name))
                {
                    recipe.m_notCraftable = true;
                    Debug.Log($"set uncraftable {recipe.name}");
                }

            }
            uncraftableRecipes.Clear();
            m_harmonySupport.UnpatchAll("SandboxMode");
            
        }

    }

    [HarmonyPatch]
    public static class SandboxMode
    {
        [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.SkipTutorial))]
        [HarmonyPostfix]
        public static void UnlockResearchAndSetCraftable()
        {
            foreach (ResearchData research in DatabaseManager.I.m_researchDatabase.researches)
            {
                ResearchBanner.m_showBanner = false;
                {
                    ResearchManager.I.ConfirmCompleteResearch(research, true);
                }
                ResearchBanner.m_showBanner = true;
            }
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
            Player.I.PlayerInventory.AddEntityToFirstAvailable(entity, count, false, false);
            return false;
        }
}