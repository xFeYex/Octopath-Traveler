
using Utils;

public class EnemyAIState : BattleState
{
    private class ActionDecision
    {
        public SkillDataSO SelectedSkill;
        public bool IsTelegraph;
        public float Weight;
    }
    
    public EnemyAIState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute()
    {
        // 1.先处理当前敌人是否有挂起的rSS阶段切换提示。
        yield return PlayPendingBossPhaseIntro();        
        // 2.再正式让AI给自己组出一条BattleCommandRequest。
        _controller.CurrentCommand = BuildAICommand();
        // 3.稍微等待一下，模拟敌人思考的节奏感停顿。
        yield return new WaitForSeconds(_controller.Config.AIThinkDuration);
        // 4.最后切到执行状态，真正把命令打出去。
        _controller.SetState(new PerformActionState(_controller));
    }

    private IEnumerator PlayPendingBossPhaseIntro()
    {
        BattleEntity actor = _controller.CurrentEntity;
        // 1.先尝试把挂起的BosS阶段正式应用掉。
        if (!actor.TryApplyPendingBossPhase(out BossPhaseConfig appliedPhase))
            yield break;
        
        // 2.应用成功后，先发战斗提示文案。
        EventBus.Publish(new BattleNotificationEvent(appliedPhase.promptText));
        
        // 3.如配置了阶段切换延迟，就在这里给一个短暂停顿。
        float introDelay = appliedPhase.introDelay;
        if (introDelay > 0)
            yield return new WaitForSeconds(introDelay);
        
    }

    private BattleCommandRequest BuildAICommand()
    {
        BattleEntity actor = _controller.CurrentEntity;
        
        // 1.如果这个敌人上一回合已经进入了蓄力状态
        if (actor.PreparedSkill != null)
            return BuildPreparedSkillCommand();
        
        // 2.否则先做一次常规行动候选评估。
        ActionDecision decision = EvaluateActions();
        
        // 3.若这次选中的是“蓄力型技能，这一回合先进入蓄力准备。
        if (decision.IsTelegraph)
            return BuildTelegraphCommand(decision.SelectedSkill);
        
        // 4.最后退回普通技能/普攻命令。
        return BuilderCommand(decision.SelectedSkill);
    }

    #region 蓄力技能

    private BattleCommandRequest BuildTelegraphCommand(SkillDataSO skill)
    {
        var actor = _controller.CurrentEntity;
        
        // 1.先把这招记录为“下回合要释放的准备技能”。
        string skillName = skill.skillName;
        actor.PreparedSkill = skill;

        // 2.再立刻播放头顶蓄力特效。
        actor.Unit.PlayTelegraphVfx(skill);
        
        // 3.然后用技能名提示和战斗通知告诉玩家“敌人正在蓄力”。
        EventBus.Publish(new BattleNotificationEvent("主教练正在热身"));
        EventBus.Publish(new SkillNameDisplayEvent(actor, skillName));

        return BattleCommandRequest.CreateDefend();
    }

    /// <summary>
    /// 构建准备好的技能命令
    /// 该法用于处理角色已准备的蓄力技能，将其转换为实际的战命令
    /// </summary>
    private BattleCommandRequest BuildPreparedSkillCommand()
    {
        var actor = _controller.CurrentEntity;
        
        // 1.先取出这次真正要释放的蓄力技能。
        var skillToCast = actor.PreparedSkill;
        var skillName = skillToCast.skillName;

        // 2.释放前先清掉“准备中”状态和头顶蓄力特效。
        actor.PreparedSkill = null;
        actor.Unit.StopTelegraphVfx();
        
        // 3。再补一条战斗通知，告诉玩家这招现在正式打出来了。
        EventBus.Publish(new BattleNotificationEvent("主教练开始出击了！"));
        EventBus.Publish(new SkillNameDisplayEvent(actor, skillName));

        return BuilderCommand(skillToCast);
    }

    #endregion

    private BattleCommandRequest BuilderCommand(SkillDataSO selectedSkill)
    {
        BattleCommandRequest command = selectedSkill == _controller.CurrentEntity.Definition.BasicAttack
            ? BattleCommandRequest.CreateAttack(_controller.CurrentEntity)
            : BattleCommandRequest.CreateSkill(selectedSkill);
        
        AutoTargetSelection(command);
        return command;
    }

    private SkillDataSO ChooseFirstAvailableSkill(BattleEntity actor)
    {
        if (actor.Definition.InitalSkills != null)
        {
            for (int i = 0; i < actor.Definition.InitalSkills.Count; i++)
            {
                var skill = actor.Definition.InitalSkills[i];
                if (skill.spCost <= actor.CurrentSP)
                    return skill;
            }
        }

        return actor.Definition.BasicAttack;
    }

    private void AutoTargetSelection(BattleCommandRequest command)
    {
        var skill = command.Skill;
        
        // 1.群体和Self目标不需要再选具体目标，清掉TargetEntityId 即可。
        if (skill.targetType != TargetType.SingleAlly && skill.targetType != TargetType.SingleEnemy)
        {
            command.TargetEntityID = null;
            return;
        }
        
        // 2.单体目标才需要真的挑一个合法候选。
        var candidates = BattleTargeting.GetAliveTargetsByType(
            _controller.CurrentEntity,
            skill.targetType,
            _controller.AllEntities);

        if (candidates.Count == 0) return;

        // 3.单体治疗优先挑血量最低的队友，其它情况先随机一个合法目标。
        bool isSingleAllyHeal = skill.targetType == TargetType.SingleAlly && skill.skillType == SkillType.Heal;
        
        BattleEntity target = isSingleAllyHeal 
            ? GetLowestHpTarget(candidates)
            : candidates[Random.Range(0, candidates.Count)];
        
        command.TargetEntityID = target.ID;
    }

