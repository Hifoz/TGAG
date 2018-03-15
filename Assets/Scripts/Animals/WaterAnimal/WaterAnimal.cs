using System;

public class WaterAnimal : Animal {

    override protected void Start() {
        base.Start();

        WaterAnimalSkeleton s = new WaterAnimalSkeleton(transform);
        s.generateInThread();
        setSkeleton(s);
    }

    override protected void Update() {

    }

    protected override void calculateSpeedAndHeading() {
        throw new NotImplementedException();
    }
}
