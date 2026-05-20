
public class PlayerInteraction : MonoBehaviour
{
    private CharacterIdentity _characterIdentity;
    private InteractionBase _target;

    private void Awake()
    {
        _characterIdentity = GetComponent<CharacterIdentity>();
    }

    void Update()
    {
        if (_target is null || _target.CacheCommandInfo.Count == 0) return; ; 
        
        var input = InputSystemController.Instance;
        if (input is null) return;

        // E键
        if (input.GetPlayerConfirmPressed() && _target is not null)
        {
            _target.Interact(_characterIdentity.characterDefinition as AllyDefinitionSO);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InteractionBase interaction))
        {
            _target = interaction;
            interaction.OnFocus(_characterIdentity.characterDefinition as AllyDefinitionSO);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out InteractionBase interaction))
        {
            interaction.OnLoseFocus(_characterIdentity.characterDefinition as AllyDefinitionSO);
        }
        _target = null;
    }
}