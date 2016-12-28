using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlScript : ScriptableObject
{
    /// <summary>
    /// The time it takes for an Ai script to make a move
    /// </summary>
    public const float AITimer = 5f;

    protected MapUnit myUnit;
    protected CombatManager combatInstance;

    /// <summary>
    /// The pattern for your turn
    /// </summary>
    public abstract void TurnPattern();

    /// <summary>
    /// How you attack
    /// </summary>
    public abstract void AttackPattern();

    /// <summary>
    /// How you move
    /// </summary>
    public abstract void MovePattern();

    public void init(MapUnit myUnit, CombatManager instance)
    {
        this.myUnit = myUnit;
        this.combatInstance = instance;

    }

}
