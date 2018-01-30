using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HideChaser : MonoBehaviour {

    public GameObject myAgent;
    public GameObject chaserHead;
    [HideInInspector]
    public HideAgent hideAgent;

    public GameObject[] seekLocations;
    private float positionDoneThreshold = 0.15f;

    public GameObject[] chaserInitialPositions;

    [HideInInspector]
    public NavMeshAgent navAgent;

    [HideInInspector]
    public GameObject currentTarget;
    public GameObject targetLocationIndicator;
    
    [HideInInspector]
    public bool agentInLOS = false;
    [HideInInspector]
    public bool agentInFOV = false;
    [HideInInspector]
    public bool chasingAgent = false;
    public bool showDebug = false;

    [HideInInspector]
    public Vector3 lastKnownAgentLocation;

    private Rigidbody chaserRigidbody;

    public HideAcademy hideAcademy;

    public GameObject winZone;

    void Start ()
    {
        this.navAgent = this.GetComponent<NavMeshAgent>();
        this.chaserRigidbody = this.GetComponent<Rigidbody>();

        this.hideAgent = myAgent.GetComponent<HideAgent>();

        currentTarget = seekLocations[Random.Range(0, seekLocations.Length)];
        navAgent.SetDestination(currentTarget.transform.position);

        this.targetLocationIndicator.transform.parent = null;
        lastKnownAgentLocation = Vector3.zero;
    }
	
	void Update ()
    {
        PlaceTargetIndicator();
        MoveChaser();
    }

    void MoveChaser()
    {
        // If Agent Has Not Been Seen
        // Pick next room upon reaching destination
        if (Mathf.Abs((currentTarget.transform.position - this.transform.position).magnitude) < positionDoneThreshold && (chasingAgent == false))
        {
            //Debug.Log("Chaser Arrived At Location: " + currentTarget);
            PickNextRoom();
        }

        // If Agent Has Been Seen
        // Pick next room upon reaching last known location
        if ((Mathf.Abs((lastKnownAgentLocation - this.transform.position).magnitude) < positionDoneThreshold) && (chasingAgent == true) && (agentInLOS == false))
        {
            //Debug.Log("Chaser Arrived At LastKnownAgentLocation");
            chasingAgent = false;
            PickNextRoom();
        }

        // If Agent is currently seen, chase
        if (chasingAgent == true)
        {
            //Debug.Log("Chase Agent");
            ChaseAgent();
        }
    }

    public void InLOS()
    {
        if (agentInFOV)
        {
            agentInLOS = true;
            chasingAgent = true;
            lastKnownAgentLocation = myAgent.transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == myAgent.transform.gameObject)
        {
            HideAgent agent = myAgent.GetComponent<HideAgent>();
            agent.done = true;
            agent.reward -= 10f;
            agent.losses += 1;
        }
    }

    void PlaceTargetIndicator()
    {
        if (chasingAgent == false)
        {
            targetLocationIndicator.transform.position = currentTarget.transform.position;
        }
        else
        {
            targetLocationIndicator.transform.position = lastKnownAgentLocation;
        }
    }

    public void PickNextRoom()
    {
        GameObject tempCurrentTarget = seekLocations[Random.Range(0, seekLocations.Length)];
        while(tempCurrentTarget == currentTarget)
        {
            tempCurrentTarget = seekLocations[Random.Range(0, seekLocations.Length)];
        }
        if(tempCurrentTarget != currentTarget)
        {
            //Debug.Log("New target chosen: " + tempCurrentTarget + " Old target was: " + currentTarget);
            currentTarget = tempCurrentTarget;
            navAgent.SetDestination(currentTarget.transform.position);
            targetLocationIndicator.transform.position = currentTarget.transform.position;
        }
    }

    public void ChaseAgent()
    {
        currentTarget = myAgent;
        navAgent.SetDestination(lastKnownAgentLocation);
    }

    public void Reset()
    {
        // Get curriculum variables from academy
        float chaserSpeed = hideAcademy.GetComponent<HideAcademy>().chaserSpeed;

        // Set curriculum variables
        navAgent.speed = chaserSpeed;

        this.chaserRigidbody.isKinematic = true;

        GameObject chaserInitialPosition = chaserInitialPositions[Random.Range(0, chaserInitialPositions.Length)];

        this.transform.position = new Vector3(chaserInitialPosition.transform.position.x, this.transform.position.y, chaserInitialPosition.transform.position.z);
        this.transform.rotation = chaserInitialPosition.transform.rotation;

        if (hideAcademy.mode == HideAcademy.Mode.Escape)
        {
            if (winZone != null)
            {
                winZone.transform.position = new Vector3(chaserInitialPosition.transform.position.x, winZone.transform.position.y, chaserInitialPosition.transform.position.z);
            }
        }

        agentInLOS = false;
        chasingAgent = false;
        currentTarget = null;
        //lastKnownAgentLocation = Vector3.zero;
        this.chaserRigidbody.isKinematic = false;
        PickNextRoom();
    }
}
