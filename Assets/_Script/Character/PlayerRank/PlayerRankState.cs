// Author : Peiyu Wang @ Daphatus
// 09 02 2025 02 13

using _Script.Utilities.StateMachine;
using UnityEngine;

namespace _Script.Character.PlayerRank
{
/// <summary>
    /// Abstract state class – every rank state must derive from this and provide its own Rank and ExpRequired.
    /// </summary>
    public abstract class PlayerRankState : IState
    {
        /// <summary>
        /// The rank that this state represents.
        /// </summary>
        public abstract PlayerRankEnum Rank { get; }

        /// <summary>
        /// The total cumulative experience required to reach this rank.
        /// </summary>
        public abstract int ExpRequired { get; }

        protected PlayerRank PlayerRank { get; }

        public PlayerRankState(PlayerRank playerRank)
        {
            PlayerRank = playerRank;
        }

        public virtual void Enter()
        {
            Debug.Log("Entered rank: " + Rank);
        }

        public virtual void Exit()
        {
            Debug.Log("Exiting rank: " + Rank);
        }

        public virtual void UpdateState()
        {
            // Optional per-frame updates can go here.
        }
    }

    /// <summary>
    /// The starting rank.
    /// </summary>
    public class PlayerRankF : PlayerRankState
    {
        public PlayerRankF(PlayerRank playerRank) : base(playerRank) { }

        public override PlayerRankEnum Rank => PlayerRankEnum.F;

        // Although F is the starting rank, we set its ExpRequired to 0.
        public override int ExpRequired => 0;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Player starts at Rank F");
        }
    }

    /// <summary>
    /// Second rank: requires 100 exp to reach.
    /// </summary>
    public class PlayerRankE : PlayerRankState
    {
        public PlayerRankE(PlayerRank playerRank) : base(playerRank) { }

        public override PlayerRankEnum Rank => PlayerRankEnum.E;

        // Total experience required to be promoted to E.
        public override int ExpRequired => 100;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Promoted to Rank E!");
        }
    }

    /// <summary>
    /// Third rank: requires 1000 exp to reach.
    /// </summary>
    public class PlayerRankD : PlayerRankState
    {
        public PlayerRankD(PlayerRank playerRank) : base(playerRank) { }

        public override PlayerRankEnum Rank => PlayerRankEnum.D;

        // Total experience required to be promoted to D.
        public override int ExpRequired => 1000;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Promoted to Rank D!");
        }
    }

    /// <summary>
    /// Fourth rank: requires 5000 exp to reach.
    /// </summary>
    public class PlayerRankC : PlayerRankState
    {
        public PlayerRankC(PlayerRank playerRank) : base(playerRank) { }

        public override PlayerRankEnum Rank => PlayerRankEnum.C;

        // Total experience required to be promoted to C.
        public override int ExpRequired => 5000;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Promoted to Rank C!");
        }
    }

    /// <summary>
    /// Fifth (and highest in this example) rank: requires 20000 exp.
    /// </summary>
    public class PlayerRankB : PlayerRankState
    {
        public PlayerRankB(PlayerRank playerRank) : base(playerRank) { }

        public override PlayerRankEnum Rank => PlayerRankEnum.B;

        // Total experience required to be promoted to B.
        public override int ExpRequired => 20000;

        public override void Enter()
        {
            base.Enter();
            Debug.Log("Promoted to Rank B!");
        }
    }

    /// <summary>
    /// Enum to represent all possible player ranks.
    /// </summary>
    public enum PlayerRankEnum
    {
        F,
        E,
        D,
        C,
        B,
    }
}