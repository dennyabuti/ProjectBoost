using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CollisionHandler : MonoBehaviour
{
    float sceneLoadDelay = 2f;
    [SerializeField] AudioClip crashAudio;
    [SerializeField] AudioClip successAudio;
    [SerializeField] ParticleSystem crashEffect;
    [SerializeField] ParticleSystem successEffect;
    [SerializeField] Material winMaterial;

    AudioSource audioSource;

    bool isInTransition = false;
    bool disableCollision = false;
    MovementMLAgent movementMLAgent;
    int winCount = 0;
    float winWindow = 0.5f;
    GameObject ground;
    Material currentGroundMaterial;

    void Start()
    {
        var parent = transform.parent;

        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.tag == "Ground")
                {
                    ground = child.gameObject;
                    break;
                }
            }
        }
        else
        {
            ground = GameObject.FindWithTag("Ground");
        }

        currentGroundMaterial = ground.GetComponent<Renderer>().material;
        audioSource = GetComponent<AudioSource>();
        movementMLAgent = GetComponent<MovementMLAgent>();
    }

    void Update()
    {
        DebugHandler();
    }

    void OnCollisionEnter(Collision other)
    {
        // if (isInTransition || disableCollision)
        // {
        //     return;
        // }

        switch (other.gameObject.tag)
        {
            case "Friendly":
                movementMLAgent.AddReward(-2f);
                break;
            case "Fuel":
                break;
            case "Finish":
                startSuccessSequence();
                break;
            default:

                StartCrushSequence();
                break;
        }
    }

    void StartCrushSequence()
    {
        isInTransition = true;
        // audioSource.Stop();
        // crashEffect.Play();
        // audioSource.PlayOneShot(crashAudio);
        // GetComponent<Movement>().enabled = false;

        movementMLAgent.SetReward(-20f);
        movementMLAgent.EndEpisode();
        // Invoke("ReloadLevel", sceneLoadDelay);
    }

    void startSuccessSequence()
    {
        isInTransition = true;
        // audioSource.Stop();
        // successEffect.Play();
        // audioSource.PlayOneShot(successAudio);
        // GetComponent<Movement>().enabled = false;
        // Invoke("LoadNextLevel", sceneLoadDelay);

        // reward agent
        movementMLAgent.SetReward(100f);
        StartCoroutine(VisualizeWin());
        movementMLAgent.EndEpisode();

        // Invoke("ReloadLevel", sceneLoadDelay);
    }

    void ReloadLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // SceneManager.LoadScene(currentSceneIndex);
    }

    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    void DebugHandler()
    {
        if (Input.GetKey(KeyCode.C))
        {
            disableCollision = !disableCollision;
        }
    }

    IEnumerator VisualizeWin()
    {
        var groundRenderer = ground.GetComponent<Renderer>();
        
        groundRenderer.material = winMaterial;

        yield return new WaitForSeconds(winWindow);

        groundRenderer.material = currentGroundMaterial;
    }

}
