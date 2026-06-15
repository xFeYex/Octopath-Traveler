
using Utils;

public class ItemCommandHandler : BattleCommandHandlerBase
{
    private ConsumableItemSO _consumableItem;
    private BattleEntity _target;
    private bool _isHpItem;

    protected override bool PreparePhase()
    {
        _consumableItem = (ConsumableItemSO)Command.ItemDefinition;
        _target = Controller.AllEntities.Find(e => e.ID == Command.TargetEntityID);
        _isHpItem = _consumableItem.itemIconKey == ItemIconKey.Healing;
        return _target != null;
    }

    protected override IEnumerator AnimationPhase()
    {
        Actor.Unit.PlayUseItemAnimation();
        float windUp = Controller.Config.AttackWindupTime;
        if ( windUp > 0f)
            yield return new WaitForSeconds(windUp);
    }

    protected override IEnumerator ExecutionPhase()
    {
        int restoreAmount = _consumableItem.restoreAmount;
        
        InventoryManager.Instance.RemoveItem(_consumableItem, 1);

        if (_isHpItem)
        {
            _target.Heal(restoreAmount);
            Controller.SpawnDamagePopup(_target, restoreAmount, DamagePopupType.Heal);
        }
        else
        {
            _target.RestoreSP(restoreAmount);
            Controller.SpawnDamagePopup(_target, restoreAmount, DamagePopupType.Magic);
        }

        yield break;
    }

    protected override IEnumerator ResolvePhase()
    {
        float recover = Controller.Config.AttackRecoveryTime;
        if (recover > 0f)
            yield return new WaitForSeconds(recover);
    }
}