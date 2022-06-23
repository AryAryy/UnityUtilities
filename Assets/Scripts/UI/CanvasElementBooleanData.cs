using UnityEngine;

namespace UnityUtilitiesCode.UI
{
    /// <summary>
    /// Canvas element data for all boolean values, create an asset for each element
    /// </summary>
    [CreateAssetMenu(fileName = "Canvas Element Boolean Data", menuName = "Canvas Data/Canvas Element Boolean Data", order = 1)]
    public class CanvasElementBooleanData : CanvasElementData
    {
        [SerializeField] private bool currentValue;
        [SerializeField] private bool defaultValue;
        [SerializeField] private string trueUIElementText;
        [SerializeField] private string falseUIElementText;

        public bool CurrentValue => currentValue;
        public string TrueUIElementText => trueUIElementText;
        public string FalseUIElementText => falseUIElementText;

        public void ModifyCurrentValue(bool value) => currentValue = value;
        
        private void ResetCurrentValue()
        {
            currentValue = defaultValue;
        }
        
        private void OnEnable() => ResetCurrentValue();

        private void OnDisable() => ResetCurrentValue();
    }
}