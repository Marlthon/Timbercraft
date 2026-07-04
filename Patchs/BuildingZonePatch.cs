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

        // Cache do material ghost e dos materiais originais
        private static Material _ghostMaterial = null;
        private static readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
        private static GameObject _lastGhost = null;

        public static void Init(Harmony harmony)
        {
            if (_initialized) return;
            _initialized = true;
            harmony.PatchAll(typeof(BuildingZonePatch));
        }

        // ─────────────────────────────────────────────────────────────
        // Carrega o material ghost do asset bundle
        // ─────────────────────────────────────────────────────────────
        private static Material GetGhostMaterial()
        {
            if (_ghostMaterial != null) return _ghostMaterial;

            GameObject prefab = ZNetScene.instance?.GetPrefab("timberghost");
            if (prefab == null)
            {
                // Busca direto no asset bundle via PrefabManager
                var mat = ItemManager.PrefabManager.RegisterPrefab("mar_timbercraft", "timberghost");
                if (mat != null)
                {
                    Renderer r = mat.GetComponent<Renderer>();
                    if (r != null) _ghostMaterial = r.sharedMaterial;
                }
            }

            return _ghostMaterial;
        }

        // ─────────────────────────────────────────────────────────────
        // PATCH: UpdatePlacementGhost
        // Aplica timberghost.mat em todos os renderers do ghost
        // quando a peça tem ProgressiveConstruction
        // ─────────────────────────────────────────────────────────────
        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void Player_UpdatePlacementGhost_Postfix(Player __instance)
        {
            if (__instance.m_placementGhost == null)
            {
                // Ghost foi removido — restaura materiais e limpa cache
                if (_lastGhost != null)
                {
                    RestoreOriginalMaterials();
                    _lastGhost = null;
                }
                return;
            }

            // Ghost mudou — limpa cache anterior
            if (_lastGhost != __instance.m_placementGhost)
            {
                RestoreOriginalMaterials();
                _lastGhost = __instance.m_placementGhost;

                // Só aplica o ghost material se for uma ProgressiveConstruction
                ProgressiveConstruction pc = __instance.m_placementGhost.GetComponent<ProgressiveConstruction>();
                if (pc != null)
                {
                    ApplyGhostMaterial(__instance.m_placementGhost);
                }
            }

            // Lógica original da BuildingZone
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

        private static void ApplyGhostMaterial(GameObject ghost)
        {
            Material ghostMat = _ghostMaterial;
            if (ghostMat == null)
            {
                // Tenta buscar pelo nome no asset bundle
                var assetBundle = AssetBundle.GetAllLoadedAssetBundles();
                foreach (var bundle in assetBundle)
                {
                    ghostMat = bundle.LoadAsset<Material>("timberghost");
                    if (ghostMat != null)
                    {
                        _ghostMaterial = ghostMat;
                        break;
                    }
                }
            }

            if (ghostMat == null) return;

            _originalMaterials.Clear();

            foreach (Renderer renderer in ghost.GetComponentsInChildren<Renderer>(true))
            {
                // Salva materiais originais
                _originalMaterials[renderer] = renderer.sharedMaterials;

                // Aplica ghost material em todos os slots
                Material[] ghostMaterials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < ghostMaterials.Length; i++)
                    ghostMaterials[i] = ghostMat;

                renderer.sharedMaterials = ghostMaterials;
            }
        }

        private static void RestoreOriginalMaterials()
        {
            foreach (var kvp in _originalMaterials)
            {
                if (kvp.Key != null)
                    kvp.Key.sharedMaterials = kvp.Value;
            }
            _originalMaterials.Clear();
        }

        // ─────────────────────────────────────────────────────────────
        // WearNTear patches
        // ─────────────────────────────────────────────────────────────
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
    }
}
