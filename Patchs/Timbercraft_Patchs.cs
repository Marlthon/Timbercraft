using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Timbercraft
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]

    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("CoroutineHelper");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    GameObject.DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public static class Timbercraft_Patchs
    {

        private static Transform fliesTransform;
        private static Transform poopTransform;
        private static Transform fliesSfxTransform;
        private static Transform poopSfxTransform;
        private static Transform descargaTransform;
        private static bool _isDirty = false;
        private static Coroutine _activateCoroutine;

        private static void FindEffects(Transform outhouse)
        {
            if (fliesTransform != null) return;
            fliesTransform = outhouse.Find("Flies");
            poopTransform = outhouse.Find("Poop");
            fliesSfxTransform = outhouse.Find("FliesSfx");
            poopSfxTransform = outhouse.Find("PoopSfx");
            descargaTransform = outhouse.Find("Descarga");
        }

        private static void PlayAudio(Transform t)
        {
            if (t == null) return;
            t.gameObject.SetActive(true);
            AudioSource audio = t.GetComponent<AudioSource>();
            if (audio != null) { audio.Stop(); audio.Play(); }
        }

        private static IEnumerator ActivateSequence()
        {
            yield return new WaitForSeconds(1f);
            PlayAudio(poopSfxTransform);

            yield return new WaitForSeconds(3f); // 1+3 = 4s total
            if (fliesTransform != null) fliesTransform.gameObject.SetActive(true);
            if (poopTransform != null) poopTransform.gameObject.SetActive(true);
            PlayAudio(fliesSfxTransform);

            _isDirty = true;
            _activateCoroutine = null;
        }

        private static void CleanEffects()
        {
            if (_activateCoroutine != null)
            {
                CoroutineHelper.Instance.StopCoroutine(_activateCoroutine);
                _activateCoroutine = null;
            }

            if (fliesTransform != null) fliesTransform.gameObject.SetActive(false);
            if (poopTransform != null) poopTransform.gameObject.SetActive(false);
            if (fliesSfxTransform != null) fliesSfxTransform.gameObject.SetActive(false);
            if (poopSfxTransform != null) poopSfxTransform.gameObject.SetActive(false);

            PlayAudio(descargaTransform);
            _isDirty = false;
        }

        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        [HarmonyPatch(typeof(Chair), "GetHoverText")]
        public static class SitEffect_Chair_GetHoverText_Patch
        {
            private static void Postfix(Chair __instance, ref string __result)
            {
                if (__instance.gameObject.name != "Privada")
                    return;

                if (_isDirty)
                    __result += "\n[<color=yellow><b>Shift + E</b></color>] Limpar";
            }
        }

        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        [HarmonyPatch(typeof(Chair), "Interact")]
        public static class SitEffect_Chair_Interact_Patch
        {
            private static bool Prefix(Chair __instance, Humanoid human, bool hold, bool alt, ref bool __runOriginal)
            {
                if (__instance == null || human == null || !human.IsPlayer() || __instance.gameObject.name != "Privada")
                    return true;

                __runOriginal = false;

                Player player = human as Player;
                if (hold || !__instance.InUseDistance(player))
                    return false;

                FindEffects(__instance.transform.parent);

                if (alt)
                {
                    if (_isDirty) CleanEffects();
                    return false;
                }

                if (_isDirty)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "The toilet is dirty, clean it before using.");
                    return false;
                }

                if (Time.time - Chair.m_lastSitTime < 1f)
                    return false;

                Player closestPlayer = Player.GetClosestPlayer(__instance.m_attachPoint.position, 0.1f);
                if (closestPlayer != null && closestPlayer != Player.m_localPlayer)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_blocked");
                    return false;
                }

                if ((bool)player && !player.IsEncumbered())
                {
                    player.AttachStart(__instance.m_attachPoint, null, hideWeapons: false, isBed: false,
                        __instance.m_inShip, __instance.m_attachAnimation, __instance.m_detachOffset);

                    Chair.m_lastSitTime = Time.time;

                    _activateCoroutine = CoroutineHelper.Instance.StartCoroutine(ActivateSequence());
                }

                return false;
            }
        }

        [Obfuscation(Exclude = true, ApplyToMembers = true)]
        [HarmonyPatch(typeof(Player), "UpdatePlacement")]
        public static class Player_UpdatePlacement_Patch
        {
            static void Prefix(Player __instance, ref float ___m_maxPlaceDistance)
            {
                var plugin = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<TimbercraftPlugin>();
                ___m_maxPlaceDistance = plugin.maximumPlacementDistance.Value;
            }
        }
    }
}
