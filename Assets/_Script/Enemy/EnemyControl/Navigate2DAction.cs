// Author : Peiyu Wang @ Daphatus
// Date : 05 12 2024

using Pathfinding;
using Unity.Behavior;
using UnityEngine;

using System;
using Unity.Properties;
using Action = Unity.Behavior.Action;

namespace _Script.Enemy.EnemyControl
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Navigate2D", story: "[Agent] Navigate 2D to [Target]", category: "Action/Navigation", id: "b2c3d8a739d5481d923fa7f4dff5c1a2")]
    public class Navigate2DAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Transform> Target;

        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);

        private IAstarAI _mAIAgent;
        private EnemyCharacter.EnemyCharacter _mEnemyCharacter;
        private Transform _mAgentTransform;
        

        protected override Status OnStart()
        {
            if (Agent == null || Agent.Value == null)
            {
                Debug.LogError("Agent is not assigned.");
                return Status.Failure;
            }

            if (Target == null || Target.Value == null)
            {
                Debug.LogError("Target is not assigned.");
                return Status.Failure;
            }

            _mAgentTransform = Agent.Value.transform;
            _mAIAgent = Agent.Value.GetComponent<IAstarAI>();

            if (_mAIAgent == null)
            {
                Debug.LogError("IAstarAI component is missing from the agent.");
                return Status.Failure;
            }
            
            _mEnemyCharacter = Agent.Value.GetComponent<EnemyCharacter.EnemyCharacter>();
            if (_mEnemyCharacter == null)
            {
                Debug.LogError("EnemyCharacter component is missing from the agent.");
                return Status.Failure;
            }
            
            // Set speed and destination
            _mAIAgent.maxSpeed = _mEnemyCharacter.MoveSpeed;
            Debug.Log("speed: " + _mAIAgent.maxSpeed + " plan: " + _mEnemyCharacter.MoveSpeed);
            _mAIAgent.destination = Target.Value.position;

            return Status.Running;
        }

        
        
        protected override Status OnUpdate()
        {
            if (_mAIAgent == null || _mAgentTransform == null || Target.Value == null)
            {
                return Status.Failure;
            }

            // Update the destination to follow the target
            _mAIAgent.destination = Target.Value.position;

            float distanceToTarget = Vector3.Distance(_mAgentTransform.position, Target.Value.position);

            // Check if the agent is within the threshold distance of the target
            if (distanceToTarget <= DistanceThreshold)
            {
                _mAIAgent.destination = _mAgentTransform.position; // Stop movement
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Reset the agent's destination to stop movement
            if (_mAIAgent != null)
            {
                _mAIAgent.destination = _mAgentTransform.position;
            }
        }
    }
}
