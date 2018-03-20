using UnityEngine;
using UnityEngine.UI;

public class AnimalDebug : MonoBehaviour {

    public Text text;
    Animal animal;

    Vector3 scale;

    void Start() {
        scale = transform.localScale;
    }

    // Update is called once per frame
    void Update() {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(new Vector3(0, 180, 0));
        if (animal) {
            transform.position = animal.transform.position + Vector3.up * 10;
            text.text = animal.getDebugString();
        } else {
            Debug.LogWarning("AnimalDebugger has no animal!");
        }

        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        if (dist < 100f) {
            float scaleFactor = dist / 100f;
            if (scaleFactor < 0.1f) {
                scaleFactor = 0.1f;
            }
            transform.localScale = scale * scaleFactor;
        }
    }

    public void setAnimal(Animal animal) {
        this.animal = animal;
    }
}
