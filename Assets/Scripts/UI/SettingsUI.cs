using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


/*
 * Generates and manages the setting menu.
 * To add new settings and/or sections, add these in the generateMenu() function.
 */
public class SettingsUI : MonoBehaviour {

    public Font settingsFont;

    private int optionHeight = -50; // value must be negative, as we are working downwards instead of upwards, which unity ui does

    private Dictionary<string, Slider> _sliders = new Dictionary<string, Slider>();
    private Dictionary<string, Dropdown> _dropdowns = new Dictionary<string, Dropdown>();
    private Dictionary<string, InputField> _inputFields = new Dictionary<string, InputField>();
    // TODO : Add a toggle type?

    // What functions should be called when a setting is changed and saved
    private Dictionary<string, Func<object>> _settingExcecutions = new Dictionary<string, Func<object>>();


    void Start() {
        generateMenu();
        load();

        executeSettings();
    }

    public void OnEnable() {
        load();
    }

    // Save changes to PlayerPrefs
    public void save() {
        Debug.Log("Saving settings.");

        foreach (KeyValuePair<string, InputField> entry in _inputFields)
            PlayerPrefs.SetString(entry.Key, entry.Value.text);
        foreach (KeyValuePair<string, Slider> entry in _sliders)
            PlayerPrefs.SetFloat(entry.Key, entry.Value.value);
        foreach (KeyValuePair<string, Dropdown> entry in _dropdowns)
            PlayerPrefs.SetInt(entry.Key, entry.Value.value);

        PlayerPrefs.Save();

        executeSettings();
    }

    // Load settings from PlayerPrefs
    public void load() {
        foreach (KeyValuePair<string, InputField> entry in _inputFields)
            entry.Value.text = PlayerPrefs.GetString(entry.Key, entry.Value.text);
        foreach (KeyValuePair<string, Slider> entry in _sliders)
            entry.Value.value = PlayerPrefs.GetFloat(entry.Key, entry.Value.value);
        foreach (KeyValuePair<string, Dropdown> entry in _dropdowns)
            entry.Value.value = PlayerPrefs.GetInt(entry.Key, entry.Value.value);
    }

    // Run functions attached to the setting (eg. change resolution to what is set by the resolution setting) 
    private void executeSettings() {
        foreach (KeyValuePair<string, Func<object>> entry in _settingExcecutions) {
            if (entry.Value != null)
                entry.Value();
        }
    }

    // Set up the content of the settings menu here
    private void generateMenu() {
        GameObject panel = this.gameObject; // Yep


        // VIDEO SETTINGS:
        GameObject videoSettings = addSection("Video", panel);

        string[] resolutions = new string[] { "2560x1440", "1920x1080", "1280x720", "1024x768" };
        GameObject resolution = addDropdownOption("Resolution",
            videoSettings,
            resolutions,
            delegate { // Update screen resolution
                string[] dimensions = resolutions[PlayerPrefs.GetInt("Resolution")].Split('x');
                Screen.SetResolution(Int32.Parse(dimensions[0]), Int32.Parse(dimensions[1]), Screen.fullScreen);
                return null;
            }
        );

        GameObject windowMode = addDropdownOption("Window Mode",
            videoSettings,
            new string[] { "Windowed", "Fullscreen" },
            delegate {
                Screen.fullScreen = PlayerPrefs.GetInt("Window Mode", 0) == 1;
                return null;
            }
        );

        GameObject vsync = addDropdownOption("Vsync",
            videoSettings,
            new string[] { "Off", "On" },
            delegate {
                QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync", 1);
                return null;
            }
        );
        
        //pack(panel);
    }

    // Creates a basic ui object with a rect transform and a canvas renderer
    private GameObject createBaseUIObject(string objectName = "Unnamed", GameObject parent = null) {
        GameObject uiObject = new GameObject();
        uiObject.AddComponent<RectTransform>();
        uiObject.AddComponent<CanvasRenderer>();
        uiObject.name = objectName;
        if (parent != null)
            uiObject.transform.SetParent(parent.transform);

        RectTransform rt = uiObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, 0);

