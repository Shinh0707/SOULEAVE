using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillBankSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private Image skillImage;
    [SerializeField] private Button slotButton;

    private int bankNo;
    public System.Action<int> OnSlotClicked;

    private void Start()
    {
        slotButton.onClick.AddListener(() => OnSlotClicked?.Invoke(bankNo));
    }

    public void SetBankNo(int no)
    {
        bankNo = no;
        SetSkillName(PlayerStatus.Instance.SkillBank.GetSkillName(no));
    }

    public void SetSkillName(SelectableSkillName skillName)
    {
        if (skillName != null)
        {
            skillNameText.text = skillName.ToString();
            skillImage.sprite = skillName.Skill.data.skillImage;
        }
        else
        {
            skillNameText.text = "Empty";
            skillImage.sprite = null;
        }
    }
}
