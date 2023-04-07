using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiSurvivorSlotProps : MonoBehaviour
{
    public TextMeshProUGUI uiSurvivorName;
    public TextMeshProUGUI uiSkill;
    public TextMeshProUGUI uiTrait;
    public TextMeshProUGUI uiInfection;
    public TextMeshProUGUI uiAttack;
    public TMP_Dropdown uiEquip;
    public TextMeshProUGUI uiEquipText;
    public Button uiEquipBtn;
    public Button uiUnequip;
    public Button uiAbandon;
    public List<int> equipIndex;
}
