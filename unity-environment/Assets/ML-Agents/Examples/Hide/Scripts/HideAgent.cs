using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideAgent : Agent
{

    [Header("Specific to Hide/Escape")]
    public GameObject HideSpace;
    public GameObject chaser;
    private HideChaser hideChaser;
    public GameObject[] spawnLocations;
    private Vector3 agentVelocity;
    public bool debug = false;
    private Rigidbody rb;
    private float distanceXNeg = 0f;
    private float distanceXPos = 0f;
    private float distanceZNeg = 0f;
    private float distanceZPos = 0f;

    private bool chaserHitByRaycast = false;
    private float lastDistanceBetweenChaser = 0;
    private float distanceBetweenChaser = 0;

    private bool willStartEvadedTimer = false;
    private bool evadedTimerActive = false;
    private float evadedTimer = 0.0f;
    private float evadedInterval = 5.0f;
    private float rewardBuffer = 0.0f;

    public HideAcademy hideAcademy;
    public GameObject winZone;

    [Header("GUI")]
    public Text stepsText;
    public Text scoreText;
    public Text winsText;
    public Text lossesText;
    public Text uparrowText;
    public Text downarrowText;
    public Text leftarrowText;
    public Text rightarrowText;
    private float totalReward;
    [HideInInspector]
    public int wins;
    [HideInInspector]
    public int losses;

    private void Start()
    {
        hideChaser = chaser.GetComponent<HideChaser>();
        rb = this.GetComponent<Rigidbody>();

        if(hideAcademy.mode == HideAcademy.Mode.Escape)
        {
            winZone.SetActive(true);
        } else if (hideAcademy.mode == HideAcademy.Mode.Hide)
        {
            winZone.SetActive(false);
        }

        totalReward = 0;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        UpdateGUI();
        Evasion();
    }

    void UpdateGUI()
    {
        stepsText.text = "Steps: " + stepCounter;
        scoreText.text = "Score: " + totalReward.ToString("F3");
        winsText.text = "Wins: " + wins.ToString();
        lossesText.text = "Losses: " + losses.ToString();
    }

    void Evasion()
    {
        if (evadedTimerActive == true)
        {
            if (evadedTimer > 0)
            {
                evadedTimer -= Time.fixedDeltaTime;
                rewardBuffer += reward;
            }
            else
            {
                evadedTimerActive = false;
                Debug.Log("Evaded in " + HideSpace.name);

                // Multiply reward for living if hiding, negate living penalty if escaping
                if (hideAcademy.mode == HideAcademy.Mode.Hide)
                    reward += rewardBuffer * 4.0f;
                else if (hideAcademy.mode == HideAcademy.Mode.Escape)
                    reward -= rewardBuffer;
            }
        }
    }

    void RaycastAtChaser()
    {
        RaycastHit raycastHit;
        Physics.Raycast(this.transform.position, chaser.transform.position - this.transform.position, out raycastHit);

        if (raycastHit.collider.name == "Chaser")
        {
            chaserHitByRaycast = true;
        }
        else
        {
            chaserHitByRaycast = false;
        }
    }

    void RaycastInDirections()
    {
        RaycastHit raycastHit;

        // Raycast right
        Physics.Raycast(this.transform.position, this.transform.right, out raycastHit);
        distanceXPos = (raycastHit.point - this.transform.position).magnitude;

        if (debug)
        {
            Debug.DrawLine(raycastHit.point, this.transform.position, Color.yellow);
        }

        // Raycast left
        Physics.Raycast(this.transform.position, -this.transform.right, out raycastHit);
        distanceXPos = (raycastHit.point - this.transform.position).magnitude;

        if (debug)
        {
            Debug.DrawLine(raycastHit.point, this.transform.position, Color.yellow);
        }

        // Raycast forward
        Physics.Raycast(this.transform.position, this.transform.forward, out raycastHit);
        distanceXPos = (raycastHit.point - this.transform.position).magnitude;

        if (debug)
        {
            Debug.DrawLine(raycastHit.point, this.transform.position, Color.yellow);
        }

        // Raycast back
        Physics.Raycast(this.transform.position, -this.transform.forward, out raycastHit);
        distanceXPos = (raycastHit.point - this.transform.position).magnitude;

        if(debug)
        {
            Debug.DrawLine(raycastHit.point, this.transform.position, Color.yellow);
        }
    }

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();

        // Where is the chaser?
        state.Add((HideSpace.transform.position.x - chaser.transform.position.x));
        state.Add((HideSpace.transform.position.z - chaser.transform.position.z));

        // Is the chaser in line of sight?
        if (chaserHitByRaycast)
            state.Add(1);
        else
            state.Add(0);

        // Is the chaser in FOV?
        if (hideChaser.agentInFOV)
            state.Add(1);
        else
            state.Add(0);

        // Is the chaser facing the agent?
        state.Add(Vector3.Dot((chaser.transform.position - this.transform.position).normalized, chaser.transform.forward));

        // Chaser's distance from agent
        state.Add(chaser.transform.position.x - this.transform.position.x);
        state.Add(chaser.transform.position.z - this.transform.position.z);
        
        // Where is the agent?
        state.Add((HideSpace.transform.position.x - this.transform.position.x));
        state.Add((HideSpace.transform.position.z - this.transform.position.z));

        // Velocity of agent?
        state.Add(rb.velocity.x);
        state.Add(rb.velocity.z);

        // Distance in each movement direction from agent
        state.Add(distanceXPos);
        state.Add(distanceXNeg);
        state.Add(distanceZPos);
        state.Add(distanceZNeg);

        // The extra states for the hide mode probably aren't needed, but make switching between the two modes easier
        if (hideAcademy.mode == HideAcademy.Mode.Escape)
        {
            // Where is the win zone?
            state.Add(HideSpace.transform.position.x - winZone.transform.position.x);
            state.Add(HideSpace.transform.position.z - winZone.transform.position.z);
        } else if (hideAcademy.mode == HideAcademy.Mode.Hide)
        {
            // Magnitude of distance from agent
            state.Add(distanceBetweenChaser);
            // Time until chaser touches the agent
            state.Add(distanceBetweenChaser/hideChaser.navAgent.speed);
        }

        return state;
    }

    public void MoveAgent(float[] act)
    {
        float directionX = 0;
        float directionZ = 0;
        float directionY = 0;

        if (brain.brainParameters.actionSpaceType == StateType.continuous)
        {
            directionX = Mathf.Clamp(act[0], -1f, 1f);
            directionZ = Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            int movement = Mathf.FloorToInt(act[0]);
            if (movement == 1)
            {
                directionX = -1;
                uparrowText.color = Color.red;
                downarrowText.color = Color.black;
                leftarrowText.color = Color.black;
                rightarrowText.color = Color.black;
            }
            if (movement == 2)
            {
                directionX = 1;
                uparrowText.color = Color.black;
                downarrowText.color = Color.red;
                leftarrowText.color = Color.black;
                rightarrowText.color = Color.black;
            }
            if (movement == 3)
            {
                directionZ = -1;
                uparrowText.color = Color.black;
                downarrowText.color = Color.black;
                leftarrowText.color = Color.red;
                rightarrowText.color = Color.black;
            }
            if (movement == 4)
            {
                directionZ = 1;
                uparrowText.color = Color.black;
                downarrowText.color = Color.black;
                leftarrowText.color = Color.black;
                rightarrowText.color = Color.red;
            }
        }

        this.rb.AddForce(new Vector3(directionX * 40f, directionY * 0f, directionZ * 40f));

        if (rb.velocity.sqrMagnitude > 25f)
        {
            rb.velocity *= 0.95f;
        }
    }

    // to be implemented by the developer
    public override void AgentStep(float[] act)
    {
        if (hideAcademy.mode == HideAcademy.Mode.Escape)
            reward = -0.005f;

        RaycastAtChaser();
        RaycastInDirections();

        if (chaserHitByRaycast)
        {
            Debug.DrawRay(this.transform.position, chaser.transform.position - this.transform.position, Color.red);
            hideChaser.InLOS();

            if (hideAcademy.mode == HideAcademy.Mode.Escape)
                reward = -0.01f;
            else if (hideAcademy.mode == HideAcademy.Mode.Hide)
                reward = -0.0025f;

            if (hideChaser.agentInFOV)
            {
                if (hideAcademy.mode == HideAcademy.Mode.Escape)
                    reward = -0.02f;
                else if (hideAcademy.mode == HideAcademy.Mode.Hide)
                    reward = -0.005f;

                willStartEvadedTimer = true;
                evadedTimerActive = false;
            }
        }
        else
        {
            hideChaser.agentInLOS = false;
            if (hideAcademy.mode == HideAcademy.Mode.Hide)
                reward += 0.005f;

            if(willStartEvadedTimer == true)
            {
                willStartEvadedTimer = false;
                evadedTimerActive = true;
                evadedTimer = evadedInterval;
                rewardBuffer = 0;
            }
        }

        MoveAgent(act);

        totalReward += reward;
    }

    // to be implemented by the developer
    public override void AgentReset()
    {
        gameObject.transform.position = spawnLocations[Random.Range(0, spawnLocations.Length)].transform.position;

        willStartEvadedTimer = false;
        evadedTimerActive = false;

        HideChaser hideChaser = chaser.GetComponent<HideChaser>();
        totalReward = 0;
        hideChaser.Reset();
    }
}
