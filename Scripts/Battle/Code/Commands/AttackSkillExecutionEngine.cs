
using Utils;

/// <summary>
/// Attack/Skitl共用执行引擎。
/// 这里专门负责真正的动作结算与表现，不再依赖额外上下类。
/// </summary>
public class AttackSkillExecutionEngine
{
    #region 运行时缓存

    private BattleController _controller;
    private BattleEntity _actor;
    private BattleCommandRequest _command;
    private List<BattleEntity> _targets;
    private SkillDataSO _skill;
    
    private readonly List<BattleEntity> _killedTargets = new();
    private bool _hasPlayedKillCinematic;
    private bool _breakCinematicRequested;

    #endregion

    #region 执行参数缓存

    // 这些字段只是把本次命令会反复读取的执行参数先算好。
    // 这样后面的物理/魔法/治疗分支可以直接读字段
    private float _powerMultiplier = 1f;
    private int _hitCount = 1;
    private float _groupInterval;
    private float _hitInterval;
    private float _vfxHitDelay;
    private DamageType _hitDamageType =  DamageType.None; 
    #endregion
    
    /* ----------------------------------------------------------------------------------------- */

    private void CacheExecutionParameters()
    {
        _killedTargets.Clear();
        _hasPlayedKillCinematic = false;
        _breakCinematicRequested = false;
        
        bool isPhysicalBranch = _skill.damageKind == DamageKind.Physical
            &&  _skill.skillType != SkillType.Heal;

        _powerMultiplier = _skill.GetBoostPowerMultiplier(_command.BPSpend);
        _hitCount = isPhysicalBranch ? _skill.GetFinalHitCount(_command.Type, _command.BPSpend) : 1;
        _groupInterval = _targets.Count > 1 ? _controller.Config.GroupTargetHitInterval : 0f;
        _hitInterval = _hitCount > 1 ? _controller.Config.MultiHitInterval : 0f;
        _hitDamageType = _skill.ResolveDamageType();
        _vfxHitDelay = Mathf.Max(0, _skill.vfxHitDelay);
    }
    
    public IEnumerator Execute(BattleController controller, List<BattleEntity> targets)
    {
        // 1.先把本次执行所需的上下文缓存起来。
        _controller = controller;
        _targets = targets;
        _actor = controller.CurrentEntity;
        _command = controller.CurrentCommand;
        _skill = _command.Skill;

        // 2.特殊技能直接走自己的逻辑，不再进入普通伤害分支。
        if (_skill.specialLogic != null)
        {
            yield return PlayAttackWithWindup();
            yield return _skill.specialLogic.ExecuteLogic(_controller, _actor, _command, _targets);
            yield break;
        }
        
        // 3.普通技能先统一把执行参数算好。
        CacheExecutionParameters();

        // 4.再按治疗/魔法/物理三条主分支结算。
        if (_skill.skillType == SkillType.Heal)
        {
            yield return ExecuteHealBranch();
        }else if (_skill.damageKind == DamageKind.Magical)
        {
            yield return ExecuteMagicalBranch();
        }
        else
        {
            yield return ExecutePhysicalBranch();
        }

        // 5.这一轮命中都跑完后，再统一处理错峰死亡VFX。
        FinishKillDissolves();
    }
    
    private IEnumerator ExecuteMagicalBranch()
    {
        yield return PlayAttackWithWindup(); // 播放动画

        var mode = GetVfxMode();
        bool hasPlayedVfx = false;
        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];
            if (!target.IsAlive) 
                continue;
            
            if (mode == SkillVfxSpawnMode.GroupCenter && !hasPlayedVfx)
            {
                yield return PlayHitVfx(target);
                hasPlayedVfx = true;
            }

            if (!hasPlayedVfx)
            {
                yield return PlayHitVfx(target);
            }
            
            var damager = target.CalculateDamageFrom(_actor, _skill, _powerMultiplier);
            ApplyDamageHit(target, damager);
            
