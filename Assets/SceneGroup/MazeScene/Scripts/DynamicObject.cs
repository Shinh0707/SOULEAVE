using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DynamicObject : MonoBehaviour
{
    public virtual void UpdateState()
    {
        //一括でタイミングを管理するため
    }

    private void FixedUpdate()
    {
        
    }
    private void Update()
    {
        
    }
}
