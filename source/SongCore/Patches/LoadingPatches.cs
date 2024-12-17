using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace SongCore.Patches
{
    [HarmonyPatch(typeof(BeatmapLevelsModel), nameof(BeatmapLevelsModel.ReloadCustomLevelPackCollectionAsync))]
    internal static class StopVanillaLoadingPatch
    {
        private static bool Prefix() => false;
    }

    [HarmonyPatch(typeof(LevelFilteringNavigationController), nameof(LevelFilteringNavigationController.UpdateCustomSongs))]
    internal static class StopVanillaLoadingPatch2
    {
        private static bool Prefix(LevelFilteringNavigationController __instance)
        {
            if (Loader.CustomLevelsRepository == null)
            {
                return false;
            }

            __instance._customLevelPacks = Loader.CustomLevelsRepository.beatmapLevelPacks;
            IEnumerable<BeatmapLevelPack>? packs = null;
            if (__instance._ostBeatmapLevelPacks != null)
            {
                packs = __instance._ostBeatmapLevelPacks;
            }

            if (__instance._musicPacksBeatmapLevelPacks != null)
            {
                packs = packs == null ? __instance._musicPacksBeatmapLevelPacks : packs.Concat(__instance._musicPacksBeatmapLevelPacks);
            }

            if (__instance._customLevelPacks != null)
            {
                packs = packs == null ? __instance._customLevelPacks : packs.Concat(__instance._customLevelPacks);
            }

            __instance._allBeatmapLevelPacks = packs.ToArray();
            __instance._levelSearchViewController.Setup(__instance._allBeatmapLevelPacks);
            __instance.UpdateSecondChildControllerContent(__instance._selectLevelCategoryViewController.selectedLevelCategory);

            return false;
        }
    }
}
