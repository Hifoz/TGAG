using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class WindController : MonoBehaviour {
    public static float globalWindHeight = 140; // Always wind above this Y value
    public static float globalWindSpeed = 12;   // Speed used for global wind, also default for wind areas
    public static Vector3 globalWindDirection = Vector3.zero;

    private void Update() {
        globalWindDirection = Player.playerPos.get();
        globalWindDirection.y = 0;
        globalWindDirection = -globalWindDirection.normalized;
        globalWindDirection.y = -0.25f;
    }

}