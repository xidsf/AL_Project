using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    public delegate void PlayerActionDelegate();
    static public PlayerActionDelegate applyAction;

    public delegate void ChangeDirectionDelegate();
    static public PlayerActionDelegate changeDir;

    private void AttackAction()
    {
        applyAction();
    }

    private void ChangeDirection()
    {
        changeDir();
    }

}
