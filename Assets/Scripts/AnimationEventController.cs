using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    public delegate void UnitActionDelegate();
    static public UnitActionDelegate applyAction;

    public delegate void ChangeDirectionDelegate();
    static public UnitActionDelegate changeDir;



    private void AttackAction()
    {
        applyAction();
    }

    private void ChangeDirection()
    {
        changeDir();
    }

}
