using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class TapController : MonoBehaviour
{
    public delegate void PlayerDelegate();
    public static event PlayerDelegate OnPlayerDied;
    public static event PlayerDelegate OnPlayerScored;

    public float tiltSmooth = 5;
    public float tapForce = 10;
    public Vector3 startPos;

    public AudioSource tapAudio;
    public AudioSource scoreAudio;
    public AudioSource dieAudio;

    Rigidbody2D rb;
    Quaternion downRotation;
    Quaternion forwardRotation;

    GameManager game;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -75);
        forwardRotation = Quaternion.Euler(0, 0, 35);
        game = GameManager.instance;
        rb.simulated = false;
    }

    void OnEnable() 
    {
        GameManager.OnGameStarted += OnGameStarted;
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
    }

    void OnDisable()
    {
        GameManager.OnGameStarted -= OnGameStarted;
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
    }

    void OnGameStarted() {
        rb.velocity = Vector3.zero; // set velocity of the bird to zero
        rb.simulated = true;    // set the bird to listen to physics
    }

    void OnGameOverConfirmed() {
        // set the bird back to start position
        transform.localPosition = startPos;
        transform.rotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (game.GameOver) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            tapAudio.Play();
            transform.rotation = forwardRotation;
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector2.up * tapForce, ForceMode2D.Force);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "ScoreZone")
        {
            // register a score event
            OnPlayerScored();   // event sent to GameManager
            // play a sound
            scoreAudio.Play();
        }
        if(col.gameObject.tag == "DeadZone")
        {
            rb.simulated = false;   // set the bird not listen to physics
            // register a dead event
            OnPlayerDied(); // event sent to GameManager
            // play a sound
            dieAudio.Play();
        }
    }
}