        return uiObject;
    }

    // Creates a section for containing and categorizing settings
    private GameObject addSection(string title, GameObject parent) {
        GameObject sectionPanel = createBaseUIObject("Section: " + title, parent);

        RectTransform rt = sectionPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, optionHeight);
        rt.offsetMax = new Vector2(0, 0);
        // Height will be set during packing


        GameObject sectionTitle = createBaseUIObject("Section title: " + title, sectionPanel);
        Text titleText = sectionTitle.AddComponent<Text>();
        titleText.text = title;
        titleText.font = this.settingsFont;
        titleText.fontSize = 25;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;

        rt = sectionTitle.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, optionHeight);
        rt.offsetMax = new Vector2(0, 0);


        return sectionPanel;
    }

    // Creates the base for which any of the option types below (slider, dropdown, etc..) build ontop of
    private GameObject addBasicOption(string text, GameObject parent) {
        GameObject option = createBaseUIObject("Option panel: " + text, parent);
        RectTransform rt = option.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, optionHeight * parent.transform.childCount);
        rt.offsetMax = new Vector2(0, optionHeight * (parent.transform.childCount - 1));

        float titleSpace = (text == "" ? 0 : 0.5f);
        if (text == "")
            titleSpace = 0;


        GameObject optionText = createBaseUIObject("Option Text: " + text, option);
        Text textComponent = optionText.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = settingsFont;
        textComponent.fontSize = 18;
        textComponent.alignment = TextAnchor.MiddleLeft;
        rt = optionText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(titleSpace, 1);
        rt.offsetMin = new Vector2(15, 5);
        rt.offsetMax = new Vector2(-5, -5);

        GameObject interactiveElement = createBaseUIObject("Option: " + text, option);
        rt = interactiveElement.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(titleSpace, 0);
        rt.offsetMax = new Vector2(-15, -15);
        rt.offsetMin = new Vector2(15, 15);

        return interactiveElement;
    }

    // Creates a textfield type option
    private GameObject addTextOption(string optionName, GameObject parent, string placeholderText = "", Func<object> func = null) {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        InputField inf = option.AddComponent<InputField>();
        inf.targetGraphic = option.GetComponent<Image>();


        GameObject text = createBaseUIObject("Text: " + optionName, option);
        Text textComponent = text.AddComponent<Text>();
        textComponent.font = settingsFont;
        textComponent.color = new Color(0, 0, 0);
        textComponent.alignment = TextAnchor.MiddleLeft;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 0);
        rt.offsetMax = new Vector2(-5, 0);


        GameObject placeholder = createBaseUIObject("Placeholder: " + optionName, option);
        Text placeholderTextComponent = placeholder.AddComponent<Text>();
        placeholderTextComponent.font = settingsFont;
        placeholderTextComponent.text = placeholderText;
        placeholderTextComponent.fontStyle = FontStyle.Italic;
        placeholderTextComponent.color = new Color(.5f, .5f, .5f);
        placeholderTextComponent.alignment = TextAnchor.MiddleLeft;

        rt = placeholder.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(5, 0);
        rt.offsetMax = new Vector2(-5, 0);


        inf.textComponent = text.GetComponent<Text>();
        inf.placeholder = placeholder.GetComponent<Text>();


        _inputFields.Add(optionName, inf);
        _settingExcecutions.Add(optionName, func);

        return option;
    }

    // Creates a slider type option
    private GameObject addSliderOption(string optionName, GameObject parent, int minval, int maxval, Func<object> func = null) {
        GameObject option = addBasicOption(optionName, parent);
        Slider slider = option.AddComponent<Slider>();
        slider.minValue = minval;
        slider.maxValue = maxval;

        GameObject background = createBaseUIObject("Background: " + optionName, option);
        background.AddComponent<Image>();

        GameObject fillArea = createBaseUIObject("Fill Area: " + optionName, option);
        GameObject fill = createBaseUIObject("Fill: " + optionName, fillArea);
        fill.AddComponent<Image>();

        GameObject handleSlideArea = createBaseUIObject("Handle Slide Area: " + optionName, option);
        GameObject handle = createBaseUIObject("Handle: " + optionName, handleSlideArea);
        handle.AddComponent<Image>().color = new Color(1, 0.5f, 0.5f);
        RectTransform rt = handle.GetComponent<RectTransform>();
        rt.offsetMax = new Vector2(5, 0);
        rt.offsetMin = new Vector2(-5, 0);

        GameObject minvalText = createBaseUIObject("Minval: " + optionName, option);
        GameObject maxvalText = createBaseUIObject("Maxval: " + optionName, option);
        GameObject currentvalText = createBaseUIObject("Currentval: " + optionName, option);

        foreach (GameObject obj in new GameObject[] { minvalText, maxvalText, currentvalText }) {
            Text text = obj.AddComponent<Text>();
            text.color = new Color(1, 1, 1);
            text.font = settingsFont;
            rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.offsetMax = new Vector2(0, 20);
        }
        minvalText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        maxvalText.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        currentvalText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        minvalText.GetComponent<Text>().text = minval.ToString();
        maxvalText.GetComponent<Text>().text = maxval.ToString();

        slider.onValueChanged.AddListener(delegate { currentvalText.GetComponent<Text>().text = slider.value.ToString("0.0"); });

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();


        _sliders.Add(optionName, slider);
        _settingExcecutions.Add(optionName, func);

        return option;
    }

    // Adds a dropdown type option
    private GameObject addDropdownOption(string optionName, GameObject parent, string[] elements, Func<object> func = null) {
        GameObject option = addBasicOption(optionName, parent);
        option.AddComponent<Image>();
        Dropdown dropdown = option.AddComponent<Dropdown>();
        foreach (string element in elements)
            dropdown.options.Add(new Dropdown.OptionData(element));


        GameObject label = createBaseUIObject("Label: " + optionName, option);
        Text labelText = label.AddComponent<Text>();
        labelText.text = "";
        labelText.font = settingsFont;
        labelText.color = new Color(0, 0, 0);
        RectTransform rt = label.GetComponent<RectTransform>();


        GameObject arrow = createBaseUIObject("Arrow: " + optionName, option);
        arrow.AddComponent<Image>().color = new Color(.5f, .5f, .5f);
        rt = arrow.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.offsetMin = new Vector2(-20, 0);

        GameObject template = createBaseUIObject("Template", option);
        template.AddComponent<Image>();
        ScrollRect templateSR = template.AddComponent<ScrollRect>();
        rt = template.GetComponent<RectTransform>();
        rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(0, -20 * elements.Length);
        templateSR.horizontal = false;
        templateSR.elasticity = 0.1f;
        template.SetActive(false);


        GameObject viewport = createBaseUIObject("Viewport", template);
        viewport.AddComponent<Mask>();
        viewport.AddComponent<Image>();

        GameObject content = createBaseUIObject("Content", viewport);
        rt = content.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, -20);

        GameObject item = createBaseUIObject("Item", content);
        Toggle itemToggle = item.AddComponent<Toggle>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, -20);

        GameObject itemBackground = createBaseUIObject("Item Background", item);
        itemBackground.AddComponent<Image>();

        GameObject itemHighlight = createBaseUIObject("Item Highlight", item);
        itemHighlight.AddComponent<Image>();

        GameObject itemLabel = createBaseUIObject("Item Label", item);
        Text itemLabelText = itemLabel.AddComponent<Text>();
        itemLabelText.font = settingsFont;
        itemLabelText.color = new Color(0, 0, 0);

        itemToggle.targetGraphic = itemBackground.GetComponent<Image>();
        itemToggle.graphic = itemHighlight.GetComponent<Image>();

        templateSR.content = content.GetComponent<RectTransform>();
        templateSR.viewport = viewport.GetComponent<RectTransform>();

        dropdown.captionText = label.GetComponent<Text>();
        dropdown.targetGraphic = dropdown.GetComponent<Image>();
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = itemLabelText;


        _dropdowns.Add(optionName, dropdown);
        _settingExcecutions.Add(optionName, func);

        return option;
    }

    // Resize the panel and sections inside so that they fit the number of settings and size of the window
    private void pack(GameObject panel) {
        int totalHeight = 0;


        for (int i = 0; i < panel.transform.childCount; i++) {
            RectTransform sectionRT = panel.transform.GetChild(i).gameObject.GetComponent<RectTransform>();
            int sectionItems = panel.transform.GetChild(i).childCount;
            sectionRT.offsetMin = new Vector2(0, totalHeight + sectionItems * optionHeight);
            sectionRT.offsetMax = new Vector2(0, totalHeight);
            totalHeight += sectionItems * optionHeight;

        }

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.offsetMin = new Vector2(0, totalHeight);
    }


}
