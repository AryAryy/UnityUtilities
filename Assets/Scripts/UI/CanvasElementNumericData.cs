using UnityEngine;

namespace UnityUtilitiesCode.UI
{
    /// <summary>
    /// Canvas element data for all numeric values, create an asset for each element
    /// </summary>
    [CreateAssetMenu(fileName = "Canvas Element Numeric Data", menuName = "Canvas Data/Canvas Element Numeric Data", order = 0)]
    public class CanvasElementNumericData : CanvasElementData
    {
        [SerializeField] private float currentValue;
        [SerializeField] private float defaultValue;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;

        public float CurrentValue => currentValue;
        public float MinValue => minValue;
        public float MaxValue => maxValue;

        public void ModifyCurrentValue(float value) => currentValue += value;
        
        private void ResetCurrentValue()
        {
            currentValue = defaultValue;
        }
        
        private void OnEnable() => ResetCurrentValue();

        private void OnDisable() => ResetCurrentValue();
    }
}