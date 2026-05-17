using System;
public class PlayerInteraction : MonoBehaviour
{
    private CharacterIdentity _characterIdentity;

    private void Awake()
    {
        _characterIdentity = GetComponent<CharacterIdentity>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InteractionBase interaction))
        {
            interaction.OnFocus(_characterIdentity.characterDefinition as AllyDefinitionSO);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out InteractionBase interaction))
        {
            interaction.OnLoseFocus(_characterIdentity.characterDefinition as AllyDefinitionSO);
        }
    }
}