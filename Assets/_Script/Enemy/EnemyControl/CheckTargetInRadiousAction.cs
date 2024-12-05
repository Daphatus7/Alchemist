using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckTargetInRadious", story: "[Agent] Checks [Target]", category: "Action", id: "a90957601ef1db05b0898b78fbc390b0")]
public partial class CheckTargetInRadiousAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> range = new BlackboardVariable<float>(10f);

    protected override Status OnStart()
    {
        return Status.Running;
    }
 

    private Collider2D CheckTargetInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Agent.Value.transform.position, range);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return collider;
            }
        }
        return null;
    }
    
    
    protected override Status OnUpdate()
    {
        //Cooldown
        var target = CheckTargetInRange();
        if (target != null)
        {
            Target.Value = target.transform;
            return Status.Success;
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

