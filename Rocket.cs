using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float verticalThrust = 100f;
    [SerializeField] float loadLevelDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip explosion;
    [SerializeField] AudioClip levelComplete;

    [SerializeField] ParticleSystem engineParticle;
    [SerializeField] ParticleSystem explosionParticle;
    [SerializeField] ParticleSystem levelCompleteParticle;

    Rigidbody rigidBody;
    AudioSource audioSource;
    enum State { Alive, Dying, Trancending }
    State state = State.Alive;

    bool collisionsDisabled = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugKey();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionsDisabled) { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Trancending;
        audioSource.Stop();
        levelCompleteParticle.Play();
        audioSource.PlayOneShot(levelComplete);
        Invoke("LoadNextScene", loadLevelDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        explosionParticle.Play();
        audioSource.PlayOneShot(explosion);
        Invoke("LoadFirstLevel", loadLevelDelay);
    }  

    private void LoadFirstLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; // Take manual control of roatation

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A)) // Turn left
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D)) // Turn right
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // Resume physics control of rotation
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space)) // Thrusters
        {
            ActivateThrust();
        }
        else
        {
            audioSource.Stop();
            engineParticle.Stop();
        }
    }

    private void RespondToDebugKey()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
    }

    private void ActivateThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * verticalThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        engineParticle.Play();
    }
}
