using UnityEngine;
using System.Collections;

public class InventoryInterface : Window{
    private const int PADDING_X = 10, PADDING_Y = 10; //At least this much padding.
    private const int SPACE_X = 2, SPACE_Y = 2; //The space between icons/items.
    private const int ICON_SIZE = 64;

    private int windowID = Random.Range(0, int.MaxValue);

    private int numIconsX = 0, numIconsY = 0;
    private float paddingY, paddingX;

    private GUIStyle style;
    private Interface inter;

    private bool active, rightClick=false;
    private Inventory inventory; //References the player's inventory;

    private Rect windowRect;

    public InventoryInterface(string windowName, Inventory inventory, GUIStyle style, Interface inter)
        : base(windowName, Constants.WINDOW_INVENTORY)
    {

        this.style = style;
        this.inter = inter;

        this.windowRect = new Rect(200, 200, 500, 500);
        this.inventory = inventory;

        this.numIconsX = (int)((windowRect.width - PADDING_X) / (ICON_SIZE + SPACE_X*2));
        this.paddingX = windowRect.width - (numIconsX * ICON_SIZE);

        this.numIconsY = (int)((windowRect.height - PADDING_Y) / (ICON_SIZE+SPACE_Y*2));
        this.paddingY = windowRect.height - (numIconsY * ICON_SIZE);
    }

    public InventoryInterface(string windowName, GUIStyle style, Interface inter) : this(windowName, null, style, inter) { }

    public override void update()
    {
        if (Input.GetMouseButtonDown(1))
            rightClick = true;
        else
            rightClick = false;
    }

    public override void draw()
    {
        if (this.active)
        {
            windowRect = GUI.Window(windowID, windowRect, DoMyWindow, "Inventory");
        }
    }

    void DoMyWindow(int windowID)
    {
        drawItems();

        //Allows the window to be dragged
        GUI.DragWindow(new Rect(0, 0, 10000, 10000)); //Drags the window
    }

    public void drawItems()
    {
        style.normal.background = null;
        style.hover.background = null;
        style.active.background = null;

        for (int y = 0; y < numIconsX; y++)
        {
            for (int x = 0; x < numIconsX; x++)
            {
                int index = x+y*numIconsX;
                Item item = null;
                if((index) < this.inventory.getNumItems())
                    item = this.inventory.getItem(index);

                if (item == null)
                    continue;

                Rect rect = new Rect(paddingX/2 + x*ICON_SIZE + x*SPACE_X, paddingY/2 + y*ICON_SIZE + y*SPACE_Y, ICON_SIZE, ICON_SIZE);
                GUI.DrawTexture(rect, item.getIcon());

                style.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(rect.x, rect.y, 100, 20), item.getItemName(), style);

                style.alignment = TextAnchor.UpperRight;
                GUI.Label(new Rect(rect.x + rect.width - 10, rect.y + rect.height - 20, 10, 20), this.inventory.getInventoryItem(x + y * numIconsX).getQuantity().ToString(), style);

                if (rightClick && rect.Contains(Event.current.mousePosition))
                {
                    Vector2 pos = Input.mousePosition;
                    pos.y = Screen.height - pos.y;
                    this.inter.setContextMenu(new ContextMenu("item_rightClick", inter, pos, this.inventory.getInventoryItem(index)));
                }
            }
        }
    }

    public override void setTarget(object o)
    {
        this.inventory = o as Inventory;
    }


    public override void setActive(bool val)
    {
        this.active = val;
    }

    public override void toggleActive()
    {
        this.active = !this.active;
    }

    public override bool isActive()
    {
        return this.active;
    }
}
