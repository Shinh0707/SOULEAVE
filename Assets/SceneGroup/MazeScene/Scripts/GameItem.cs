using UnityEngine;

public abstract class GameItem : MonoBehaviour
{
    [SerializeField] protected string itemName;
    [SerializeField] protected float cooldownTime;
    [SerializeField] protected float duration;
    [SerializeField] protected AudioClip useSound;

    protected float currentCooldown;
    protected float currentDuration;

    protected PlayerController player;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public virtual bool Use()
    {
        if (currentCooldown <= 0 && currentDuration <= 0)
        {
            currentCooldown = cooldownTime;
            currentDuration = duration;
            PlayUseSound();
            OnUse();
            return true;
        }
        return false;
    }

    protected virtual void Update()
    {
        if (currentDuration > 0)
        {
            currentDuration -= Time.deltaTime;
            if (currentDuration <= 0)
            {
                OnEffectEnd();
            }
        }
        else if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    protected virtual void PlayUseSound()
    {
        if (useSound != null)
        {
            audioSource.PlayOneShot(useSound);
        }
    }

    public float GetCooldownProgress()
    {
        return 1 - (currentCooldown / cooldownTime);
    }

    public float GetDurationProgress()
    {
        return currentDuration / duration;
    }

    public string GetItemName()
    {
        return itemName;
    }

    protected abstract void OnUse();
    protected abstract void OnEffectEnd();
}