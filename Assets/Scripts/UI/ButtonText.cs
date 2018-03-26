using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// For animating the colors on the text of a button.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    public Color baseColor;
    public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;
    public Color disabledColor;

    Text txt;
    Button btn;
    bool interactableDelay;

    void Awake() {
        txt = GetComponentInChildren<Text>();
        btn = gameObject.GetComponent<Button>();
    }

    void Start() {
        interactableDelay = btn.interactable;
    }

    void Update() {
        if (btn.interactable != interactableDelay) {
            if (btn.interactable) {
                txt.color = baseColor * normalColor * btn.colors.colorMultiplier;
            } else {
                txt.color = baseColor * disabledColor * btn.colors.colorMultiplier;
            }
        }
        interactableDelay = btn.interactable;
    }

    private void OnEnable() {
        if (btn.interactable) {
            txt.color = baseColor * normalColor * btn.colors.colorMultiplier;
        } else {
            txt.color = baseColor * disabledColor * btn.colors.colorMultiplier;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (btn.interactable) {
            txt.color = baseColor * highlightedColor * btn.colors.colorMultiplier;
        } else {
            txt.color = baseColor * normalColor * btn.colors.colorMultiplier;
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (btn.interactable) {
            txt.color = baseColor * pressedColor * btn.colors.colorMultiplier;
        } else {
            txt.color = baseColor * disabledColor * btn.colors.colorMultiplier;
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (btn.interactable) {
            txt.color = baseColor * highlightedColor * btn.colors.colorMultiplier;
        } else {
            txt.color = baseColor * disabledColor * btn.colors.colorMultiplier;
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (btn.interactable) {
            txt.color = baseColor * normalColor * btn.colors.colorMultiplier;
        } else {
            txt.color = baseColor * disabledColor * btn.colors.colorMultiplier;
        }
    }

}