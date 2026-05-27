
using System;
using Utils;

[RequireComponent(typeof(PartyFieldController))]
public class PartyManager : Singleton<PartyManager>
{
    private PartyFieldController fieldController;
    
    [Header("Init Party")]
    [SerializeField] private CharacterDefinitionSO PlayerDefinition;

    [SerializeField] private List<CharacterRuntimeData> partyMembers = new();
    
    public List<CharacterRuntimeData> PartyMembers => partyMembers;
    
    /* ------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        InitParty();
        fieldController = GetComponent<PartyFieldController>();
    }

    private void Start()
    {
        ApplyInitialEquipment();
    }
    
    /* ------------------------------------------------------------------------- */

    private void InitParty()
    {
        if (partyMembers.Count == 0)
        {
            partyMembers.Add(new CharacterRuntimeData(PlayerDefinition));
        }
    }

    private void AddMember(CharacterDefinitionSO characterDefinition)
    {
        // 队伍人员唯一
        partyMembers.Add(new CharacterRuntimeData(characterDefinition));
        RefreshFieldFollowers();
    }

    private void RefreshFieldFollowers()
    {
        List<CharacterDefinitionSO> defs = new(partyMembers.Count);

        foreach (var member in partyMembers)
        {
            defs.Add(member.Definition);
        }
        fieldController.UpdateFollowers(defs);
        
    }

    public void RecruitMember(CharacterDefinitionSO characterDefinition)
    {
        AddMember(characterDefinition);
        GameModeManager.Instance.RequestChangeMode(GameMode.Explore);
    }
    
    public void ApplyInitialEquipment()
    {
        foreach (var member in partyMembers)
            member.ApplyInitialEquipment();
    }
}