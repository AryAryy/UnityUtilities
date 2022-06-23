using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtilitiesCode.UI
{
    public class CanvasManager : MonoBehaviour
    {
        private enum PanelType
        {
            MainMenu,
            HUD,
            GameOver
        }
        
        private enum IterationType
        {
            Ascending,
            Descending
        }

        [Serializable]
        private struct PanelObjects
        {
            public GameObject panel;
            public PanelType panelType;
        } 

        [Serializable]
        private struct ObjectActivationSettings
        {
            public GameObject obj;
            public bool status;
        }
        
        [Serializable]
        private struct ElementAdditionalSettings
        {
            [Tooltip(
                "If true will update all text objects in 'Text Objects' after data is modified with current value " +
                "in data or passed string value in string data")]
            public bool updateTexts;
            public TextMeshProUGUI[] textObjects;
            [Tooltip("If true will enable/disable objects in 'Objects To Toggle' after data is modified")]
            public bool toggleObjectActivation;
            [Tooltip(
                "If true will toggle active status of all objects in the list, if not after receiving " +
                "positive modification will toggle in ascending order and with negative modification will proceed with descending order"
            )]
            public bool toggleAll;
            public ObjectActivationSettings[] objectsToToggle;
        }

        [Serializable]
        private struct ElementDataEvents
        {
            public UnityEvent onPositiveChange;
            public UnityEvent onNegativeChange;
            public UnityEvent onReachMaximum;
            public UnityEvent onReachMinimum;
        }
        
        [Serializable]
        private struct ElementData
        {
            public CanvasElementData elementData;
            [Tooltip("These setting will be used with positive data modification")]
            public ElementAdditionalSettings positiveChangeAdditionalSettings;
            [Tooltip("These setting will be used with negative data modification")]
            public ElementAdditionalSettings negativeChangeAdditionalSettings;
            public ElementDataEvents elementEvents;
        }
        
        private static CanvasManager instance_;
        public static CanvasManager Instance => instance_;

        private Dictionary<string, ElementData> data_ = new Dictionary<string, ElementData>();

        [Header("Setup Before Entering Play Mode")]
        [SerializeField, Tooltip("All UI elements should be in this list")] 
        private ElementData[] elementDataList;
        [SerializeField, Tooltip("All UI panels should be in this list")]
        private PanelObjects[] uiPanels;
        [SerializeField, Tooltip("UI panel to activate after starting the scene")]
        private PanelType initialUIPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        
        public event Action OnStartGame;
        public event Action OnSettingMenu;
        public event Action OnQuitGame;

        public event Action OnNextLevel;
        public event Action OnRetry;
        public event Action OnMainManu;

        private void Awake()
        {
            instance_ = this;
            foreach (var element in elementDataList) data_.Add(element.elementData.ElementName, element);
            foreach (var p in uiPanels)
            {
                if (initialUIPanel == p.panelType)
                {
                    p.panel.SetActive(true);
                    continue;
                }

                p.panel.SetActive(false);
            }
        }

        /// <summary>
        /// This function adds a value to a numeric data in canvas manager
        /// </summary>
        /// <param name="key"> Data to modify </param>
        /// <param name="valueToAdd"> Value to add </param>
        public void AddToNumericData(string key, float valueToAdd)
        {
            ElementData mainData;
            if (data_.ContainsKey(key)) mainData = data_[key];
            else
                throw new Exception(key +
                                    " numeric element data is not available, create a 'Canvas Element Numeric Data' asset and add it to 'Element Data List'");

            var elementData = mainData.elementData as CanvasElementNumericData;
            if (!elementData)
                throw new Exception("Element data type is wrong, use a 'Canvas Element Numeric Data' asset");

            if (elementData.CurrentValue + valueToAdd <= elementData.MaxValue)
                elementData.ModifyCurrentValue(valueToAdd);
            HandleAdditionalSettings(elementData.CurrentValue.ToString(), mainData.positiveChangeAdditionalSettings,
                IterationType.Ascending);
            mainData.elementEvents.onPositiveChange?.Invoke();
            if (elementData.CurrentValue >= elementData.MaxValue) mainData.elementEvents.onReachMaximum?.Invoke();
        }

        /// <summary>
        /// This function subtracts a value from a numeric data in canvas manager
        /// </summary>
        /// <param name="key"> Data to modify </param>
        /// <param name="valueToSubtract"> Value to subtract </param>
        public void SubtractFromNumericData(string key, float valueToSubtract)
        {
            ElementData mainData;
            if (data_.ContainsKey(key)) mainData = data_[key];
            else
                throw new Exception(key +
                                    " numeric element data is not available, create a 'Canvas Element Numeric Data' asset and add it to 'Element Data List'");

            var elementData = mainData.elementData as CanvasElementNumericData;
            if (!elementData)
                throw new Exception("Element data type is wrong, use a 'Canvas Element Numeric Data' asset");
            
            if (elementData.CurrentValue - valueToSubtract >= elementData.MinValue)
                elementData.ModifyCurrentValue(-valueToSubtract);
            HandleAdditionalSettings(elementData.CurrentValue.ToString(), mainData.negativeChangeAdditionalSettings,
                IterationType.Descending);
            mainData.elementEvents.onNegativeChange?.Invoke();
            if (elementData.CurrentValue <= elementData.MinValue) mainData.elementEvents.onReachMinimum?.Invoke();
        }

        /// <summary>
        /// This function toggles a boolean data in canvas manager
        /// </summary>
        /// <param name="key"> Data to toggle </param>
        /// <param name="status"> Data status, true or false </param>
        public void ModifyBooleanValue(string key, bool status)
        {
            ElementData mainData;
            if (data_.ContainsKey(key)) mainData = data_[key];
            else
                throw new Exception(key +
                                    " boolean element data is not available, create a 'Canvas Element Boolean Data' asset and add it to 'Element Data List'");

            var elementData = mainData.elementData as CanvasElementBooleanData;
            if (!elementData)
                throw new Exception("Element data type is wrong, use a 'Canvas Element Boolean Data' asset");
            
            elementData.ModifyCurrentValue(status);
            var text = status ? elementData.TrueUIElementText : elementData.FalseUIElementText;
            var iterationType = status ? IterationType.Ascending : IterationType.Descending;
            var settings =
                status ? mainData.positiveChangeAdditionalSettings : mainData.negativeChangeAdditionalSettings;
            HandleAdditionalSettings(text, settings, iterationType);
            if (status) mainData.elementEvents.onPositiveChange?.Invoke();
            else mainData.elementEvents.onNegativeChange?.Invoke();
        }

        /// <summary>
        /// This function modifies a string data in canvas manager
        /// </summary>
        /// <param name="key"> Data to modify </param>
        /// <param name="text"> Desired data </param>
        /// <param name="dataModificationType"> Is this a positive or a negative data modification?
        /// If true, all implemented list iterations will be ascending and if false they will be descending,
        /// also event calls are based on this value </param>
        public void ModifyTextValue(string key, string text, bool dataModificationType)
        {
            ElementData mainData;
            if (data_.ContainsKey(key)) mainData = data_[key];
            else
                throw new Exception(key +
                                    " string element data is not available, create a 'Canvas Element String Data' asset and add it to 'Element Data List'");

            var elementData = mainData.elementData as CanvasElementTextData;
            if (!elementData)
                throw new Exception("Element data type is wrong, use a 'Canvas Element String Data' asset");
            
            elementData.ModifyCurrentValue(text);
            var iterationType = dataModificationType ? IterationType.Ascending : IterationType.Descending;
            var settings = dataModificationType
                ? mainData.positiveChangeAdditionalSettings
                : mainData.negativeChangeAdditionalSettings;
            HandleAdditionalSettings(elementData.CurrentValue, settings, iterationType);
            if (dataModificationType) mainData.elementEvents.onPositiveChange?.Invoke();
            else mainData.elementEvents.onNegativeChange?.Invoke();
        }

        /// <summary>
        /// This function handles all additional settings after modifying a data
        /// </summary>
        /// <param name="text"> Desired text to set on all text objects </param>
        /// <param name="settings"> Is this a positive or a negative data modification? </param>
        /// <param name="iteration"> List iteration type, is set according to data modification being positive or negative </param>
        private void HandleAdditionalSettings(string text, ElementAdditionalSettings settings, IterationType iteration)
        {
            if (settings.updateTexts)
            {
                foreach (var t in settings.textObjects)
                {
                    if (!t) throw new Exception("Text object variable in 'Text Objects' is null");
                    t.text = text;
                }
            }

            if (settings.toggleObjectActivation)
            {
                if (settings.toggleAll)
                {
                    foreach (var o in settings.objectsToToggle)
                    {
                        if (!o.obj) throw new Exception("Object variable in 'Objects To Toggle' is null");
                        o.obj.SetActive(o.status);
                    }

                    return;
                }

                switch (iteration)
                {
                    case (IterationType.Ascending):
                    {
                        for (var i = 0; i < settings.objectsToToggle.Length; i++)
                        {
                            var o = settings.objectsToToggle[i];
                            if (!o.obj) throw new Exception("Object variable in 'Objects To Toggle' is null");
                            if (o.status)
                            {
                                if (!o.obj.activeSelf)
                                {
                                    o.obj.SetActive(true);
                                    return;
                                }
                            }

                            else
                            {
                                if (o.obj.activeSelf)
                                {
                                    o.obj.SetActive(false);
                                    return;
                                }
                            }
                        }

                        break;
                    }
                    case (IterationType.Descending):
                    {
                        for (var i = settings.objectsToToggle.Length - 1; i >= 0; i--)
                        {
                            var o = settings.objectsToToggle[i];
                            if (!o.obj) throw new Exception("Object variable in 'Objects To Toggle' is null");
                            if (o.status)
                            {
                                if (!o.obj.activeSelf)
                                {
                                    o.obj.SetActive(true);
                                    return;
                                }
                            }

                            else
                            {
                                if (o.obj.activeSelf)
                                {
                                    o.obj.SetActive(false);
                                    return;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Star the game
        /// </summary>
        public void StartGame() => OnStartGame?.Invoke();

        /// <summary>
        /// Open setting menu
        /// </summary>
        public void SettingMenu() => OnSettingMenu?.Invoke();

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame() => OnQuitGame?.Invoke();

        /// <summary>
        /// Go to the next level
        /// </summary>
        public void NextLevel() => OnNextLevel?.Invoke();

        /// <summary>
        /// Retry the same level
        /// </summary>
        public void Retry() => OnRetry?.Invoke();

        /// <summary>
        /// Go to main menu
        /// </summary>
        public void MainMenu() => OnMainManu?.Invoke();

        /// <summary>
        /// This function is called when the level is over and player wins
        /// </summary>
        public void GameOverWin()
        {
            winPanel.SetActive(true);
        }
        
        /// <summary>
        /// This function is called when the level is over and player loses
        /// </summary>
        public void GameOverLose()
        {
            losePanel.SetActive(true);
        }

        //public void TestLog(string t) => Debug.Log(t);
    }
}