

using System;
using Framework.Event;
using Utils;

[RequireComponent(typeof(PartyFieldController))]
public class PartyManager : Singleton<PartyManager>,
    IEventReceiver<GameModeChangedEvent>
{
    private PartyFieldController fieldController;
    
    [Header("Init Party")]
    [SerializeField] private CharacterDefinitionSO PlayerDefinition;

    [SerializeField] private List<CharacterRuntimeData> partyMembers = new();
    
    public List<CharacterRuntimeData> PartyMembers => partyMembers;

    private bool fieldActorsHidden;
    
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

    private void OnEnable()
    {
        EventBus.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(this);
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
        FadeController.Instance.SetStyle(FadeStyle.PanelFade);
        FadeController.Instance.FadeOut(() =>
        {
            AddMember(characterDefinition);
            FadeController.Instance.FadeIn(() => GameModeManager.Instance.RequestChangeMode(GameMode.Explore));
        });
        
    }
    
    public void ApplyInitialEquipment()
    {
        foreach (var member in partyMembers)
            member.ApplyInitialEquipment();
    }

    #region 事件监听

    public void OnEvent(GameModeChangedEvent e)
    {
        if (e.newMode == GameMode.Battle)
        {
            if (fieldActorsHidden) return;
            
            fieldActorsHidden = true;
            fieldController.SetPlayerActive(false);
            fieldController.ClearFollower();
            return;
        }

        if (e.newMode == GameMode.Explore)
        {
            if (!fieldActorsHidden) return;
            
            fieldActorsHidden = false;
            fieldController.SetPlayerActive(true);
            RefreshFieldFollowers();
        }
    }

    #endregion
    
}