using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using Berezhnoy.Dialogue.Serializable;

namespace Berezhnoy.Dialogue.Editor
{

    public class DialogueGraph : EditorWindow
    {

        #region Outer processors

        [MenuItem("Tools/Graph/Dialogue")]
        public static void OpenDialogueGraphView()
        {

            var window = GetWindow<DialogueGraph>();

            window.titleContent = new GUIContent("Dialogue Graph");

        }

        #endregion

        #region Constants

        static readonly public Vector2 minimapDefaultPosition = new Vector2(10, 30);
        static readonly public Vector2 minimapDefaultSize = new Vector2(200, 140);
        static readonly public Rect miniMapPosition = new Rect(minimapDefaultPosition, minimapDefaultSize);

        #endregion

        #region Fields

        private DialogueGraphView targetGraphView;

        private string EditedFileName = "New Narrative";

        #endregion

        #region Events

        private void OnEnable()
        {

            ConstructGraphView();

            GenerateToolbar();

            GenerateMinimap();

            GenerateBlackboard();

        }

        private void OnDisable()
        {

            rootVisualElement.Remove(targetGraphView);

        }

        #endregion

        #region Methods

        private void ConstructGraphView()
        {

            targetGraphView = new DialogueGraphView(this)
            {

                name = "Dialogue Graph"

            };

            VisualElementExtensions.StretchToParentSize(targetGraphView);

            rootVisualElement.Add(targetGraphView);

        }

        private void GenerateToolbar()
        {
            // Toolbar itself
            var toolbar = new Toolbar();

            // Buttons group related to data processing
            var DataDropDownMenu = StylishedToolbarMenu("Data");

            DataDropDownMenu.menu.AppendAction("Save", a => { RequestDataOperation(true); }, a => DropdownMenuAction.Status.Normal);
            DataDropDownMenu.menu.AppendAction("Load", a => { RequestDataOperation(false); }, a => DropdownMenuAction.Status.Normal);

            toolbar.Add(DataDropDownMenu);

            // Textfield with a edited file name
            var fileNameTextField = new TextField("File Name");

            fileNameTextField.SetValueWithoutNotify(EditedFileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback((evt) => EditedFileName = evt.newValue);

            toolbar.Add(fileNameTextField);


            // Instantiate toolbar in editor window
            rootVisualElement.Add(toolbar);

        }

        private void GenerateMinimap()
        {

            var miniMap = new MiniMap()
            {

                anchored = true

            };

            var cords = targetGraphView.contentViewContainer.WorldToLocal(new Vector2(maxSize.x - 10, 30));

            miniMap.SetPosition(new Rect(cords, minimapDefaultSize));

            targetGraphView.Add(miniMap);

        }

        private void GenerateBlackboard()
        {

            var targetBlackboard = new Blackboard(targetGraphView);

            targetBlackboard.Add(new BlackboardSection()
            {

                title = "Exposed properties"

            });

            // Event
            targetBlackboard.addItemRequested = (blackboard => 
            {
                
                targetGraphView.AddPropertyToBlackboard(new ExposedProperty());
                
            });

            // Event
            targetBlackboard.editTextRequested = (blackboard, element, newValue) =>
            {

                var blackboardFieldElement = (BlackboardField) element;

                var oldPropertyName = blackboardFieldElement.text;

                if(targetGraphView.exposedProperties.Any(x => x.PropertyName == newValue))
                {

                    EditorUtility.DisplayDialog("Error", "This property name already exists, please choose another one!", "OK");

                    return;

                };

                var propertyIndex = targetGraphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);

                targetGraphView.exposedProperties[propertyIndex].PropertyName = newValue;

                blackboardFieldElement.text = newValue;

            };

            targetBlackboard.SetPosition(DialogueGraphView.BlackboardDefaultPosition);

            targetGraphView.targetBlackboard = targetBlackboard;

            targetGraphView.Add(targetBlackboard);

        }

        #endregion

        #region Buttons

        private void RequestDataOperation(bool IsSaving)
        {

            if (string.IsNullOrEmpty(EditedFileName))
            {

                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name", "OK");

                return;

            };

            var SaveLoadUtility = DialogueGraphSaveLoadUtility.GetInstance(targetGraphView);

            if (IsSaving)
            {

                SaveLoadUtility.Save(EditedFileName);

            }
            else
            {

                SaveLoadUtility.Load(EditedFileName);

            };

        }

        #endregion

        #region UI elements

        private ToolbarMenu StylishedToolbarMenu(string label)
        {

            var styleshedToolbarMenu = new ToolbarMenu() { text = label };

            styleshedToolbarMenu.style.paddingTop = 4;
            styleshedToolbarMenu.style.paddingBottom = 4;
            styleshedToolbarMenu.style.paddingLeft = 4;
            styleshedToolbarMenu.style.paddingRight = 4;

            return styleshedToolbarMenu;

        }

        #endregion

    }

}