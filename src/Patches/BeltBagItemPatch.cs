using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scopophobia.Patches
{
    [HarmonyPatch(typeof(BeltBagItem))]
    internal class BeltBagItemPatch
    {
        [HarmonyPatch(typeof(BeltBagItem), "PutObjectInBagLocalClient")]
        [HarmonyPrefix]
        private static void Prefix_PutObjectInBagLocalClient(BeltBagItem __instance, GrabbableObject gObject)
        {
            if (gObject != null)
            {
                ShyGuyPaintingProp painting = gObject.GetComponent<ShyGuyPaintingProp>();
                if (painting != null && !painting.hasSpawnedFromBeltBag)
                {
                    PlayerControllerB player = __instance.playerHeldBy;
                    ScopophobiaPlugin.Instance.LogInfoExtended("[BeltBagPatch] Trigger painting on pickup: " +painting.name);
                    painting.TriggerFromBeltBag(player);
                }
            }
        }
    }
}
