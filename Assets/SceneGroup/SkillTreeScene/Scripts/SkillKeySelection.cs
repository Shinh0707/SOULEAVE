using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SkillKeySelection : MonoBehaviour
{
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CancelButton;
    [SerializeField] private GameObject SkillBankSlotPrefab;
    [SerializeField] private Transform SkillBankSlotContainer;
    [SerializeField] private GameObject mainWindow;
    [SerializeField] private TMP_Text messageText;

    private List<SkillBankSlot> skillBankSlots = new List<SkillBankSlot>();
    private SelectableSkillName currentSkillToAssign;

    private void Initialize()
    {
        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.interactable = false;
        CancelButton.onClick.RemoveAllListeners();
        CancelButton.interactable = true;
        CancelButton.onClick.AddListener(() => Close(false, -1));

        int maxSkillBank = PlayerStatus.Instance.PlayerParameter.MaxSkillBank;
        for (int i = 0; i < maxSkillBank; i++)
        {
            if (skillBankSlots.Count <= i)
            {
                GameObject slotObject = Instantiate(SkillBankSlotPrefab, SkillBankSlotContainer);
                SkillBankSlot slot = slotObject.GetComponent<SkillBankSlot>();
                skillBankSlots.Add(slot);
            }

            SkillBankSlot currentSlot = skillBankSlots[i];
            currentSlot.SetBankNo(i);
            currentSlot.OnSlotClicked = SlotSelect;

            currentSlot.gameObject.SetActive(true);
        }

        UpdateSlotDisplay();
    }

    private void Close(bool selected, int bankNo)
    {
        SkillTreeScene.Instance.State = SkillTreeSceneState.SkillSelect;
        if (selected && bankNo != -1)
        {
            PlayerStatus.Instance.SkillBank.AssignSkill(bankNo, currentSkillToAssign);
            SkillTreeScene.Instance.DialogManager.DisplaySkillDetails(currentSkillToAssign);
        }
        mainWindow.SetActive(false);
    }

    public void Open(SelectableSkillName skillToAssign)
    {
        SkillTreeScene.Instance.State = SkillTreeSceneState.KeySelect;
        currentSkillToAssign = skillToAssign;
        Initialize();
        UpdateMessageText();
        mainWindow.SetActive(true);
    }

    private void SlotSelect(int bankNo)
    {
        SelectableSkillName existingSkill = PlayerStatus.Instance.SkillBank.GetSkillName(bankNo);
        if (existingSkill != null)
        {
            ShowConfirmationDialog(bankNo, existingSkill);
        }
        else
        {
            Close(true, bankNo);
        }
    }

    private void ShowConfirmationDialog(int bankNo, SelectableSkillName existingSkill)
    {
        // ここで確認ダイアログを表示する
        // 例: ConfirmationDialog.Show($"Replace {existingSkill} with {currentSkillToAssign}?", 
        //     () => Close(true, bankNo), 
        //     () => {/* キャンセル時の処理 */});
    }

    private void UpdateSlotDisplay()
    {
        for (int i = 0; i < skillBankSlots.Count; i++)
        {
            SelectableSkillName skillName = PlayerStatus.Instance.SkillBank.GetSkillName(i);
            skillBankSlots[i].SetSkillName(skillName);
        }
    }

    private void UpdateMessageText()
    {
        messageText.text = $"Select a slot to assign {currentSkillToAssign}";
    }
}