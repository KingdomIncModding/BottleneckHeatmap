using KingdomFoundation.Api;
using UnityEngine;
using System.Reflection;
using TMPro;

namespace KingdomBottleneckHeatmap
{
    public static class BottleneckStateEvaluator
    {
        public static BottleneckState Evaluate(Component world)
        {
            if (!world || !world.gameObject.activeInHierarchy)
                return BottleneckState.Idle;
            var element = FoundationApi.WorkspaceAccess.FromView(world);
            var production = element?.Production;
            if (production == null)
                return BottleneckState.Idle;
            if (production.IsStockpile && production.HasStorageContents && production.StoredItemCount >= production.StorageCapacity)
                return BottleneckState.FullStorage;
            if (production.IsArmory && production.HasArmoryType && FoundationApi.Kingdom != null && !production.CanStoreArmoryItem)
                return BottleneckState.FullStorage;
            if (production.IsShop && production.ShopGoldCap > 0 && production.CollectedGold >= production.ShopGoldCap)
                return BottleneckState.Clogged;
            if (FoundationApi.WorkspaceVisuals.IsOutputFullIndicatorVisible(world))
                return BottleneckState.Clogged;
            if (production.Progress >= 1f && production.OutputCount > 0)
            {
                bool canBuffer = false;
                for (int i = 0; i < production.OutputCount; i++)
                    if (production.OutputCanBuffer(i))
                    {
                        canBuffer = true;
                        break;
                    }

                if (!canBuffer)
                    return BottleneckState.Clogged;
            }

            if (production.IsRecipe)
            {
                if (!production.HasRecipe)
                    return BottleneckState.Idle;
                if (production.Crafting || production.HasCraftingInput)
                    return BottleneckState.Running;
                for (int i = 0; i < production.ReagentCount; i++)
                    if (i >= production.InputCount || production.BufferedInputCount(i) < production.ReagentAmount(i))
                        return BottleneckState.Starved;
                var cost = production.CraftingCost;
                if (cost != null && FoundationApi.Kingdom != null && !FoundationApi.Kingdom.CanAfford(cost))
                    return BottleneckState.Starved;
                return production.Progress > 0f ? BottleneckState.Running : BottleneckState.Starved;
            }

            if (production.IsResource)
            {
                if (!production.HasResource || production.QueuedResourceCount <= 0)
                    return BottleneckState.Idle;
                if (FoundationApi.Kingdom != null && production.ResourceQuotaMultiplier <= 0.001f)
                    return BottleneckState.Starved;
                return BottleneckState.Running;
            }

            if (production.Progress > 0f)
                return BottleneckState.Running;
            if (production.InputCount > 0)
            {
                bool empty = true;
                for (int i = 0; i < production.InputCount; i++)
                    if (production.BufferedInputCount(i) > 0)
                    {
                        empty = false;
                        break;
                    }

                if (empty)
                    return BottleneckState.Starved;
            }

            return BottleneckState.Idle;
        }

        public static BottleneckState Evaluate(Component worldPipe, float lastTransferTime, bool wasRejected)
        {
            if (!worldPipe || !worldPipe.gameObject.activeInHierarchy)
                return BottleneckState.Idle;
            var pipe = FoundationApi.WorkspaceVisuals.PipeFromView(worldPipe);
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
            if (output.BufferedItemCount == 0)
                return BottleneckState.Starved;
            return BottleneckState.Running;
        }
    }
}
