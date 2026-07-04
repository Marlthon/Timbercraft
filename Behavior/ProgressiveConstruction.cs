using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Obfuscation(Exclude = true, ApplyToMembers = true)]
public class ProgressiveConstruction : MonoBehaviour
{
    [Header("Stages de Construção")]
    public GameObject[] m_stages;

    [Header("NPCs da Construção")]
    public GameObject[] m_npcs;

    [Header("Efeitos de Construção")]
    public GameObject m_sfx;
    public GameObject m_vfx;

    [Header("Efeitos dos NPCs")]
    public GameObject m_npcSpawnVfx;
    public GameObject m_npcDespawnVfx;

    private ZNetView m_nview;
    private bool m_initialized = false;
    private int m_initAttempts = 0;
    private int m_cachedTotalPieces = -1;
    private bool m_isNewConstruction = false;

    private const int MAX_INIT_ATTEMPTS = 10;

    private static readonly int s_currentPiece = "currentPiece".GetStableHashCode();

    private bool ProgressiveEnabled => ProgressiveConstructionConfig.Enabled;
    private float NpcArrivalDelay => ProgressiveConstructionConfig.NpcArrivalDelay;
    private float ConstructionStartDelay => ProgressiveConstructionConfig.ConstructionStartDelay;
    private float PieceInterval => ProgressiveConstructionConfig.PieceInterval;
    private string MessageArriving => ProgressiveConstructionConfig.MessageArriving;
    private string MessageStarted => ProgressiveConstructionConfig.MessageStarted;
    private string MessageCompleted => ProgressiveConstructionConfig.MessageCompleted;

    private void Start()
    {
        TryInit();
    }

    private void TryInit()
    {
        m_initAttempts++;
        if (m_initAttempts > MAX_INIT_ATTEMPTS)
            return;

        m_nview = GetComponent<ZNetView>();

        if (m_nview == null || !m_nview.IsValid() || m_nview.GetZDO() == null)
        {
            Invoke("TryInit", 1f);
            return;
        }

        if (m_stages == null || m_stages.Length == 0)
            return;

        if (m_initialized) return;
        m_initialized = true;

        // Desativa stages e filhos — só roda no objeto real, nunca no ghost
        // pois o ghost não tem ZNetView válido e não chega até aqui
        if (m_stages != null)
        {
            foreach (GameObject stage in m_stages)
            {
                if (stage == null) continue;
                for (int i = 0; i < stage.transform.childCount; i++)
                    stage.transform.GetChild(i).gameObject.SetActive(false);
                stage.SetActive(false);
            }
        }

        m_cachedTotalPieces = CalculateTotalPieces();

        // Progressive desativado — aparece tudo imediatamente
        if (!ProgressiveEnabled)
        {
            ApplyAllPiecesInstantly();
            return;
        }

        m_nview.Register<int>("RPC_SetPiece", RPC_SetPiece);

        if (m_nview.IsOwner())
        {
            if (m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime) == 0L)
            {
                m_isNewConstruction = true;
                m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
            }
        }

        int currentPiece = m_nview.GetZDO().GetInt(s_currentPiece);

        // Aplica estado atual sem efeitos (quem entra depois)
        ApplyPieces(currentPiece, playEffects: false);

