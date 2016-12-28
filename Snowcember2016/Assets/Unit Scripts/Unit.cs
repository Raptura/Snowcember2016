using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Unit : ScriptableObject
{
    public int maxHealth;
    public int speed; //determines attack order
    public int mov; //determines how far a unit can move
    public int range; //determines how far a unit can attack
    public int damage; //determines how much damage they deal

    public enum AttackType
    {
        Point,
        Line,
        Spread
    }
    public AttackType attackType;
}
