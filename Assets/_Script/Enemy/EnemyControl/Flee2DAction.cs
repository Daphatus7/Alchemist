using System;
using _Script.Managers;
using Pathfinding;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace _Script.Enemy.EnemyControl
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Flee2D", story: "[Agent] flees with [coolddown]", category: "Action", id: "722dbdf50a6260a91e8fdd6a67fbf268")]
    public class Flee2DAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> Coolddown;
        // Flee range and threshold
        [SerializeReference] public BlackboardVariable<float> FleeDistance = new BlackboardVariable<float>(5f);
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
        
        // Whether the action is currently on cooldown
        [SerializeReference] public BlackboardVariable<bool> IsCooldown = new BlackboardVariable<bool>(false);

        private IAstarAI _mAIAgent;
        private Transform _mAgentTransform;
        private Vector3 _fleePoint;

        // Tracks the time when we are allowed to flee again
        private float _nextFleeTime;

        protected override Status OnStart()
        {
            // 1) If we're still on cooldown, set IsCooldown and return Success immediately
            //    so the Sequence node moves on to the next action.
            if (Time.time < _nextFleeTime)
            {
                IsCooldown.Value = true;
                return Status.Success;
            }
            else
            {
                // Not on cooldown
                IsCooldown.Value = false;
            }

            // 2) Normal setup if fleeing is allowed
            if (Agent.Value == null)
            {
                Debug.LogError("Flee2DAction: Agent is null.");
                return Status.Failure;
            }

            _mAIAgent = Agent.Value.GetComponent<IAstarAI>();
            if (_mAIAgent == null)
            {
                Debug.LogError("IAstarAI component is missing from the agent.");
                return Status.Failure;
            }

            _mAgentTransform = Agent.Value.transform;

            // 3) Pick a flee point
            if (SubGameManager.Instance == null || SubGameManager.Instance.ReachableArea == null)
            {
                // random direction in 2D
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
                // random distance around FleeDistance
                float randomDistance = FleeDistance.Value * UnityEngine.Random.Range(0.7f, 1.3f);
                _fleePoint = _mAIAgent.position + (Vector3)(randomDirection * randomDistance);
            }
            else
            {
                _fleePoint = SubGameManager.Instance.ReachableArea.GetARandomPosition();
            }

            // 4) Assign destination
            _mAIAgent.destination = _fleePoint;
            _mAIAgent.SearchPath();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // If references are missing, fail out
            if (_mAIAgent == null || _mAgentTransform == null)
            {
                return Status.Failure;
            }

            // Continuously move to the flee point
            // _mAIAgent.destination = _fleePoint;

            float distanceToTarget = Vector3.Distance(_mAgentTransform.position, _fleePoint);
            if (distanceToTarget <= DistanceThreshold.Value)
            {
                // Once we're close enough, stop and set cooldown
                _mAIAgent.destination = _mAgentTransform.position;
                _nextFleeTime = Time.time + Coolddown.Value;
                IsCooldown.Value = true;

                // Returning Success moves the Sequence to its next action
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Optional: forcibly stop movement if needed
            if (_mAIAgent != null && _mAgentTransform != null)
            {
                _mAIAgent.destination = _mAgentTransform.position;
            }
        }
    }
}