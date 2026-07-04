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


#region BANHEIRO

        private static Transform beeEffectTransform;

        [HarmonyPatch(typeof(Chair), "Interact")]
        public static class SitEffect_Chair_Interact_Patch
        {
            private static bool Prefix(Chair __instance, Humanoid human, bool hold, bool alt, ref bool __runOriginal)
            {
                if (__instance == null || human == null || !human.IsPlayer() || __instance.gameObject.name != "Cagador") return true;

                __runOriginal = false;

                // Debug.LogWarning(" __instance.m_attachPoint.name >> " + __instance.m_attachPoint.name); // this name must be use at Player.AttachStop()

                Player player = human as Player;
                if (hold || !__instance.InUseDistance(player) || Time.time - Chair.m_lastSitTime < 2f)
                {
                    return false;
                }

                Player closestPlayer = Player.GetClosestPlayer(__instance.m_attachPoint.position, 0.1f);
                if (closestPlayer != null && closestPlayer != Player.m_localPlayer)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_blocked");
                    return false;
                }
                if ((bool)player && !player.IsEncumbered())
                {
                    player.AttachStart(__instance.m_attachPoint, null, hideWeapons: false, isBed: false, __instance.m_inShip, __instance.m_attachAnimation, __instance.m_detachOffset);
                    Chair.m_lastSitTime = Time.time;

                    StartBeeEffect(__instance.transform);
                    DropItemOnGround(__instance.transform.position, "Resin");
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "AttachStop")]
        public static class SitEffect_Player_AttachStop_Patch
        {
            private static void Prefix(Player __instance)
            {
                if (__instance == null || __instance.m_attachPoint == null) return;

                if (__instance.m_attachPoint.name == "pontodeataque") //<< find the attach point name
                {
                    CoroutineHelper.Instance.StartCoroutine(StopBeeEffectWithDelay(10f));
                    // Debug.LogWarning(" Player.AttachStop() working properly"); //<< add this log se we know the StopBeeEffect() is the problem.
                }
            }
        }

        private static void StartBeeEffect(Transform chairTransform)
        {
            if (beeEffectTransform == null)
            {
                beeEffectTransform = chairTransform.Find("MH_BeeEffect");
            }

            if (beeEffectTransform != null)
            {
                beeEffectTransform.gameObject.SetActive(true);
            }
            else
            {
                // Debug.LogWarning("MH_BeeEffect não encontrado no prefab A1_A_Outhouse!");
            }
        }

        private static IEnumerator StopBeeEffectWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopBeeEffect();
        }

        private static void StopBeeEffect()
        {
            if (beeEffectTransform != null)
            {
                beeEffectTransform.gameObject.SetActive(false);
            }
            else
            {
                // Debug.LogWarning("MH_BeeEffect não encontrado no prefab A1_A_Outhouse!"); //<< i do not know what was said here but its someting
            }
        }

        private static float heightOffsets = 0.2f;
        private static void DropItemOnGround(Vector3 position, string itemName)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
            if (itemPrefab != null)
            {
                Vector3 dropPosition = position + Vector3.up * heightOffsets; // <<< Ajustar posição para garantir que o item não caia através do chão
                GameObject itemDrop = GameObject.Instantiate(itemPrefab, dropPosition, Quaternion.identity);
                itemDrop.GetComponent<ItemDrop>().OnPlayerDrop();
            }
            else
            {
                // Debug.LogWarning($"Item {itemName} não encontrado!");
            }
        }

        #endregion