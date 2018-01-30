using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWinZone : MonoBehaviour
{
	void Start ()
    {

	}
	
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            HideAgent hideAgent = other.gameObject.GetComponent<HideAgent>();
            hideAgent.done = true;
            hideAgent.reward += 10.0f;
            hideAgent.wins += 1;
            hideAgent.AgentReset();
        }
    }
}
