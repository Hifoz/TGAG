using System;

public abstract class AirAnimal : Animal {

    override protected abstract void move();

    private void Update() {
        if (skeleton != null) {
            move();
        }
    }

}
