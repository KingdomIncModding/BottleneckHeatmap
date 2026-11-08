using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    public static class BottleneckStateEvaluator
    {
        static readonly FieldInfo OutputFullField = AccessTools.Field(typeof(WorldFlowElement), "_outputFull");

        public static BottleneckState Evaluate(WorldFlowElement world)
        {
            if (!world || !world.gameObject.activeInHierarchy)
                return BottleneckState.Idle;

            var element = world.Element;
            if (element == null)
                return BottleneckState.Idle;

            // 0. Storage Containers: Stockpiles and Armories (including Hoards, Treasuries, Pantries)
            if (element is Stockpile stockpile)
            {
                if (stockpile.Contents != null && stockpile.Contents.Count >= stockpile.Capacity)
                    return BottleneckState.FullStorage;
            }
            if (element is Armory armory)
            {
                if (armory.ArmoryType != null && GamePlayer.Current != null &&
                    !GamePlayer.Current.Armory.CanAddTo(armory.Channel, armory.ArmoryType, armory.MaxItems))
                    return BottleneckState.FullStorage;
            }

            // Shop congestion: when gold cap is reached, shop is clogged
            if (element is Shop shop)
            {
                if (shop.GoldCap > 0 && shop.CollectedGold >= shop.GoldCap)
                    return BottleneckState.Clogged;
            }

            // 1. Output congestion check (highest priority for production: if blocked, it's a bottleneck!)
            if (OutputFullField?.GetValue(world) is TMP_Text outputText && outputText && outputText.gameObject.activeSelf)
                return BottleneckState.Clogged;

            if (element.Progress >= 1f && element.Outputs != null && element.Outputs.Count > 0)
            {
                bool anyCanBuffer = false;
                for (int i = 0; i < element.Outputs.Count; i++)
                {
                    if (element.Outputs[i].CanBuffer)
                    {
                        anyCanBuffer = true;
                        break;
                    }
                }
                if (!anyCanBuffer)
                    return BottleneckState.Clogged;
            }

            // 2. Recipe Elements (Smelters, Refiners, Crafters, Kitchens, Disenchanters, etc.)
            if (element is RecipeElement recipeEl)
            {
                if (recipeEl.Recipe == null)
                    return BottleneckState.Idle;

                if (recipeEl.Crafting || recipeEl.Input != null)
                    return BottleneckState.Running;

                // Check reagents
                if (recipeEl.Recipe.Reagents != null && recipeEl.Recipe.Reagents.Length > 0)
                {
                    for (int i = 0; i < recipeEl.Recipe.Reagents.Length; i++)
                    {
                        var reagent = recipeEl.Recipe.Reagents[i];
                        if (i >= recipeEl.Inputs.Count || recipeEl.Inputs[i].BufferedItems.Count < reagent.Count)
                            return BottleneckState.Starved;
                    }
                }

                // Check funds
                var cost = recipeEl.GetCraftingCost();
                if (cost != null && GamePlayer.Current != null && !GamePlayer.Current.CanAfford(cost))
                    return BottleneckState.Starved;

                if (element.Progress > 0f)
                    return BottleneckState.Running;

                return BottleneckState.Starved;
            }

            // 3. Resource Elements (Mines, Farms, Foresters, Wells, etc.)
            if (element is ResourceElement resourceEl)
            {
                if (resourceEl.Resource == null || resourceEl.QueuedCount <= 0)
                    return BottleneckState.Idle;

                if (GamePlayer.Current != null && GamePlayer.Current.GetResourceQuotaMultiplier(resourceEl.Resource) <= 0.001f)
                    return BottleneckState.Starved;

                return BottleneckState.Running;
            }

            // 4. Fallback for other FlowElements (Collectors, Stockpiles, etc.)
            if (element.Progress > 0f)
                return BottleneckState.Running;

            if (element.Inputs != null && element.Inputs.Count > 0)
            {
                bool allEmpty = true;
                for (int i = 0; i < element.Inputs.Count; i++)
                {
                    if (element.Inputs[i].BufferedItems.Count > 0)
                    {
                        allEmpty = false;
                        break;
                    }
                }
                if (allEmpty)
                    return BottleneckState.Starved;
            }

            return BottleneckState.Idle;
        }

        public static BottleneckState Evaluate(WorldFlowPipe worldPipe, float lastTransferTime, bool wasRejected)
        {
            if (!worldPipe || !worldPipe.gameObject.activeInHierarchy)
                return BottleneckState.Idle;

            var pipe = worldPipe.Pipe;
            if (pipe == null)
                return BottleneckState.Idle;

            // 1. Blocked / Rejected checks
            if (wasRejected || pipe.ItemRejected)
                return BottleneckState.Clogged;

            var output = pipe.ConnectedOutput;
            var input = pipe.ConnectedInput;

            if (output == null || input == null)
                return BottleneckState.Idle;

            var item = output.AvailableItem;
            if (item != null)
            {
                if (!input.CanAcceptItem(item) || !input.CanBuffer)
                    return BottleneckState.Clogged;
            }

            // 2. Active flow
            if (Time.time - lastTransferTime <= 1.5f)
                return BottleneckState.Running;

            // 3. Starved / Empty
            if (output.BufferedItems == null || output.BufferedItems.Count == 0)
                return BottleneckState.Starved;

            return BottleneckState.Running;
        }
    }
}