        if (m_isNewConstruction && currentPiece == 0)
        {
            ShowMessage(MessageArriving);
            StartCoroutine(NewConstructionSequence());
        }
        else
        {
            if (currentPiece > 0 && currentPiece < m_cachedTotalPieces)
                SetNpcsActive(true, playEffects: false);

            InvokeRepeating("UpdateConstruction", 5f, PieceInterval);
        }
    }

    private IEnumerator NewConstructionSequence()
    {
        yield return new WaitForSeconds(NpcArrivalDelay);

        if (m_npcs != null && m_npcs.Length > 0)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < m_npcs.Length; i++)
                if (m_npcs[i] != null) indices.Add(i);

            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

            foreach (int idx in indices)
            {
                GameObject npc = m_npcs[idx];
                if (npc == null) continue;

                npc.SetActive(true);

                if (m_npcSpawnVfx != null)
                    UnityEngine.Object.Instantiate(m_npcSpawnVfx, npc.transform.position, npc.transform.rotation);

                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(ConstructionStartDelay);

        ShowMessage(MessageStarted);
        InvokeRepeating("UpdateConstruction", 0f, PieceInterval);
    }

    private void ApplyAllPiecesInstantly()
    {
        foreach (GameObject stage in m_stages)
        {
            if (stage == null) continue;
            stage.SetActive(true);
            for (int i = 0; i < stage.transform.childCount; i++)
                stage.transform.GetChild(i).gameObject.SetActive(true);
        }

        if (m_npcs != null)
            foreach (GameObject npc in m_npcs)
                if (npc != null) npc.SetActive(false);
    }

    private void UpdateConstruction()
    {
        if (!m_nview.IsOwner()) return;

        if (m_nview.GetZDO().GetLong(ZDOVars.s_spawnTime) == 0L)
        {
            m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
            m_nview.GetZDO().Set(s_currentPiece, 0);
        }

        int savedPiece = m_nview.GetZDO().GetInt(s_currentPiece);

        if (savedPiece >= m_cachedTotalPieces)
        {
            CancelInvoke("UpdateConstruction");
            return;
        }

        int nextPiece = savedPiece + 1;

        m_nview.GetZDO().Set(s_currentPiece, nextPiece);
        m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPiece", nextPiece);

        if (nextPiece >= m_cachedTotalPieces)
        {
            ShowMessage(MessageCompleted);
            SetNpcsActive(false, playEffects: true);
            CancelInvoke("UpdateConstruction");
        }
    }

    private void RPC_SetPiece(long sender, int pieceIndex)
    {
        ApplyPieces(pieceIndex, playEffects: true);
    }

    private void ApplyPieces(int upToPiece, bool playEffects)
    {
        int globalIndex = 0;

        for (int s = 0; s < m_stages.Length; s++)
        {
            GameObject stageObj = m_stages[s];
            if (stageObj == null) continue;

            int childCount = stageObj.transform.childCount;
            bool stageStarted = globalIndex < upToPiece;

            if (stageStarted && !stageObj.activeSelf)
                stageObj.SetActive(true);

            for (int i = 0; i < childCount; i++)
            {
                GameObject piece = stageObj.transform.GetChild(i).gameObject;
                bool shouldBeActive = globalIndex < upToPiece;
                bool wasActive = piece.activeSelf;

                piece.SetActive(shouldBeActive);

                if (playEffects && shouldBeActive && !wasActive)
                    PlayEffects(piece.transform.position);

                globalIndex++;
            }
        }
    }

    private void SetNpcsActive(bool active, bool playEffects)
    {
        if (m_npcs == null) return;

        foreach (GameObject npc in m_npcs)
        {
            if (npc == null) continue;

            bool wasActive = npc.activeSelf;
            npc.SetActive(active);

            if (playEffects)
            {
                if (active && !wasActive && m_npcSpawnVfx != null)
                    UnityEngine.Object.Instantiate(m_npcSpawnVfx, npc.transform.position, npc.transform.rotation);

                if (!active && wasActive && m_npcDespawnVfx != null)
                    UnityEngine.Object.Instantiate(m_npcDespawnVfx, npc.transform.position, npc.transform.rotation);
            }
        }
    }

    private void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (Player.m_localPlayer != null)
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, message);
    }

    private int CalculateTotalPieces()
    {
        int total = 0;
        foreach (GameObject stage in m_stages)
        {
            if (stage != null)
                total += stage.transform.childCount;
        }
        return total;
    }

    private void PlayEffects(Vector3 position)
    {
        if (m_sfx != null)
            UnityEngine.Object.Instantiate(m_sfx, position, Quaternion.identity);

        if (m_vfx != null)
            UnityEngine.Object.Instantiate(m_vfx, position, Quaternion.identity);
    }
}

// Classe estática ponte — valores padrão usados se o mod não estiver carregado
// Os valores reais são definidos pelo Patch_ProgressiveConstruction.cs no mod
[Obfuscation(Exclude = true, ApplyToMembers = true)]
public static class ProgressiveConstructionConfig
{
    public static bool Enabled = true;
    public static float NpcArrivalDelay = 8f;
    public static float ConstructionStartDelay = 3f;
    public static float PieceInterval = 2f;
    public static string MessageArriving = "The workers are on their way...";
    public static string MessageStarted = "Construction has begun!";
    public static string MessageCompleted = "Construction complete!";
}
