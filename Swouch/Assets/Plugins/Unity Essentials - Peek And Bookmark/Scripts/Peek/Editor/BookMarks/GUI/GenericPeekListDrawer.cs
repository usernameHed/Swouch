using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using unityEssentials.peek.extensions;
using unityEssentials.peek.extensions.editor;
using unityEssentials.peek.options;
using static UnityEditor.EditorGUILayout;

namespace unityEssentials.peek.gui
{
    public class GenericPeekListDrawer
    {
        public class DragSettings
        {
            public bool CanReorder;
            public Vector2 VerticalConstrain = Vector2.zero;
            public int CurrentIndexSelected { get; private set; } = -1;
            public int CurrentIndexShouldPointEmptyCell { get; private set; } = -1;
            public int RealDropIndex = -1;
            public int HowManyCellAreDisplayed { get; private set; } = 0;
            public bool IsDragging { get; private set; } = false;
            public Rect FirstItem { get; private set; }
            public Rect LastItem { get; private set; }
            public Vector2 VerticalCosntrain;
            public float InitialOffset { get; private set; } = 0;

            public void StartDragging(int index, Rect logo)
            {
                if (HowManyCellAreDisplayed < 1)
                {
                    return;
                }

                CurrentIndexSelected = index;
                CurrentIndexShouldPointEmptyCell = index;
                RealDropIndex = index;
                IsDragging = true;
                InitialOffset = -(Event.current.mousePosition.y - (logo.y + EditorGUIUtility.singleLineHeight / 2));
            }

            public void OnDrag(List<Object> listToDisplay, List<string> listToDisplayPath, List<Object> sceneLinked, List<Object> initialListToDisplay)
            {
                if (!IsDragging)
                {
                    return;
                }
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, 10000, 10000), MouseCursor.MoveArrow);
                if (Event.current.type == EventType.MouseDrag)
                {
                    InitialOffset = Mathf.Lerp(InitialOffset, 0, 0.2f);
                }

                if (Event.current.type == EventType.MouseUp)
                {
                    StopDragging(listToDisplay, listToDisplayPath, sceneLinked, initialListToDisplay);
                }
                if (!(Event.current.button == 0 && Event.current.isMouse)
                    && Event.current.type == EventType.Ignore)
                {
                    AbortDragging();
                }
            }

            /// <summary>
            /// when stop dragging, change order of lists
            /// abort if list is different than previously when drag first occure
            /// </summary>
            /// <param name="listToDisplay"></param>
            /// <param name="pathLinked"></param>
            /// <param name="initialListToDisplay"></param>
            public void StopDragging(List<Object> listToDisplay, List<string> pathLinked, List<Object> sceneLinked, List<Object> initialListToDisplay)
            {
                if (!ExtList.AreListHaveSameContent(listToDisplay, initialListToDisplay))
                {
                    AbortDragging();
                    return;
                }
                IsDragging = false;
                InitialOffset = 0;
                listToDisplay.Move(CurrentIndexSelected, RealDropIndex);
                pathLinked?.Move(CurrentIndexSelected, RealDropIndex);
                sceneLinked?.Move(CurrentIndexSelected, RealDropIndex);
                CurrentIndexSelected = -1;
                CurrentIndexShouldPointEmptyCell = -1;
            }

            public void AbortDragging()
            {
                IsDragging = false;
                InitialOffset = 0;
                CurrentIndexSelected = -1;
                RealDropIndex = -1;
                CurrentIndexShouldPointEmptyCell = -1;
            }

