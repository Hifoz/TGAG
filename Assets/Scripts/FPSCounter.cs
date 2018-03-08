using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour {
    int frames = 0;
    float timer = 0;
    string fpsLastSecond = "Frames last second: N/A";

    // Update is called once per frame
    void Update() {
        frames++;
        timer += Time.deltaTime;
        if (timer >= 1) {
            fpsLastSecond = string.Format("Frames last second: {0}", frames);
            frames = 0;
            timer = 0;
        }
        GetComponent<Text>().text = string.Format("{0}\nFPS: {1}\n", fpsLastSecond, (1 / Time.deltaTime).ToString("N2"));
    }
}
