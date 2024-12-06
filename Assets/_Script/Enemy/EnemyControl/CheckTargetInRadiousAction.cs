using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckTargetInRadious", story: "[Agent] Checks [Target]", category: "Action", id: "a90957601ef1db05b0898b78fbc390b0")]
public class CheckTargetInRadiousAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> range = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<string> layerMask = new BlackboardVariable<string>("Player");
    
    protected override Status OnStart()
    {
        return Status.Running;
    }
 
    private Collider2D CheckTargetInRange()
    {
        Vector2 agentPos = Agent.Value.transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(agentPos, range.Value, LayerMask.GetMask(layerMask.Value));
        
        // Create a combined layer mask for raycasting:
        foreach (var collider in colliders)
        {
            //check if the player is in the range
            if (collider.CompareTag("Player"))
            {
                // Draw a debug line to visualize the check

                var distance = Vector2.Distance(agentPos, collider.transform.position);
                Vector2 direction = collider.transform.position - Agent.Value.transform.position;
                /**temp solution**/
                var hitResults = Physics2D.RaycastAll(agentPos, direction, distance, LayerMask.GetMask("Obstacle"));
                return hitResults.Length == 0 ? collider : null;
            }
        }
        return null;
    }
    
    protected override Status OnUpdate()
    {
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