            public void SaveFirstItemPosition()
            {
                if (Event.current.type != EventType.Layout)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
                    rect.x -= EditorGUIUtility.standardVerticalSpacing;
                    rect.width -= SIZE_SCROLL_BAR;
                    FirstItem = rect;
                }
            }

            public void SaveLastItemPosition()
            {
                if (Event.current.type != EventType.Layout)
                {
                    LastItem = GUILayoutUtility.GetLastRect();
                }
            }

            public void CalculateEmptyCellIndexBasedOnPosition()
            {
                if (!IsDragging
                    || HowManyCellAreDisplayed < 2
                    || FirstItem.y == 0
                    || LastItem.y == 0)
                {
                    return;
                }

                float min = FirstItem.y + EditorGUIUtility.standardVerticalSpacing;
                float max = LastItem.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                float mousePosition = SetBetween(Event.current.mousePosition.y, min, max);
                int numberItem = HowManyCellAreDisplayed;

                float percent = 1 - ExtMathf.Remap(mousePosition, min, max, 0f, 1f);
                float indexPrecise = ExtMathf.Remap(percent, 0, 1, 0, numberItem);
                int index = Mathf.RoundToInt(indexPrecise - (1f / numberItem));

                index = SetBetween(index, 0, numberItem);
                CurrentIndexShouldPointEmptyCell = index;
            }

            public void SetHowManyCellAreDisplayed(int howMany)
            {
                HowManyCellAreDisplayed = howMany;
            }
        }

        private const float SIZE_SCROLL_BAR = 9f;
        private const string CLEAR_LIST = "d_TreeEditor.Trash";
        private const string DELETE_ICON = "winbtn_win_close";
        private const string GAMEOBJECT_ICON = "GameObject Icon";
        public const int WIDTH_BUTTON_HOVER = 30;

        public List<UnityEngine.Object> ListToDisplay { get; private set; }
        public List<string> ListToDisplayPath { get; private set; }
        public List<UnityEngine.Object> SceneLinkedPath { get; private set; }
        public ButtonImageWithHoverOptions BookMarkButtonOptions { get; private set; }
        public CalculateWidthExtentOptions CalculateWidthExtentOptions { get; private set; }

        public UnityAction<int> OnBookMarkClic { get; private set; }
        public UnityAction<int> OnSelectItem { get; private set; }
        public UnityAction<int> OnInfoItem { get; private set; }
        public UnityAction<int> OnInfoForceItem { get; private set; }

        public UnityAction<int> OnPinItem { get; private set; }
        public UnityAction<int> OnRemoveItem { get; private set; }
        public UnityAction OnClear { get; private set; }

        private HiddedEditorWindow _hiddedEditorWindow;
        private string _iconList;
        private string _listDescription;
        private bool _calculateExtent;
        private string _foldStateKey;
        private DragSettings _dragSettings = new DragSettings();
        private List<UnityEngine.Object> _listToDisplayCopy;


        public GenericPeekListDrawer(
            HiddedEditorWindow editorWindow,
            string iconList,
            string listDescription,
            bool calculateExtent,
            string foldStateKey,
            bool canReorder,

            List<UnityEngine.Object> list,
            List<string> listPath,
            List<UnityEngine.Object> sceneLinked,

            string iconBookMark,
            string iconHoverBookMark,
            string toolTipBookMark,

            UnityAction<int> onBookMarkClic,
            UnityAction<int> onSelectItem,
            UnityAction<int> onInfoItem,
            UnityAction<int> onInfoForceItem,
            UnityAction<int> onPinItem,
            UnityAction<int> onRemoveItem,
            UnityAction onClear)
        {
            _hiddedEditorWindow = editorWindow;
            _iconList = iconList;
            _listDescription = listDescription;
            _calculateExtent = calculateExtent;
            _foldStateKey = foldStateKey;
            _dragSettings.CanReorder = canReorder;
            ListToDisplay = list;
            ListToDisplayPath = listPath;
            SceneLinkedPath = sceneLinked;
            OnBookMarkClic = onBookMarkClic;
            OnSelectItem = onSelectItem;
            OnInfoItem = onInfoItem;
            OnInfoForceItem = onInfoForceItem;
            OnPinItem = onPinItem;
            OnRemoveItem = onRemoveItem;
            OnClear = onClear;

            BookMarkButtonOptions = new ButtonImageWithHoverOptions(
                iconBookMark,
                iconHoverBookMark,
                ExtGUIStyles.MicroButtonLeftCenter,
                toolTipBookMark,
                GUILayout.ExpandWidth(false), GUILayout.MaxWidth(30), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            CalculateWidthExtentOptions = new CalculateWidthExtentOptions(
                0.2f,
                10,
                AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
        }

        public void Display()
        {
            float widthEditorWindow = _hiddedEditorWindow.GetPeekNScrollEditorWindow().position.width;
            //float widthWithoutScrollBar = widthEditorWindow - SIZE_SCROLL_BAR;
            float widthScope = widthEditorWindow - SIZE_SCROLL_BAR * 2;

            if (ExtGUILayout.Section(_listDescription, _iconList, true, _foldStateKey, 0))
            {
                return;
            }
            _dragSettings.SaveFirstItemPosition();

            Color oldColor = GUI.color;
            _dragSettings.CalculateEmptyCellIndexBasedOnPosition();


            if (_dragSettings.IsDragging && _dragSettings.CurrentIndexShouldPointEmptyCell == ListToDisplay.Count)
            {
                DisplayEmptyCellItem(widthScope);
                _dragSettings.RealDropIndex = ListToDisplay.Count - 1;
                GUI.color = oldColor;
            }

            for (int i = ListToDisplay.Count - 1; i >= 0; i--)
            {
                if (_dragSettings.IsDragging && i == _dragSettings.CurrentIndexSelected)
                {
                    if (i == _dragSettings.CurrentIndexShouldPointEmptyCell)
                    {
                        DisplayEmptyCellItem(widthScope);
                        _dragSettings.RealDropIndex = i;
                    }
                    continue;
                }
                string nameToShow = ListToDisplayPath?[i];
                if (SceneLinkedPath != null && SceneLinkedPath[i] != null)
                {
                    nameToShow = SceneLinkedPath[i].name + "/" + nameToShow;
                }
                DisplayOneItem(ListToDisplay[i], nameToShow, i, widthScope, false);
                if (_dragSettings.IsDragging && i == _dragSettings.CurrentIndexShouldPointEmptyCell)
                {
                    DisplayEmptyCellItem(widthScope);
                    _dragSettings.RealDropIndex = i - (_dragSettings.CurrentIndexSelected < i ? 1 : 0);
                }

                GUI.color = oldColor;
            }

            _dragSettings.SaveLastItemPosition();
            _dragSettings.SetHowManyCellAreDisplayed(ListToDisplay.Count);

            ClearList(widthScope, OnClear);



            if (_dragSettings.CanReorder && _dragSettings.IsDragging)
            {
                IsDraggingItem(ListToDisplay[_dragSettings.CurrentIndexSelected], ListToDisplayPath?[_dragSettings.CurrentIndexSelected], _dragSettings.CurrentIndexSelected);
                GUI.color = oldColor;
            }
        }

        private void DisplayOneItem(Object toDisplay, string nameInListPath, int index, float width, bool disable)
        {
            if (toDisplay == null && string.IsNullOrEmpty(nameInListPath))
            {
                return;
            }

            if (toDisplay == null && !UnityEssentialsPreferences.GetBool(UnityEssentialsPreferences.SHOW_GAMEOBJECTS_FROM_OTHER_SCENE, true))
            {
                return;
            }

            GUI.color = (_hiddedEditorWindow.LastSelectedObject == toDisplay && toDisplay != null) ? Color.green : Color.white;

            EditorGUI.BeginDisabledGroup(disable);
            {
                using (HorizontalScope horizontalScope = new HorizontalScope(GUILayout.Width(width)))
                {
                    float widthExtent = CalculateWidthExtentOptions.CalculateWidthExtent(width, index, ListToDisplay.Count);
                    float widthButtonWithoutExtent = CalculateWidthExtentOptions.CalculateButtonWidthWithoutExtent(widthExtent);

                    if (_calculateExtent)
                    {
                        GUILayout.Label("", GUILayout.Width(widthExtent));
                    }

                    if (toDisplay == null)
                    {

                    }

                    bool clickedOnSpecialButton = false;
                    bool ctrlClicOnSpecialButton = false;
                    bool clicOnBookMark = BookMarkButtonOptions.ButtonImageWithHover(WIDTH_BUTTON_HOVER, toDisplay != null, OnInfoItem != null, ref clickedOnSpecialButton, ref ctrlClicOnSpecialButton);
                    if (ctrlClicOnSpecialButton)
                    {
                        OnInfoForceItem?.Invoke(index);
                        return;
                    }
                    else if (clickedOnSpecialButton)
                    {
                        OnInfoItem?.Invoke(index);
                        return;
                    }
                    else if (clicOnBookMark)
                    {
                        OnBookMarkClic?.Invoke(index);
                        return;
                    }

                    DisplayLogoByTypeOfObject(toDisplay, EditorGUIUtility.singleLineHeight);
                    if (!disable && !_dragSettings.IsDragging)
                    {
                        Rect logoContent = GUILayoutUtility.GetLastRect();
                        if (logoContent.Contains(Event.current.mousePosition))
                        {
                            EditorGUIUtility.AddCursorRect(logoContent, MouseCursor.MoveArrow);
                            if (Event.current.type == EventType.MouseDown)
                            {
                                _listToDisplayCopy = new List<Object>(ListToDisplay);
                                _dragSettings.StartDragging(index, logoContent);
                                Event.current.Use();
                            }
                        }

                    }

                    EditorGUI.BeginDisabledGroup(toDisplay == null);
                    {
                        string nameObjectToSelect;
                        if (toDisplay == null)
                        {
                            nameObjectToSelect = !string.IsNullOrEmpty(nameInListPath) ? ObjectNames.NicifyVariableName(nameInListPath) : " --- not found --- ";
                        }
                        else
                        {
                            nameObjectToSelect = ObjectNames.NicifyVariableName(toDisplay.name);
                        }


                        GUIContent buttonSelectContent = ShortenNameIfNeeded(nameObjectToSelect, width, widthButtonWithoutExtent);
                        if (GUILayout.Button(buttonSelectContent, BookMarkButtonOptions.GuiStyle, GUILayout.ExpandWidth(true)))
                        {
                            if (Event.current.button == 0)
                            {
                                OnSelectItem?.Invoke(index);
                            }
                            else
                            {
                                OnPinItem?.Invoke(index);
                            }
                            return;
                        }
                    }
                    EditorGUI.EndDisabledGroup();




                    GUIContent buttonDeletContent = EditorGUIUtility.IconContent(DELETE_ICON);
                    buttonDeletContent.tooltip = "Remove from list";
                    if (GUILayout.Button(buttonDeletContent, BookMarkButtonOptions.GuiStyle, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(WIDTH_BUTTON_HOVER))
                        && Event.current.button == 0)
                    {
                        OnRemoveItem?.Invoke(index);
                        return;
                    }
                    if (_calculateExtent)
                    {
                        GUILayout.Label("", GUILayout.Width(widthExtent));
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private GUIContent ShortenNameIfNeeded(string nameObjectToSelect, float currentWidth, float widthButton)
        {
            GUIContent contentCurrent = new GUIContent(nameObjectToSelect);
            Vector2 sizeCurrent = ExtGUIStyles.MicroButtonLeftCenter.CalcSize(contentCurrent);

            bool sizeOverflow = sizeCurrent.x > currentWidth - widthButton;

            if (sizeOverflow)
            {
                string shortName = CropUntilSizeIsGood(nameObjectToSelect, ExtGUIStyles.MicroButtonLeftCenter, currentWidth, widthButton);
                contentCurrent = new GUIContent(shortName, nameObjectToSelect);
            }
            else
            {
                contentCurrent = new GUIContent(nameObjectToSelect);
            }
            return (contentCurrent);
        }

        private static string CropUntilSizeIsGood(string originalName, GUIStyle guiStyleUsed, float currentWidth, float widthButton)
        {
            string croppedName = originalName;
            bool sizeOverflow = true;
            int maxIteration = 10;
            do
            {
                croppedName = CropStringByWordOrCaracter(croppedName, 1, 5);

                GUIContent contentCurrent = new GUIContent(croppedName);
                Vector2 sizeCurrent = guiStyleUsed.CalcSize(contentCurrent);

                sizeOverflow = sizeCurrent.x > currentWidth - widthButton;
                maxIteration--;
            } while (sizeOverflow && maxIteration > 0);

            return (croppedName + "...");
        }

        private static string CropStringByWordOrCaracter(string content, int wordsToCrop, int caracterToCropIfOnlyOneWord)
        {
            content = content.Trim(' ');
            string[] words = content.Split(' ');
            if (words.Length == 1)
            {
                if (words[0].Length < caracterToCropIfOnlyOneWord)
                {
                    return (words[0]);
                }
                return (words[0].Substring(0, words[0].Length - caracterToCropIfOnlyOneWord));
            }

            if (words.Length == 0)
            {
                return (content);
            }
            StringBuilder contentBuilder = new StringBuilder();
            for (int i = 0; i < words.Length - wordsToCrop; i++)
            {
                if (i != 0)
                {
                    contentBuilder.Append(" ");
                }
                contentBuilder.Append(words[i]);
            }
            return (contentBuilder.ToString());
        }

        private void DisplayEmptyCellItem(float width)
        {
            using (ExtGUILayout.HorizontalScope(new Color(0, 0, 0, 0.2f), GUILayout.Width(width), GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2)))
            {
                GUILayout.Label("");
            }
        }

        private void IsDraggingItem(Object toMove, string nameInPath, int index)
        {
            Rect position = new Rect(
                _dragSettings.FirstItem.x,
                Event.current.mousePosition.y + _dragSettings.InitialOffset - EditorGUIUtility.singleLineHeight / 2 - EditorGUIUtility.standardVerticalSpacing,
                _dragSettings.FirstItem.width,
                EditorGUIUtility.singleLineHeight);

            _dragSettings.VerticalConstrain = new Vector2(_dragSettings.FirstItem.y, _dragSettings.LastItem.y);
            if (_dragSettings.VerticalConstrain.x != 0 && _dragSettings.VerticalConstrain.y != 0)
            {
                position.y = SetBetween(position.y, _dragSettings.VerticalConstrain.x, _dragSettings.VerticalConstrain.y);
            }


            GUILayout.BeginArea(position);
            {
                DisplayOneItem(toMove, nameInPath, index, position.width, true);
            }
            GUILayout.EndArea();



            _dragSettings.OnDrag(ListToDisplay, ListToDisplayPath, SceneLinkedPath, _listToDisplayCopy);
            _hiddedEditorWindow.GetPeekNScrollEditorWindow().Repaint();
        }

        private void DisplayLogoByTypeOfObject(Object current, float width)
        {
            if (current == null)
            {
                GUIContent icon = EditorGUIUtility.IconContent(GAMEOBJECT_ICON);
                GUILayout.Label(icon, GUILayout.Width(width), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            else
            {
                Texture2D icon = AssetPreview.GetMiniThumbnail(current);
                GUILayout.Label(icon, GUILayout.Width(width), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        private void ClearList(float currentWidth, UnityAction actionOnClear)
        {
            using (new HorizontalScope(GUILayout.Width(currentWidth)))
            {
                GUILayout.Label("");
                GUIContent icon = EditorGUIUtility.IconContent(CLEAR_LIST);
                icon.tooltip = "Clear list";
                if (GUILayout.Button(icon, GUILayout.ExpandWidth(false))
                    && Event.current.button == 0)
                {
                    actionOnClear?.Invoke();
                }
                GUILayout.Label("");
            }
        }

        private static float SetBetween(float currentValue, float value1, float value2)
        {
            if (value1 > value2)
            {
                return (0);
            }

            if (currentValue < value1)
            {
                currentValue = value1;
            }
            if (currentValue > value2)
            {
                currentValue = value2;
            }
            return (currentValue);
        }
        private static int SetBetween(int currentValue, int value1, int value2)
        {
            if (value1 > value2)
            {
                return (0);
            }

            if (currentValue < value1)
            {
                currentValue = value1;
            }
            if (currentValue > value2)
            {
                currentValue = value2;
            }
            return (currentValue);
        }
    }
}