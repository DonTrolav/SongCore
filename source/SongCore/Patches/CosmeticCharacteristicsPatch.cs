using System.Collections.Generic;
using System.IO;
using System.Linq;
using BGLib.Polyglot;
using HMUI;
using SiraUtil.Affinity;
using SongCore.Utilities;
using UnityEngine;

namespace SongCore.Patches
{
    internal class CosmeticCharacteristicsPatch : IAffinity
    {
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly CustomLevelLoader _customLevelLoader;

        private CosmeticCharacteristicsPatch(StandardLevelDetailViewController standardLevelDetailViewController, CustomLevelLoader customLevelLoader)
        {
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _customLevelLoader = customLevelLoader;
        }

        [AffinityPatch(typeof(BeatmapCharacteristicSegmentedControlController), nameof(BeatmapCharacteristicSegmentedControlController.SetData))]
        private void SetCosmeticCharacteristic(BeatmapCharacteristicSegmentedControlController __instance, BeatmapCharacteristicSO selectedBeatmapCharacteristic)
        {
            if (!Plugin.Configuration.DisplayCustomCharacteristics)
            {
                return;
            }

            var beatmapLevel = _standardLevelDetailViewController._beatmapLevel;
            if (beatmapLevel.hasPrecalculatedData)
            {
                return;
            }

            var extraSongData = Collections.RetrieveExtraSongData(Hashing.GetCustomLevelHash(beatmapLevel));
            if (extraSongData?._characteristicDetails == null || extraSongData._characteristicDetails.Length == 0)
            {
                return;
            }

            var segmentedControl = __instance._segmentedControl;
            var dataItems = segmentedControl._dataItems;
            var newDataItems = new List<IconSegmentedControl.DataItem>();
            var i = 0;
            var cellIndex = 0;
            foreach (var dataItem in dataItems)
            {
                var beatmapCharacteristic = __instance._currentlyAvailableBeatmapCharacteristics[i];
                var serializedName = beatmapCharacteristic.serializedName;
                var characteristicDetails = extraSongData._characteristicDetails.FirstOrDefault(c => c._beatmapCharacteristicName == serializedName);

                if (characteristicDetails != null)
                {
                    Sprite? icon = null;

                    if (characteristicDetails._characteristicIconFilePath != null)
                    {
                        icon = Utils.LoadSpriteFromFile(Path.Combine(_customLevelLoader._loadedBeatmapSaveData[beatmapLevel.levelID].customLevelFolderInfo.folderPath, characteristicDetails._characteristicIconFilePath));
                    }

                    if (icon == null)
                    {
                        icon = beatmapCharacteristic.icon;
                    }

                    var label = characteristicDetails._characteristicLabel ?? Localization.Get(beatmapCharacteristic.descriptionLocalizationKey);
                    newDataItems.Add(new IconSegmentedControl.DataItem(icon, label));
                }
                else
                {
                    newDataItems.Add(dataItem);
                }

                if (beatmapCharacteristic == selectedBeatmapCharacteristic)
                {
                    cellIndex = i;
                }

                i++;
            }

            segmentedControl.SetData(newDataItems.ToArray());
            segmentedControl.SelectCellWithNumber(cellIndex);
        }
    }
}