            if (_groupInterval > 0f && i < _targets.Count - 1)
                yield return new WaitForSeconds(_groupInterval);
        }
    }

    private IEnumerator ExecutePhysicalBranch()
    {
        
        var mode = GetVfxMode();
        bool hasPlayedVfx = false;
        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];
            
            // boost修改
            var damager = target.CalculateDamageFrom(_actor, _skill, _powerMultiplier);

            for (int hitIndex = 0; hitIndex < _hitCount; hitIndex++)
            {
                if (!target.IsAlive) break;
                
                if (mode == SkillVfxSpawnMode.GroupCenter && !hasPlayedVfx)
                {
                    yield return PlayAttackWithWindup();
                    yield return PlayHitVfx(target);
                    hasPlayedVfx = true;
                }

                if (!hasPlayedVfx)
                {
                    yield return PlayAttackWithWindup();
                    yield return PlayHitVfx(target);
                }
                ApplyDamageHit(target, damager);
                
                if (_hitInterval > 0f && hitIndex < _hitCount - 1)
                    yield return new WaitForSeconds(_hitInterval);
            }
            
            if (_groupInterval > 0f && i < _targets.Count -1)
                yield return new WaitForSeconds(_groupInterval);
        }
        
        yield break;
    }
    
    private IEnumerator ExecuteHealBranch()
    {
        yield return PlayAttackWithWindup(); // 播放动画
        int healAmount = _actor.CalculateHealAmountFromSkill(_skill, _powerMultiplier);

        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];
            if (!target.IsAlive)
                continue;

            yield return PlayHitVfx(target);
            
            target.Heal(healAmount);
            Debug.Log($"[Battle] Heal {target.Definition.Name} +{healAmount}");
            _controller.SpawnDamagePopup(target, healAmount, DamagePopupType.Heal);
            
            if (_groupInterval > 0f && i < _targets.Count - 1)
                yield return new WaitForSeconds(_groupInterval);
        }
    }

    private IEnumerator PlayAttackWithWindup()
    {
        _actor.Unit.PlayAttackAnimation();
        float windup = _controller.Config.AttackWindupTime;
        if (windup > 0)
            yield return new WaitForSeconds(windup);
    }

    public void ResetExecutionState()
    {
        _powerMultiplier = 1f;
        _hitCount = 1;
        _groupInterval = 0f;
        _hitInterval = 0f;
    }

    private void ApplyDamageHit(BattleEntity target, int damage)
    {
        // 1. 先判断这一击会不会直接击杀。
        bool willKill = !target.IsPlayer && target.CurrentHP - damage <= 0;
        if (willKill)
            _killedTargets.Add(target);

        // 2.如果技能带镜头冲击，就先让表现层震一下。
        if (_skill.cameraImpulseStrength > 0f)
            target.Unit.PlayImpulse(_skill.cameraImpulseStrength);

        // 3.再真正写回伤害值，并生成飘字。
        _controller.SpawnDamagePopup(target, damage, DamagePopupType.Normal);
        target.TakeDamage(damage);

        // 4.击杀时只发击杀演出请求，不触发Break。
        if (willKill)
        {
            if (!_hasPlayedKillCinematic)
            {
                _hasPlayedKillCinematic = true;
                EventBus.Publish(new KillCinematicRequestedEvent(target));
            }

            return;
        }

        // 5.没有击杀的话，再尝试破盾和BoSS阶段即时切换。
        TryResolveBreakFromHit(target);
    }

    #region 破盾

    private void TryResolveBreakFromHit(BattleEntity target)
    {
        // 先读 CacheExecutionParameters（ 算好的_hitDamageType，
        // 这里只专心判断“弱点命中+护盾是否真的被扣掉”。
        // 只有“打到弱点+护盾真的被扣掉”时，才算触发Break。
        if (target.IsPlayer
            || !target.IsWeakTo(_hitDamageType)
            || !target.TryReduceShield(1))
            return;

        // 把破盾后的时间轴重排交给BattleController。
        _controller.NotifyEntityBrokenOrDead(target);

        if (_breakCinematicRequested)
            return;
        _breakCinematicRequested = true;
        EventBus.Publish(new BreakCinematicRequestedEvent());
    }

    #endregion

    #region 特效

    private IEnumerator PlayHitVfx(BattleEntity target)
    {
        SpawnSkillVfx(target);
        if (_vfxHitDelay > 0)
            yield return new WaitForSeconds(_vfxHitDelay);
    }

    private void SpawnSkillVfx(BattleEntity target)
    {
        var mode = GetVfxMode();
        bool spawnFromCaster = _skill.vfxSpawnFromCaster;
        var spawnRot = GetVfxRotation(spawnFromCaster);
        var spawnPos = spawnFromCaster
            ? _actor.Unit.transform.position
            : GetTargetVfxPosition(target, mode);
        
        var spawnOffest = spawnFromCaster
            ? spawnRot * _skill.vfxOffset
            : _skill.vfxOffset;

        GameObject vfx = Object.Instantiate(_skill.hitVfxPrefab, spawnPos + spawnOffest, spawnRot);
        float vfxLiftTime = Mathf.Max(0, _skill.vfxLifeTime);
        if (vfxLiftTime > 0f)
            Object.Destroy(vfx, vfxLiftTime);
    }

    private SkillVfxSpawnMode GetVfxMode()
    {
        if (_skill.vfxSpawnMode == SkillVfxSpawnMode.AutoByTargetType)
            return _targets.Count > 1
                ? SkillVfxSpawnMode.GroupCenter
                : SkillVfxSpawnMode.Target;
        
        return _skill.vfxSpawnMode;
    }

    private Quaternion GetVfxRotation(bool spawnFromCaster)
    {
        var baseRot = Quaternion.identity;
        if (spawnFromCaster)
            baseRot = _actor.Unit.transform.rotation;
        
        return baseRot * Quaternion.Euler(0f, _skill.vfxYRotation, 0f);
    }

    private Vector3 GetTargetVfxPosition(BattleEntity target, SkillVfxSpawnMode mode)
    {
        if (mode == SkillVfxSpawnMode.GroupCenter)
            return _controller.FieldManager.GetSideCenter(ResolveVfxSide());
        return target.Unit.GetPopupAnchorPosition();
    }

    private bool ResolveVfxSide()
    {
        return _skill.targetType switch
        {
            TargetType.AllAllies or TargetType.SingleAlly or TargetType.Self => _actor.IsPlayer,
            _=> !_actor.IsPlayer
        };
    }

    private void FinishKillDissolves()
    {
        float stagger = _controller.CinematicService.killDissolveStagger;
        for (int i = 0; i < _killedTargets.Count; i++)
        {
            var target = _killedTargets[i];
            target.Unit.PlayEnemyDissolve(stagger * (i - 1));
        }
    }
    
    #endregion
}