using TMPro;
using UnityEngine;

namespace MainMenu
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class QualityDropdown : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        private void Start()
        {
            foreach (var qName in QualitySettings.names) dropdown.options.Add(new TMP_Dropdown.OptionData(qName));

            dropdown.value = QualitySettings.GetQualityLevel();
            dropdown.onValueChanged.AddListener(OnQualitySelected);
        }

        private void OnQualitySelected(int newLevel)
        {
            QualitySettings.SetQualityLevel(newLevel, true);
        }
    }
}
