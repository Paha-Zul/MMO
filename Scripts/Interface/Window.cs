using UnityEngine;
using System.Collections;

public abstract class Window{
    public string windowName;
    int windowType;

    public Window(string name, int type)
    {
        this.windowName = name;
        this.windowType = type;
    }

    public abstract void update();
    public abstract void draw();
    public abstract void setActive(bool val);
    public abstract void toggleActive();
    public abstract bool isActive();
    public abstract void setTarget(object o);
}
