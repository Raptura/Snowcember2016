using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public CanvasGroup MainPanel;

    public Button Movement, AcceptMovement,
        AcceptAttack, Attack, EndTurn, CenterCursor;

    public Text TurnText;

    public CanvasGroup PlayerText, HighlightText;
}
