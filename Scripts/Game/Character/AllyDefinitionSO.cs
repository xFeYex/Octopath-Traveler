
using Utils;

[CreateAssetMenu(menuName = "Character/Ally")]
public class AllyDefinitionSO : CharacterDefinitionSO
{
    [Header("Ally Definition")] 
    public PlayerJob job;
}