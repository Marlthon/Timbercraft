using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Timbercraft
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class BuildingZonePatch
    {
        private static bool _initialized = false;

        private static readonly HashSet<int> _supportedInstances = new HashSet<int>();

        public static void Init(Harmony harmony)
        {
            if (_initialized) return;
            _initialized = true;
            harmony.PatchAll(typeof(BuildingZonePatch));
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.UpdateSupport))]
        [HarmonyPrefix]
        public static bool WearNTear_UpdateSupport_Prefix(WearNTear __instance)
        {
            if (BuildingZone.AllZones.Count == 0)
                return true;

            int id = __instance.GetInstanceID();

            if (_supportedInstances.Contains(id))
            {
                __instance.m_supports = true;
                return false;
            }

            Vector3 position = __instance.transform.position;
            foreach (Collider zone in BuildingZone.AllZones)
            {
                if (zone != null && zone.bounds.Contains(position))
                {
                    __instance.m_supports = true;
                    _supportedInstances.Add(id);
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.OnDestroy))]
        [HarmonyPrefix]
        public static void WearNTear_OnDestroy_Prefix(WearNTear __instance)
        {
            _supportedInstances.Remove(__instance.GetInstanceID());
        }

        [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
        [HarmonyPostfix]
        public static void Game_SpawnPlayer_Postfix()
        {
            _supportedInstances.Clear();
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Shutdown))]
        [HarmonyPostfix]
        public static void ZNetScene_Shutdown_Postfix()
        {
            _supportedInstances.Clear();
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void Player_UpdatePlacementGhost_Postfix(Player __instance)
        {
            if (__instance.m_placementGhost == null) return;
            if (__instance.m_placementStatus == Player.PlacementStatus.Valid) return;
            if (BuildingZone.AllZones.Count == 0) return;

            Vector3 ghostPosition = __instance.m_placementGhost.transform.position;

            foreach (Collider zone in BuildingZone.AllZones)
            {
                if (zone != null && zone.bounds.Contains(ghostPosition))
                {
                    __instance.m_placementStatus = Player.PlacementStatus.Valid;
                    __instance.SetPlacementGhostValid(true);
                    return;
                }
            }
        }
    }
}