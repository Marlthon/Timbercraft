using UnityEngine;

public class ConstructionNPC : MonoBehaviour
{
    [Header("Equipamentos")]
    public GameObject m_chestItem;
    public GameObject m_legItem;
    public GameObject m_helmetItem;
    public GameObject m_rightHandItem;
    public GameObject m_leftHandItem;
    public GameObject m_shoulderItem;
    public GameObject m_hairItem;
    public GameObject m_beardItem;

    private VisEquipment m_visEquipment;

    private void Awake()
    {
        RemoveIfExists<Player>();
        RemoveIfExists<PlayerController>();
        RemoveIfExists<FootStep>();
        RemoveIfExists<BaseAI>();
        RemoveIfExists<MonsterAI>();
        RemoveIfExists<AnimalAI>();
        RemoveIfExists<CharacterDrop>();
        RemoveIfExists<Talker>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
            col.enabled = false;

        m_visEquipment = GetComponent<VisEquipment>();
    }

    private void Start()
    {
        if (m_visEquipment == null) return;

        if (m_chestItem != null)
            m_visEquipment.SetChestItem(m_chestItem.name);

        if (m_legItem != null)
            m_visEquipment.SetLegItem(m_legItem.name);

        if (m_helmetItem != null)
            m_visEquipment.SetHelmetItem(m_helmetItem.name);

        if (m_rightHandItem != null)
            m_visEquipment.SetRightItem(m_rightHandItem.name);

        if (m_leftHandItem != null)
            m_visEquipment.SetLeftItem(m_leftHandItem.name, 0);

        if (m_shoulderItem != null)
            m_visEquipment.SetShoulderItem(m_shoulderItem.name, 0);

        if (m_hairItem != null)
            m_visEquipment.SetHairItem(m_hairItem.name);

        if (m_beardItem != null)
            m_visEquipment.SetBeardItem(m_beardItem.name);
    }

    private void RemoveIfExists<T>() where T : UnityEngine.Component
    {
        T component = GetComponent<T>();
        if (component != null)
            Destroy(component);
    }
}