using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTimerHazard : MonoBehaviour
{
    public AudioSource sound;
    public Transform player;
    public ParticleSystem particles, entryparticles;
    public BoxCollider2D collider;
    public float inittime, timeon, timeoff, fadein, fadeout;
    public float entrytime;
    public Vector4 startBox, endBox;
    public float minDistance, maxDistance, nullDistance;

    public bool disablecollider, mute_if_invisivle;

    private bool start = false;
    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        particles.Stop();
    }

    private void Start()
    {
        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;
        StartCoroutine(Init());
    }

    void Update()
    {
        Vector3 screen_pos = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        float distance = Vector2.Distance(transform.position, player.position);
        if(distance > nullDistance || (mute_if_invisivle && !(screen_pos.x >= -0.05 && screen_pos.x <= 1.05 && screen_pos.y >= -0.05 && screen_pos.y <= 1.05)))
        {
            sound.Stop(); sound.volume = 0;
        }
        else if (distance > maxDistance)
        {
            sound.volume = 0;
        }
        else if(distance < minDistance)
        {
            sound.volume = gamemanager.sfx_volume;
        }
        else
        {
            sound.volume = gamemanager.sfx_volume * (1 - (distance - minDistance) / (maxDistance - minDistance));
        }

        if(start)
        {
            StopAllCoroutines();
            StartCoroutine(Sequence());
            start = false;
        }
    }

    IEnumerator Init()
    {
        float time = 0;
        while (time < inittime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        start = true;
    }

    IEnumerator Sequence()
    {
        sound.Stop();
        entryparticles.Stop();
        particles.Stop();
        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;

        float time = 0;
        while(time < timeoff)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if(entrytime > 0)
        {
            time = 0;
            entryparticles.Play();
            while (time < entrytime)
            {
                time += Time.deltaTime;
                yield return null;
            }
        }

        if (Vector2.Distance(transform.position, player.position) < nullDistance)
        {
            sound.Play();
        }

        particles.Play();

        collider.enabled = true;
        time = 0;
        while(time/fadein < 1)
        {
            collider.size = Vector2.Lerp(new Vector2(endBox.x, endBox.y), new Vector2(startBox.x, startBox.y), time);
            collider.offset = Vector2.Lerp(new Vector2(endBox.z, endBox.w), new Vector2(startBox.z, startBox.w), time);
            time += Time.deltaTime;
            yield return null;
        }

        collider.size = new Vector2(startBox.x, startBox.y);
        collider.offset = new Vector2(startBox.z, startBox.w);

        time = 0;
        while (time < timeon)
        {
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time / fadeout < 1)
        {
            collider.size = Vector2.Lerp(new Vector2(startBox.x, startBox.y), new Vector2(endBox.x, endBox.y), time);
            collider.offset = Vector2.Lerp(new Vector2(startBox.z, startBox.w), new Vector2(endBox.z, endBox.w), time);
            time += Time.deltaTime;
            yield return null;
        }

        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;

        start = true;
    }
}
