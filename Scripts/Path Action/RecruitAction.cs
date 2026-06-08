
using System;

public class RecruitAction : ActionBase
{
    public CharacterDefinitionSO CurrentCharacter { get; private set; }
    
    
    /* ---------------------------------------------------------------------- */

    private void Awake()
    {
        CurrentCharacter = GetComponent<CharacterIdentity>().characterDefinition;
    }

    public override void TriggerAction(AllyDefinitionSO interaction)
    {
        EventBus.Publish(new PanelRequestEvent(this));
    }

    public override void Execute()
    {
        PartyManager.Instance.RecruitMember(CurrentCharacter);
        HideSceneNPC(); 
    }

    private void HideSceneNPC()
    {
        this.gameObject.SetActive(false);
    }
}
