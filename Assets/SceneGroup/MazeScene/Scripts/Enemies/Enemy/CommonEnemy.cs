using SL.Lib;
using UnityEngine;

public class CommonEnemy : EnemyController
{
    

    protected override void Think()
    {
        Debug.Log($"Thinking! {gameObject.name}");
        virtualInput.Movement = Random.insideUnitCircle;
    }
}
