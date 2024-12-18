using _Script.Map.WorldMap;
using _Script.Utilities;
using UnityEngine.InputSystem;

namespace _Script.Managers.PlayerState
{
    public class ExploreState : IState
    {
        private PlayerInput playerInput;

        public ExploreState(PlayerInput playerInput)
        {
            this.playerInput = playerInput;
        }

        public void Enter()
        {

        }

        public void Exit()
        {
            // Cleanup if needed
        }

        public void Update()
        {
            // Normal gameplay updates
        }
    }

    public class MapState : IState
    {
        private PlayerInput playerInput;

        public MapState(PlayerInput playerInput)
        {
            this.playerInput = playerInput;
        }

        public void Enter()
        {

        }

        public void Exit()
        {

        }

        public void Update()
        {
            
        }
    }

    public class CombatState : IState
    {
        private PlayerInput playerInput;

        public CombatState(PlayerInput playerInput)
        {
            this.playerInput = playerInput;
        }

        public void Enter()
        {
            // Switch to a "Combat" action map if you have one, or just use "Gameplay"
            playerInput.SwitchCurrentActionMap("Combat");
            // Disable map UI, show combat HUD if any
            MapExplorerUI.Instance.HideUI();
            // Movement might be restricted or altered
        }

        public void Exit()
        {
            // Cleanup combat-related UI
        }

        public void Update()
        {
            // Combat logic updates (if needed)
        }
    }
}