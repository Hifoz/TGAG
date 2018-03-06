using UnityEngine;
using System.Collections;

public class AirAnimalNPC : AirAnimal {

    override protected void Start() {
        base.Start();


        var test = new AirAnimalSkeleton(transform);
        test.generateInThread();
        setSkeleton(test);
    }

    override protected void move() {

    }
}
