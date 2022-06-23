using UnityEngine;

namespace UnityUtilitiesCode.UI
{
    /// <summary>
    /// Base class for all 'Canvas Element Data' scriptable objects, all of them should be derived from this class 
    /// </summary>
    public class CanvasElementData : ScriptableObject
    {
        [SerializeField] private string elementName;
        public string ElementName => elementName;
    }
}