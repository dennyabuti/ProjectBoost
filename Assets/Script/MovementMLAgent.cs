using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MovementMLAgent : Agent
{
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float rotationThrust = 1f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] ParticleSystem mainBoosterEffect;
    [SerializeField] ParticleSystem leftBoosterEffect;
    [SerializeField] ParticleSystem rightBoosterEffect;


    public bool useVecObs;
    GameObject goal;
    Rigidbody rb;
    AudioSource audioSource;
    float previousDistance = float.MaxValue;
    float elapsedTime;
    float maxVal = int.MaxValue;
    Vector3 originalPos;
    Vector3 originalGoalPos;

    // Start is called before the first frame update
    public override void Initialize()
    {
        var parent = transform.parent;

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.tag == "Finish")
                {
                    goal = child.gameObject;
                    break;
                }
            }
        }
        else
        {
            goal = GameObject.FindWithTag("Finish");
        }

        originalPos = transform.position;
        originalGoalPos = goal.transform.position;
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     ProcessThrust();
    //     ProcessRotation();
    // }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;
        var rotate = Mathf.Clamp(continuousActions[0], -1f, 1f);
        // var distance = Vector3.Distance(transform.position, goal.transform.position);

        // if (distance < previousDistance) {
        //     AddReward(.1f / (distance * (Time.realtimeSinceStartup - elapsedTime)));
        //     // Debug.Log("reward: " + (.1f / (distance * (Time.realtimeSinceStartup - elapsedTime))));
        // } else {
        //     // Debug.Log("inverse Reward: " + (-.1f / (distance * (Time.realtimeSinceStartup - elapsedTime))));
        //     AddReward(-.1f / (distance * (Time.realtimeSinceStartup - elapsedTime)));
        // }

        // previousDistance = distance;

        // // Debug.Log(1f / ((distance + 1) * (Time.realtimeSinceStartup - elapsedTime)));

        AIProcessThrust(discreteActions[0]);
        AIProcessRotation(rotate);
        AddReward(-1f / MaxStep);
        // Debug.Log("Current reward: " + GetCumulativeReward());
    }

    public override void OnEpisodeBegin()
    {
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        transform.position = originalPos;
        goal.transform.position = new Vector3(Random.Range(originalGoalPos.x - 2, originalGoalPos.x + 5), goal.transform.position.y);
        elapsedTime = Time.realtimeSinceStartup;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVecObs)
        {

            float maxDistance = 10;
            var localVelocity = transform.InverseTransformDirection(rb.velocity);
            var velocity = rb.velocity.magnitude;

            sensor.AddObservation(gameObject.transform.rotation.y);
            sensor.AddObservation(gameObject.transform.rotation.x);
            sensor.AddObservation(gameObject.transform.position.y);
            sensor.AddObservation(gameObject.transform.position.x);
            // sensor.AddObservation(velocity);
            sensor.AddObservation(localVelocity.x);
            sensor.AddObservation(localVelocity.y);

            // sensor.AddObservation(goal.transform.position);

            AddLookObservation(sensor, transform.up, maxDistance, velocity); // 4 observations
            AddLookObservation(sensor, -transform.up, maxDistance, velocity); // 4 observations
            AddLookObservation(sensor, transform.right, maxDistance, velocity); // 4 observations
            AddLookObservation(sensor, -transform.right, maxDistance, velocity); // 4 observations



        }
    }

    void DirectionalObservations(VectorSensor sensor, Vector3 direction)
    {
        if (direction.x > 0)
        {
            sensor.AddObservation(1);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else if (direction.x < 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(1);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        else if (direction.y > 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(1);
            sensor.AddObservation(0);
        }
        else if (direction.y < 0)
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(1);
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
    }

    void AddHitObservation(VectorSensor sensor, float velocity, GameObject other, float hitDistance)
    {
        if (velocity > 0)
        {
            sensor.AddObservation(hitDistance / velocity); // time it may take to collide with static object

        }
        else
        {
            sensor.AddObservation(maxVal);
        }

        // var movingObject = other.GetComponent<Oscillator>();

        // if (movingObject)
        // {
        //     sensor.AddObservation(movingObject.speed);
        // }
        // else
        // {
        //     sensor.AddObservation(0); // other object is static;
        // }
    }

    void AddLookObservation(VectorSensor sensor, Vector3 direction, float distance, float velocity)
    {
        RaycastHit hit;
        DirectionalObservations(sensor, direction);
        Vector3 dir = direction * distance;
        Debug.DrawRay(transform.position, dir, Color.green, 0f, true);

        if (Physics.SphereCast(transform.position, 2f, dir, out hit, distance, 3))
        {
            var other = hit.collider.gameObject;

            if (other.CompareTag("Finish"))
            {
                AddHitObservation(sensor, velocity, other, hit.distance);
                sensor.AddObservation(hit.distance); // if we hit the goal
                sensor.AddObservation(maxVal); // if we didn't hit the obstacle
                sensor.AddObservation(1); // param value denoting object with reward

            }
            else if (other.CompareTag("Obstacle") || other.CompareTag("Ground"))
            {
                AddHitObservation(sensor, velocity, other, hit.distance);
                sensor.AddObservation(maxVal); // if we didn't hit the goal
                sensor.AddObservation(hit.distance);  // if we hit the obstacle
                sensor.AddObservation(-1); // param value denoting obstacle or ground type will destroy us
            }
            else
            {
                // assume no damage for now
                AddHitObservation(sensor, velocity, other, hit.distance);
                sensor.AddObservation(maxVal); // if we didn't hit the goal
                sensor.AddObservation(hit.distance);  // if we hit the obstacle
                sensor.AddObservation(0.1f); // param value denoting other object doesn't destroy us
            }

        }
        else
        {
            // sensor.AddObservation(0); // no speed when nothing is detected
            sensor.AddObservation(maxVal); // if we didn't hit the goal
            sensor.AddObservation(maxVal); // if we didn't hit the obstacle
            sensor.AddObservation(maxVal);
            sensor.AddObservation(0); // param value denoting not object hit
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActions = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActions[0] = 1f;
        }

        else if (Input.GetKey(KeyCode.A))
        {
            continuousActions[0] = -1f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 1;
        }
    }

    void AIProcessThrust(int descreteAction)
    {
        if (descreteAction > 0)
        {
            rb.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

            if (!mainBoosterEffect.isPlaying)
            {
                mainBoosterEffect.Play();
            }

            if (!audioSource.isPlaying)
            {
                // audioSource.PlayOneShot(mainEngine);
            }
        }
        else
        {
            audioSource.Stop();
            mainBoosterEffect.Stop();
        }
    }

    void ProcessThrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

            if (!mainBoosterEffect.isPlaying)
            {
                mainBoosterEffect.Play();
            }

            if (!audioSource.isPlaying)
            {
                // audioSource.PlayOneShot(mainEngine);
            }
        }
        else
        {
            audioSource.Stop();
            mainBoosterEffect.Stop();
        }
    }

    void AIProcessRotation(float direction)
    {
        if (direction > 0)
        {
            applyRotation(-rotationThrust);

            if (!leftBoosterEffect.isPlaying)
            {
                rightBoosterEffect.Stop();
                leftBoosterEffect.Play();
            }

        }

        else if (direction < 0)
        {
            if (!rightBoosterEffect.isPlaying)
            {
                leftBoosterEffect.Stop();
                rightBoosterEffect.Play();
            }

            applyRotation(rotationThrust);
        }
        else
        {
            leftBoosterEffect.Stop();
            rightBoosterEffect.Stop();
        }
    }

    void ProcessRotation()
    {

        if (Input.GetKey(KeyCode.D))
        {
            applyRotation(-rotationThrust);

            if (!leftBoosterEffect.isPlaying)
            {
                rightBoosterEffect.Stop();
                leftBoosterEffect.Play();
            }

        }

        else if (Input.GetKey(KeyCode.A))
        {
            if (!rightBoosterEffect.isPlaying)
            {
                leftBoosterEffect.Stop();
                rightBoosterEffect.Play();
            }

            applyRotation(rotationThrust);
        }
        else
        {
            leftBoosterEffect.Stop();
            rightBoosterEffect.Stop();
        }
    }

    void applyRotation(float rotation)
    {
        rb.freezeRotation = true; // manually handle rotate
        transform.Rotate(Vector3.forward * rotation * Time.deltaTime);
        rb.freezeRotation = false; // restore physics system
    }
}
