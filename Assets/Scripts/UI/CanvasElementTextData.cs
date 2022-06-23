using UnityEngine;

namespace UnityUtilitiesCode.UI
{
    /// <summary>
    /// Canvas element data for all string values, create an asset for each element
    /// </summary>
    [CreateAssetMenu(fileName = "Canvas Element String Data", menuName = "Canvas Data/Canvas Element String Data", order = 0)]
    public class CanvasElementTextData : CanvasElementData
    {
        [SerializeField] private string currentValue;
        [SerializeField] private string defaultValue;

        public string CurrentValue => currentValue;
        
        public void ModifyCurrentValue(string value) => currentValue = value;
        
        private void ResetCurrentValue()
        {
            currentValue = defaultValue;
        }
        
        private void OnEnable() => ResetCurrentValue();

        private void OnDisable() => ResetCurrentValue();
    }
}