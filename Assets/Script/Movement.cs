using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float rotationThrust = 1f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] ParticleSystem mainBoosterEffect;
    [SerializeField] ParticleSystem leftBoosterEffect;
    [SerializeField] ParticleSystem rightBoosterEffect;

    Rigidbody rb;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessThrust();
        ProcessRotation();
    }

    void ProcessThrust()
    {
       if (Input.GetKey(KeyCode.Space))
       {
           rb.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);

           if (!mainBoosterEffect.isPlaying) {
               mainBoosterEffect.Play();
           }

           if (!audioSource.isPlaying)
           {
               audioSource.PlayOneShot(mainEngine);
           }
       } else {
           audioSource.Stop();
           mainBoosterEffect.Stop();
       }
    }

    void ProcessRotation()
    {
       
       if (Input.GetKey(KeyCode.D))
       {
           applyRotation(-rotationThrust);

           if(!leftBoosterEffect.isPlaying)
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
       else {
           leftBoosterEffect.Stop();
           rightBoosterEffect.Stop();
       }
    }

    void applyRotation(float rotation) {
        rb.freezeRotation = true; // manually handle rotate
        transform.Rotate(Vector3.forward * rotation * Time.deltaTime);
        rb.freezeRotation = false; // restore physics system
    }
}
