using System;
using System.Collections;
using _Script.Managers;
using Pathfinding;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace _Script.Enemy.EnemyControl
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Flee2D", story: "[Agent] flees", category: "Action", id: "722dbdf50a6260a91e8fdd6a67fbf268")]
    public class Flee2DAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;

        // Distance range and threshold
        [SerializeReference] public BlackboardVariable<float> FleeDistance = new BlackboardVariable<float>(5f);
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);

        // How long to remain on cooldown after a successful flee
        [SerializeReference] public BlackboardVariable<float> FleeCooldown = new BlackboardVariable<float>(5f);

        private IAstarAI _mAIAgent;
        private Transform _mAgentTransform;
        private Vector3 _fleePoint;

        // Track cooldown
        private bool _isFleeOnCooldown;

        // We'll store any MonoBehaviour on the agent's gameobject to run coroutines
        private MonoBehaviour _agentMonoBehaviour;

        protected override Status OnStart()
        {
            // 1) If agent is on cooldown, no need to flee again. Return Success immediately.
            if (_isFleeOnCooldown)
            {
                return Status.Success;
            }

            // 2) Otherwise, do normal setup
            if (Agent.Value == null)
            {
                Debug.LogError("Flee2DAction: Agent is null.");
                return Status.Failure;
            }

            // Grab the A* component
            _mAIAgent = Agent.Value.GetComponent<IAstarAI>();
            if (_mAIAgent == null)
            {
                Debug.LogError("IAstarAI component is missing from the agent.");
                return Status.Failure;
            }

            // We'll also need some MonoBehaviour to start our coroutine
            _agentMonoBehaviour = Agent.Value.GetComponent<MonoBehaviour>();
            if (_agentMonoBehaviour == null)
            {
                Debug.LogError("No MonoBehaviour found on Agent to run coroutines.");
                return Status.Failure;
            }

            _mAgentTransform = Agent.Value.transform;

            // 3) Decide where to flee
            // If there's no SubGameManager or no reachable area, pick a random direction
            if (SubGameManager.Instance == null || SubGameManager.Instance.ReachableArea == null)
            {
                // random direction in 2D
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
                // random distance near the configured FleeDistance
                float randomDistance = FleeDistance.Value * UnityEngine.Random.Range(0.7f, 1.3f);
                _fleePoint = _mAIAgent.position + (Vector3)(randomDirection * randomDistance);
            }
            else
            {
                _fleePoint = SubGameManager.Instance.ReachableArea.GetARandomPosition();
            }

            // 4) Set the agent's destination
            _mAIAgent.destination = _fleePoint;

            // If we need to recalc the path right away
            _mAIAgent.SearchPath();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_mAIAgent == null || _mAgentTransform == null)
            {
                return Status.Failure;
            }

            // Continuously update the AI's destination
            _mAIAgent.destination = _fleePoint;

            float distanceToTarget = Vector3.Distance(_mAgentTransform.position, _fleePoint);

            // If the agent is within the threshold of the flee point, we consider it "done"
            if (distanceToTarget <= DistanceThreshold.Value)
            {
                // Stop the agent
                _mAIAgent.destination = _mAgentTransform.position;

                // Start the cooldown to prevent immediate re-flee
                _agentMonoBehaviour.StartCoroutine(StartFleeCooldown());

                return Status.Success;
            }

            // Otherwise, keep running until we get there
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

        /// <summary>
        /// Coroutine to enable "flee cooldown."
        /// During this time, any attempt to flee returns Success immediately.
        /// </summary>
        private IEnumerator StartFleeCooldown()
        {
            _isFleeOnCooldown = true;
            yield return new WaitForSeconds(FleeCooldown.Value);
            _isFleeOnCooldown = false;
        }
    }
}