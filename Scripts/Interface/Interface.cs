using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Interface : MonoBehaviour {
	public Texture2D background, orderBackground;
	public Texture2D[] buttonBackground;
    public Texture2D[] marketButtonBackground;
	public GUIStyle style;
    public GUIStyle invStyle;

    public MarketInterface marketInterface;
    public List<Window> windowsList = new List<Window>();

    private ContextMenu menu;
    private Player playerScript;
    private Search searchScript;

	// Use this for initialization
	void Start () {
        //Initializes the market interface object
        this.marketInterface = new MarketInterface(this);
        this.marketInterface.background = this.background;
        this.marketInterface.orderBackground = this.orderBackground;
        this.marketInterface.buttonBackground = this.buttonBackground;
        this.marketInterface.style = this.style;

        //Initializes the inventory interface
        //this.playerInventoryInterface = new InventoryInterface(this.gameObject.GetComponent<Player>().getInventory(), invStyle, this);
        //this.marketVaultInventoryInterface = new InventoryInterface(invStyle, this);

        //Saves the player script reference.
        this.playerScript = this.gameObject.GetComponent<Player>();
        this.searchScript = this.gameObject.GetComponent<Search>();
	}

	/// <summary>
	/// Sets the interface to active and assigns the market object.
	/// </summary>
	/// <param name="val">If set to <c>true</c> value.</param>
	/// <param name="market">Market.</param>
	public void setMarketInterfaceActive(bool val, Market market){
		this.marketInterface.setActive(val);
		this.marketInterface.setMarket(market);
	}

	/// <summary>
	/// Toggles the active val and sets the market object.
	/// </summary>
	/// <param name="val">If set to <c>true</c> value.</param>
	/// <param name="market">Market.</param>
	public void toggleMarketInterface(Market market){
        this.marketInterface.toggleActive(market);
        if (!this.marketInterface.isWindowActive()) this.marketInterface.orderInfo.orderType = "none";

        if (this.anyWindowsActive())
            gameObject.BroadcastMessage("lockMovement", true); //Lock movement of the player.
        else
            gameObject.BroadcastMessage("lockMovement", false); //Unlock movement of the player.
	}

    public void setWindowActive(string name, bool val)
    {
        this.findWindowByName(name).setActive(val);
    }

    public void addNewWindow(string windowName, int type)
    {
        if(this.findWindowByName(name) != null) return;

        if(type == Constants.WINDOW_INVENTORY)
            this.windowsList.Add(new InventoryInterface(windowName, style, this));
    }

    public void removeWindow(string name)
    {
        for (int i = windowsList.Count - 1; i <= 0; i--)
            if (windowsList[i].windowName == name)
                windowsList.RemoveAt(i);
    }

    public void toggleWindow(string windowName)
    {
        Window window = this.findWindowByName(windowName);
        if (window == null)
        {
            window = new InventoryInterface(windowName, style, this);
            windowsList.Add(window);
        }
        window.toggleActive();

        if (this.anyWindowsActive())
        { //Lock movement of the player.
            gameObject.BroadcastMessage("lockMovement", true);
        }
        else
            //Lock movement of the player.
            gameObject.BroadcastMessage("lockMovement", false);
    }

    public bool setWindowTarget(string windowName, object o)
    {
        this.findWindowByName(windowName).setTarget(o);
        return true;
    }

    void Update()
    {
        if (this.marketInterface!= null && this.marketInterface.isWindowActive())
           this.marketInterface.update();

        foreach (Window window in windowsList)
            window.update();

       if (this.menu != null)
            this.menu.update();

       
    }

	void OnGUI(){
        if (this.marketInterface.isWindowActive()) //If the GUI is active, draw the market window
            this.marketInterface.run();

        foreach (Window window in windowsList)
            window.draw();

        if (this.menu != null)
            this.menu.draw();

        if (this.searchScript.hitMessage != "")
        {
            GUI.Label(new Rect(Screen.width / 2, Screen.height - Screen.height * 0.2f, 400f, 30f), this.searchScript.hitMessage);
        }
	}

    private bool anyWindowsActive()
    {
        foreach (Window window in windowsList)
            if (window.isActive())
                return true;
        return this.marketInterface.isWindowActive() || this.menu != null;
    }

    public void setContextMenu(ContextMenu menu)
    {
        this.menu = menu;
        if (this.anyWindowsActive()) gameObject.BroadcastMessage("lockMovement", true);
        else gameObject.BroadcastMessage("lockMovement", false);
    }

    public Player getPlayerScript()
    {
        return this.playerScript;
    }

    public Window findWindowByName(string name)
    {
        foreach (Window window in windowsList)
        {
            if (window.windowName == name)
                return window;
        }

        return null;
    }
}
