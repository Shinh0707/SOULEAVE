using System.Collections;
using UnityEngine;
public abstract class EffectUnit : ScriptableObject
{
    public string description;
    public abstract IEnumerator ApplyEffect(PlayerController player, int level, KeyCode triggerKey);
}