    private BattleEntity GetLowestHpTarget(List<BattleEntity> candidates)
    {
        var bestTarget = candidates[0];
        float bestRatio = bestTarget.CurrentHP / bestTarget.TotalStats.MaxHP;

        for (int i = 1; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            float ratio = candidate.CurrentHP / candidate.TotalStats.MaxHP;
            if (ratio >= bestRatio)
                continue;
            
            bestTarget = candidate;
            bestRatio = ratio;
        }
        
        return bestTarget;
    }

    /// <summary>
    /// 评估敌方当前可用的所有行动，并打分.
    /// </summary>
    private ActionDecision EvaluateActions()
    {
        var actor = _controller.CurrentEntity;
        CharacterDefinitionSO def = actor.Definition;
        BossPhaseConfig phase = actor.GetActiveBossPhaseConfig();
        EnemyAITurningConfig turning = ((EnemyDefinitionSO)def).AiTuning;

        List<ActionDecision> candidates = new()
        {
            new ActionDecision
            {
                SelectedSkill = def.BasicAttack,
                IsTelegraph = false,
                Weight = ApplyPhaseWeight(turning.basicAttackWeight,
                    phase != null ? phase.basicAttackWeightMultiplier : 1)
            }
        };
        
        int currentSp = actor.CurrentSP;
        for (int i = 0; i < def.InitalSkills.Count; i++)
        {
            SkillDataSO skill = def.InitalSkills[i];
            if (skill.spCost > currentSp)
                continue;

            float weight = 0f;
            bool isTelegraph = false;

            switch (skill.skillType)
            {
                case SkillType.Damage:
                    if (IsTelegraphSkill(skill, turning))
                    {
                        isTelegraph = true;
                        weight = ShouldPreferTelegraphSkill(turning)
                            ? ApplyPhaseWeight(turning.telegraphWegiht, phase != null 
                                ? phase.telegraphWeightMultiplier : 1)
                            : ApplyPhaseWeight(turning.damageSkillWeight, phase != null
                                    ? phase.damageWeightMultiplier : 1);
                    }
                    break;
                case SkillType.Heal:
                    weight = EvaluateHealWeight(skill, turning);
                    weight = ApplyPhaseWeight(weight, phase != null ? phase.healWeightMultiplier : 1);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                    weight = turning.defaultSkillWeight;
                    break;
            }

            if (weight > 0)
            {
                candidates.Add(new ActionDecision
                    {
                        SelectedSkill = skill,
                        IsTelegraph = isTelegraph,
                        Weight = weight
                    }
                );
            }
        }

        return PickByWeight(candidates);
    }

    private ActionDecision PickByWeight(List<ActionDecision> candidates)
    {
        float totalWeight = 0f;
        foreach (ActionDecision candidate in candidates)
            totalWeight += candidate.Weight;
        
        float randomWeight = Random.Range(0f, totalWeight);

        foreach (ActionDecision candidate in candidates)
        {
            randomWeight -= candidate.Weight;
            if (randomWeight <= 0f)
                return candidate;
        }
        
        return candidates[^1];
    }

    private float ApplyPhaseWeight(float baseWeight, float multiplier)
    {
        return baseWeight *  multiplier;
    }

    private static bool IsTelegraphSkill(SkillDataSO skill, EnemyAITurningConfig turning)
    {
        return skill.basePower >= turning.telegraphMinBasePower
               && skill.spCost >= turning.telegraphMinSpCost;
    }

    /// <summary>
    /// 是否进入"优先蓄力大招"阶段。
    /// 说明：这里只决定权重提升，不决定技能是否需要先蓄力。
    /// 只要技能被认定为蓄力技，被选中后都会先进入蓄力回合
    /// </summary>
    private bool ShouldPreferTelegraphSkill(EnemyAITurningConfig turing)
    {
        BattleEntity actor = _controller.CurrentEntity;
        return actor.CurrentHP / (float)actor.TotalStats.MaxHP <= turing.telegraphHpRatioThreshold;
    }

    /// <summary>
    /// 精准评估治疗技能的权重.
    /// </summary>
    private float EvaluateHealWeight(SkillDataSO skill, EnemyAITurningConfig turning)
    {
        int lowHpCount = 0;
        foreach (var ally in _controller.AllEntities)
        {
            if (!ally.IsPlayer && ally.IsAlive
                && ally.CurrentHP / (float)ally.TotalStats.MaxHP <= turning.healLowHpRatioThreshold)
                lowHpCount++;
        }

        if (lowHpCount <= 0)
            return 0;

        float weight = turning.healBaseWeight + lowHpCount * turning.healPerLowHpBonus;
        return skill.targetType == TargetType.SingleAlly && lowHpCount > 1
            ? weight * turning.singleHealMultiLowHpPenalty
            : weight;
    }
}