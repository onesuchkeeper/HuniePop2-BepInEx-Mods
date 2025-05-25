using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod.Utility;

public static class StyleUnlockUtility
{
    private static readonly FieldInfo _notificationSequence = AccessTools.Field(typeof(NotificationBoxBehavior), "_notificationSequence");
    private readonly static float _styleUnlockDuration = 5f;
    private readonly static Dictionary<UiDoll, Queue<(string message, bool silent)>> _queues
        = new Dictionary<UiDoll, Queue<(string message, bool silent)>>();

    private static IEnumerable<int> GetMissingInSequence(IEnumerable<int> values, int minInclusive, int maxExclusive)
    {
        foreach (var value in values.OrderBy(x => x))
        {
            while (minInclusive < value)
            {
                yield return minInclusive++;
            }

            minInclusive++;
        }

        while (minInclusive < maxExclusive)
        {
            yield return minInclusive++;
        }
    }

    public static bool UnlockRandomStyle(PlayerFileGirl playerFileGirl, bool silent)
    {
        var def = playerFileGirl.girlDefinition;

        var lockedOutfitIndexes = GetMissingInSequence(playerFileGirl.unlockedOutfits, 0, def.outfits.Count).ToArray();

        var outfitIndex = lockedOutfitIndexes.Length == 0
            ? -1
            : lockedOutfitIndexes[UnityEngine.Random.Range(0, lockedOutfitIndexes.Length - 1)];

        if (outfitIndex == -1)
        {
            var lockedHairstyleIndexes = GetMissingInSequence(playerFileGirl.unlockedHairstyles, 0, def.hairstyles.Count).ToArray();

            var hairstyleIndex = lockedHairstyleIndexes.Length == 0
                ? -1
                : lockedHairstyleIndexes[UnityEngine.Random.Range(0, lockedHairstyleIndexes.Length - 1)];

            return UnlockStyle(playerFileGirl, hairstyleIndex, outfitIndex, silent);
        }

        return UnlockStyle(playerFileGirl, def.outfits[outfitIndex].pairHairstyleIndex, outfitIndex, silent);
    }

    public static bool UnlockCurrentStyle(PlayerFileGirl playerFileGirl, bool silent)
    {
        if (playerFileGirl == null || playerFileGirl.girlDefinition == null)
        {
            return false;
        }

        var doll = Game.Session.gameCanvas.GetDoll(playerFileGirl.girlDefinition);
        if (doll == null)
        {
            return false;
        }

        return UnlockStyle(playerFileGirl, doll.currentHairstyleIndex, doll.currentOutfitIndex, silent);
    }

    public static bool HandleStyleUnlocks(PlayerFileGirl playerFileGirl, RelativeId hairId, RelativeId outfitId, bool silent)
    {
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, playerFileGirl.girlDefinition.id);
        var girlExpansion = playerFileGirl.girlDefinition.Expansion();

        return UnlockStyle(playerFileGirl,
            girlExpansion.HairstyleIdToIndex[hairId],
            girlExpansion.OutfitIdToIndex[outfitId],
            silent);
    }

    public static bool UnlockStyle(PlayerFileGirl playerFileGirl, int hairIndex, int outfitIndex, bool silent)
    {
        if (!Game.Persistence.playerFile.girls.Contains(playerFileGirl)
            || (playerFileGirl?.stylesOnDates ?? true))
        {
            return false;
        }

        var unlocked = false;

        string message = null;
        if (hairIndex == outfitIndex)
        {
            if (outfitIndex != -1)
            {
                //both are the same and non-default
                message = $"{playerFileGirl.girlDefinition.outfits[outfitIndex].outfitName} outfit and hairstyle unlocked!";
                unlocked |= playerFileGirl.UnlockHairstyle(hairIndex);
                unlocked |= playerFileGirl.UnlockOutfit(outfitIndex);
            }
            else
            {
                //both are default, nothing to unlock
                return false;
            }
        }
        else
        {
            //one or both are unlocked, they aren't the same
            if (hairIndex == -1)
            {
                message = string.Empty;
            }
            else
            {
                message = $"\"{playerFileGirl.girlDefinition.hairstyles[hairIndex].hairstyleName}\" hairstyle";
                unlocked |= playerFileGirl.UnlockHairstyle(hairIndex);
            }

            if (outfitIndex != -1)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    message += " and ";
                }

                message += $"\"{playerFileGirl.girlDefinition.outfits[outfitIndex].outfitName}\" outfit";
                unlocked |= playerFileGirl.UnlockOutfit(outfitIndex);
            }

            message += " unlocked!";
        }

        if (!unlocked)
        {
            return false;
        }

        var doll = Game.Session.gameCanvas.GetDoll(playerFileGirl?.girlDefinition);
        if (doll == null)
        {
            return true;
        }

        if (!_queues.TryGetValue(doll, out var queue))
        {
            queue = new Queue<(string message, bool silent)>();
            _queues[doll] = queue;
            queue.Enqueue((message, silent));
            HandleQueue(doll);
            return true;
        }

        queue.Enqueue((message, silent));

        return true;
    }

    private static void HandleQueue(UiDoll doll)
    {
        var queue = _queues[doll];
        var entry = queue.Peek();

        doll.notificationBox.Show(entry.message, _styleUnlockDuration, entry.silent);
        var notificationSequence = _notificationSequence.GetValue<Sequence>(doll.notificationBox);

        notificationSequence.onComplete += () =>
        {
            queue.Dequeue();

            if (queue.Count == 0)
            {
                _queues.Remove(doll);
            }
            else
            {
                ModInterface.Log.LogInfo("Next item in queue");
                HandleQueue(doll);
            }
        };
    }
}
