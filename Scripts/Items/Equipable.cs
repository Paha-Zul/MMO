using UnityEngine;
using System.Collections;

public class Equipable : ItemComp {
	private string slot;

	public Equipable(string slot){
		this.slot = slot;
	}

}
