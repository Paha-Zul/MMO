using UnityEngine;
using System.Collections;

public class DisableCamera : MonoBehaviour {

	void Start(){
        if (uLink.Network.isClient)
            Destroy(this.gameObject);
	}
}
