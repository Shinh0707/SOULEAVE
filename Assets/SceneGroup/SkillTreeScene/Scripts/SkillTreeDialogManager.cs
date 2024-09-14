using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillTreeDialogManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI registerButtonText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] public RectTransform DialogRect;

    [SerializeField] private SkillKeySelection skillKeySelection;

    private Coroutine dialogAnimationCoroutine;
    private bool isDialogOpen = false;
    private float animationDuration = 0.3f;

    public IEnumerator Initialize()
    {
        // �K�v�ȏ��������W�b�N������΂����ɒǉ�
        yield return null;
    }

    public void DisplaySkillDetails(SelectableSkillName skillName)
    {
        skillNameText.text = skillName.skillName;
        skillLevelText.text = skillName.Skill.SkillLevelStr;
        skillDescriptionText.text = skillName.Skill.data.description;

        UpdateActionButton(skillName);
        ShowDialog();
    }

    public void CloseDialog()
    {
        HideDialog();
    }

    private void ShowDialog()
    {
        if (dialogAnimationCoroutine != null)
        {
            StopCoroutine(dialogAnimationCoroutine);
        }
        dialogAnimationCoroutine = StartCoroutine(AnimateDialog(true));
    }

    private void HideDialog()
    {
        if (dialogAnimationCoroutine != null)
        {
            StopCoroutine(dialogAnimationCoroutine);
        }
        dialogAnimationCoroutine = StartCoroutine(AnimateDialog(false));
    }

    private IEnumerator AnimateDialog(bool open)
    {
        float startX = DialogRect.anchoredPosition.x;
        float endX = open ? 0 : DialogRect.sizeDelta.x;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            float newX = Mathf.Lerp(startX, endX, t);
            DialogRect.anchoredPosition = new Vector2(newX, DialogRect.anchoredPosition.y);
            yield return null;
        }

        DialogRect.anchoredPosition = new Vector2(endX, DialogRect.anchoredPosition.y);
        isDialogOpen = open;
        dialogAnimationCoroutine = null;
    }

    private void UpdateActionButton(SelectableSkillName skillName)
    {
        Skill skill = skillName.Skill;
        if (skill.isUnlocked)
        {
            registerButton.gameObject.SetActive(true);
            registerButton.onClick.RemoveAllListeners();
            registerButton.interactable = true;
            if (PlayerStatus.Instance.SkillBank.IsSkillAssigned(skillName))
            {
                if (skill.data.HasManualEffects())
                {
                    int bankNo = PlayerStatus.Instance.SkillBank.GetSkillBankNo(skillName);
                    registerButtonText.text = $"�o�^����\n[�X���b�g{bankNo + 1}]";
                }
                else
                {
                    registerButtonText.text = $"������";
                }
                registerButton.onClick.AddListener(() => RemoveSkill(skillName));
            }
            else
            {
                if (skill.data.HasManualEffects())
                {
                    registerButtonText.text = $"�o�^";
                    registerButton.onClick.AddListener(() => AssignSkill(skillName));
                }
                else
                {
                    registerButtonText.text = $"�L����";
                    registerButton.onClick.AddListener(() => AssignSkill(skillName, true));
                }
            }
            if (skill.CanUpgrade())
            {
                int cost = skill.GetUpgradeCost();
                upgradeButtonText.text = $"�X�L������({cost}LP)";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.interactable = true;
                upgradeButton.onClick.AddListener(() => UpgradeSkill(skillName));
            }
            else
            {
                upgradeButtonText.text = "�ő僌�x��";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.interactable = false;
            }
        }
        else
        {
            registerButton.gameObject.SetActive(false);
            int cost = skill.GetUpgradeCost();
            upgradeButtonText.text = $"�X�L�����({cost}LP)";
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.interactable = SkillTree.Instance.CanUpgradeSkill(skill.data, PlayerStatus.Instance.PlayerParameter.LP);
            upgradeButton.onClick.AddListener(() => UpgradeSkill(skillName));
        }
    }

    private void AssignSkill(SelectableSkillName skillName, bool isPassive = false)
    {
        if (isPassive)
        {
            PlayerStatus.Instance.SkillBank.AssignSkill(0, skillName); // �p�b�V�u�X�L���͏��0�Ԗڂ̃X���b�g�Ɋ��蓖�Ă�
        }
        else
        {
            skillKeySelection.Open(skillName);
        }
        UpdateActionButton(skillName);
    }

    private void RemoveSkill(SelectableSkillName skillName)
    {
        PlayerStatus.Instance.SkillBank.RemoveSkill(skillName);
        UpdateActionButton(skillName);
    }

    private void UpgradeSkill(SelectableSkillName skillName)
    {
        int availableLP = PlayerStatus.Instance.PlayerParameter.LP;
        if (SkillTree.Instance.UpgradeSkill(skillName, ref availableLP))
        {
            PlayerStatus.Instance.PlayerParameter.LP = availableLP;
            UpdateActionButton(skillName);
        }
    }
}