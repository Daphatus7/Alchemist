using System;
using _Script.Enemy.EnemyAbility;
using _Script.Utilities;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackAction", story: "[Agent] attacks [Target]", category: "Action", id: "fdaa1fed8d7e2b9432ca0b08451f06cf")]
public class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    
    protected override Status OnStart()
    {
        var attackAbility = Agent.Value.GetComponent<IEnemyAbilityHandler>();
        if(attackAbility == null)
        {
            Debug.LogError("Agent does not have an EnemyAbilityHandler component.");
            return Status.Failure;
        }
        attackAbility.UseAbility(Target.Value);
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

