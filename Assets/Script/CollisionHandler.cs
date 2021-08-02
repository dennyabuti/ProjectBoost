using UnityEngine;
using UnityEngine.SceneManagement;
public class CollisionHandler : MonoBehaviour
{
    float sceneLoadDelay = 2f;
    [SerializeField] AudioClip crashAudio;
    [SerializeField] AudioClip successAudio;
    [SerializeField] ParticleSystem crashEffect;
    [SerializeField] ParticleSystem successEffect;

    AudioSource audioSource;

    bool isInTransition = false;
    bool disableCollision = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        DebugHandler();
    }

    void OnCollisionEnter(Collision other) {
        if (isInTransition || disableCollision)
        {
            return;
        }

        switch (other.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("Friendly Object");
                break;
            case "Fuel":
                Debug.Log("Fuel Object");
                break;
            case "Finish":
                startSuccessSequence();
                break;
            default:
                
                StartCrushSequence();
                break;
        }
    }

    void StartCrushSequence() {
        isInTransition = true;
        audioSource.Stop();
        crashEffect.Play();
        audioSource.PlayOneShot(crashAudio);
        GetComponent<Movement>().enabled = false;
        Invoke("ReloadLevel", sceneLoadDelay);
    }

    void startSuccessSequence() {
        isInTransition = true;
        audioSource.Stop();
        successEffect.Play();
        audioSource.PlayOneShot(successAudio);
        GetComponent<Movement>().enabled = false;
        Invoke("LoadNextLevel", sceneLoadDelay);
    }

    void ReloadLevel()
    {   
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(currentSceneIndex);
    }

    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings) {
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
}
