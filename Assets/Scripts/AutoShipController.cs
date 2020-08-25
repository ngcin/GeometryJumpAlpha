﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AutoShipController : PlayerController
{
    //  reference velocity
    private Vector3 v_Velocity;

    private Rigidbody2D player_body;
    public Collider2D player_collider;
    public CapsuleCollider2D ship_collider;

    private AudioSource bgmusic;

    public LayerMask groundLayer;
    public LayerMask deathLayer;
    public TrailRenderer trail;

    public ParticleSystem death_particles;
    public ParticleSystem flame_stream, flame_spray;

    public GameObject player_renderer;
    public Transform HEIGHT;
    private GameObject eyes;
    private GameObject icon;
    public GameObject ship;

    private float jumpForce = 10f;
    private float speed, speed0 = 40f, speed1 = 55f, speed2 = 75f, speed3 = 90f, speed4 = 110f, respawn_speed;
    private float posJump, negate = 1, regate = 1;

    private float moveX, grav_scale;
    private float smoothing;

    private bool grounded = false, reversed = false, jump = false, checkGrounded = true, 
                    yellow_j = false, red_j = false, blue_j = false, pink_j = false, green_j = false, black_j = false;

    private bool yellow = false, blue = false, red = false, pink = false, green = false, black = false;
    private bool yellow_p = false, blue_p = false, red_p = false, pink_p = false;
    private bool grav = false, gravN = false, teleA = false;
    private Vector3 teleB, respawn;
    private bool respawn_rev = false, respawn_mini = false;
    private bool crouch = false, upright = true, dead = false, check_death = false, able = true;

    private bool isjumping = false;
    private float time;

    private float maxSpeed = 12f;

    private bool mini = false;

    void Awake()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_body = GetComponent<Rigidbody2D>();
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        ship_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        eyes = GameObject.Find("Icon_Eyes");
        //icon = eyes.transform.parent.gameObject;
        //setAnimation();
    }

    private void Start()
    {
        //bgmusic.Play();
        //player_body.freezeRotation = false;
    }

    public override void setAnimation()
    {
        player_renderer.SetActive(true);
        flame_stream.Play();

        player_body.freezeRotation = true;
        transform.rotation = new Quaternion(0, 0, 0, 0);
        upright = true;

        player_body.gravityScale = 3.3f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        icon.transform.localScale = new Vector3(.65f, .65f, 1f);
        icon.transform.localPosition = new Vector3(.08f, .17f, 0);
        ship.SetActive(true);
        icon.SetActive(true);

        trail.transform.localPosition = new Vector3(-.72f, -.23f, 0);

        HEIGHT.GetComponent<Animator>().ResetTrigger("Crouch");
        HEIGHT.GetComponent<Animator>().ResetTrigger("Squash");
        HEIGHT.GetComponent<Animator>().ResetTrigger("Stretch");
        HEIGHT.GetComponent<Animator>().SetTrigger("Default");

        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
    }

    void ChangeSize()
    {
        if (mini)
        {
            transform.localScale = new Vector2(.47f, .47f);
            jumpForce = 8f;
        }
        else
        {
            transform.localScale = new Vector2(1f, 1f);
            jumpForce = 12.5f;
        }

        posJump = jumpForce;
    }

    public override void setIcons(GameObject i)
    {
        icon = i;
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = check_death && (Physics2D.IsTouchingLayers(ship_collider, deathLayer) || Mathf.Abs(player_body.velocity.x) <= 2);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            grounded = checkGrounded && Physics2D.IsTouchingLayers(ship_collider, groundLayer);
            
            if (reversed)
            {
                regate = -1;
            }
            else
            {//.9
                regate = 1;
            }

            // IF GROUNDED --> TURN OFF TRAIL
            /*
            if (grounded && (!red_p && !yellow_p && !blue_p && !pink_p))
            {
                trail.emitting = false;
                animator.SetBool("Jump", false);
                animator.SetBool("Orb", false);
            }*/

            // LIMIT Y SPEED
            if (player_body.velocity.y > maxSpeed)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, maxSpeed);
            }
            else if (player_body.velocity.y < -maxSpeed)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, -maxSpeed);
            }


            // Movement Speed
            moveX = speed;

            // JUMP!
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown("space"))
            {
                if (!grounded || yellow || pink || red || green || blue || black)
                {
                    isjumping = true;
                }
                if (yellow) { yellow_j = true; }
                if (red) { red_j = true;}
                if (pink) { pink_j = true; }
                if (blue) { blue_j = true; }
                if (green) { green_j = true; }
                if (black) { black_j = true;}

                jump = true;
                flame_spray.Play();
            }

            // RELEASE JUMP
            if (Input.GetButtonUp("Jump") || Input.GetKeyUp("space"))
            {
                isjumping = false;
                jump = false;

                flame_spray.Stop();
            }

            // CHANGE JUMP DIRECTION WHEN REVERSED
            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }

            // IF DEAD --> RESPAWN
            if (dead)
            {
                //dead = false;
                Respawn();
            }
        }
    }

    void FixedUpdate()
    {
        // one job and one job only. MOVE
        if (able)
        {
            Move();
        }
    }

    public override void Move()
    {
        // If the input is moving the player right and the player is facing left...
        if (!reversed && !upright)
        {
            // ... flip the player.
            negate = 1;
            upright = !upright;
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if(reversed && upright)
        {
            // ... flip the player.
            negate = -1;
            upright = !upright;
            Flip();
        }

        // movement controls
        Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        player_body.velocity = targetVelocity;

        /*
        if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * .7f);
        }
        else
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.5f);
        }*/

        Rotate();
        Eyes();
        Jump();     // check if jumping
        Pad();      // check if hit pad
        Portal();   // check if on portal

        // IF GROUNDED --> TURN OFF TRAIL
        if (grounded && Mathf.Abs(player_body.velocity.y) <= 0.01 && (!red_p && !yellow_p && !blue_p && !pink_p))
        {
            trail.emitting = false;
        }

        check_death = true;
    }

    //private bool resetRotation = true;

    public void Rotate()
    {
        //Debug.Log(Mathf.Abs(transform.rotation.eulerAngles.z % 90) <= .001f);

        if (grounded)
        {
            player_body.freezeRotation = true;
            transform.rotation = new Quaternion(0, 0, 0, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, 0, 0, 0), .5f);
        }
        else if (player_body.velocity.y >= 0)
        {
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, player_body.velocity.y / .25f);
            Vector3 newAngle = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan(player_body.velocity.y / player_body.velocity.x));
            transform.rotation = Quaternion.Euler(newAngle);
        }
        else
        {
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, 360 + (player_body.velocity.y / .25f));
            Vector3 newAngle = new Vector3(0, 0, 360 + (Mathf.Rad2Deg * Mathf.Atan(player_body.velocity.y / player_body.velocity.x)));
            transform.rotation = Quaternion.Euler(newAngle);
        }
    }

    public void Eyes()
    {
        int rev = 1;
        //if (reversed) { rev = -1; }
        eyes.transform.localPosition = Vector3.Lerp(eyes.transform.localPosition, new Vector3(rev * (moveX / 800), 0 * rev * (player_body.velocity.y / 80), 0), .4f);

        if (!grounded)
        {
            return;
        }
        else
        {
            if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 0)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 1)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 2)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 3)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(true);
            }
        }
    }

    public override void Jump()
    {
        trail.emitting = true;

        if(maxSpeed != 12)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, 12, time);
            time += 1f * Time.deltaTime;

            if (time > 1.0f)
            {
                time = 0.0f;
            }
        }

        if (yellow_j)
        {
            yellow = false;
            yellow_j = false;
            //yellow_count++;
            //if (yellow_count >= 0) { yellow_j = false; yellow_count = 0; }

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.3f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.3f);
            time = 0;
        }
        else if (red_j)
        {
            red = false;
            red_j = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.65f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.65f);
            time = 0;
        }
        else if (pink_j)
        {
            pink = false;
            pink_j = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = 12f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .5f);
            time = 0;
        }
        else if (blue_j)
        {
            blue = false;
            blue_j = false;
            //yellow_count++;
            //if (yellow_count >= 0) { green_j = false; yellow_count = 0; }

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            reversed = !reversed;

            maxSpeed = 12f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .7f);

            player_body.gravityScale *= -1;
            grav_scale *= -1;
            time = 0;
        }
        else if (green_j)
        {
            green = false;
            green_j = false;
            //yellow_count++;
            //if (yellow_count >= 0) { green_j = false; yellow_count = 0; }

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            reversed = !reversed;

            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }

            maxSpeed = Mathf.Abs(jumpForce) * 1.3f;

            player_body.gravityScale *= -1;
            grav_scale *= -1;

            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.3f);
            time = 0;
        }
        else if (black_j)
        {
            black = false;
            black_j = false;
            //black_count++;
            //if (black_count >= 5) { black_j = false; black_count = 0; }

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 2.4f;
            player_body.velocity = new Vector2(player_body.velocity.x, 0f);
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * -2.4f);
            time = 0;
        }

        if (jump)
        {
            //player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y + 3f);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
            player_body.AddForce(new Vector2(0, 23f * grav_scale));
        }
    }

    public override bool isJumping()
    {
        return isjumping;
    }

    public override void setIsJumping(bool j)
    {
        isjumping = j;
    }

    public override void Pad()
    {
        if(maxSpeed != 12)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, 12, time);
            time += 1f * Time.deltaTime;

            if (time > 1.0f)
            {
                time = 0.0f;
            }
        }

        if (yellow_p)
        {
            yellow_p = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.2f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            time = 0;
        }
        else if (red_p)
        {
            red_p = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.55f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.55f);
            time = 0;
        }
        else if (pink_p)
        {
            pink_p = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = 12f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .5f);
            time = 0;
        }
        else if (blue_p)
        {
            blue_p = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            reversed = !reversed;

            maxSpeed = Mathf.Abs(jumpForce) * 1.2f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);

            player_body.gravityScale *= -1;
            grav_scale *= -1;
            time = 0;
        }
    }

    public override void Portal()
    {
        if (grav)
        {
            grav = false;

            if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (gravN)
        {
            gravN = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }
                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (teleA)
        {
            //trail.emitting = false;
            //trail.Clear();
            trail.enabled = true;
            teleA = false;
            player_body.transform.position += teleB;
            //trail.enabled = true;
        }
    }


    // COROUTUNES
    // none needed

    public override void Flip()
    {
        // Switch the way the player is labelled as facing.
        //facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.y *= -1;
        transform.localScale = theScale;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "PortalGravity")
        {
            grav = true;
        }
        if (collision.gameObject.tag == "PortalGravityN")
        {
            gravN = true;
        }
        if (collision.gameObject.tag == "TeleportA")
        {
            teleA = true;

            Transform t = collision.gameObject.transform;

            Transform tb = t;
            foreach (Transform tr in t)
            {
                if (tr.tag == "TeleportB")
                {
                    tb = tr.GetComponent<Transform>();
                    break;
                }
            }

            //teleB = t.InverseTransformPoint(tb.position);
            teleB = tb.position - t.position;
            //teleB.z = transform.position.z;
            teleB.z = 0;
        }
        if (collision.gameObject.tag == "YellowPad")
        {
            yellow_p = true;
        }
        if (collision.gameObject.tag == "PinkPad")
        {
            pink_p = true;
        }
        if (collision.gameObject.tag == "RedPad")
        {
            red_p = true;
        }
        if (collision.gameObject.tag == "BluePad")
        {
            blue_p = true;
        }
        if (collision.gameObject.tag == "Speed0x")
        {
            speed = speed0;
        }
        if (collision.gameObject.tag == "Speed1x")
        {
            speed = speed1;
        }
        if (collision.gameObject.tag == "Speed2x")
        {
            speed = speed2;
        }
        if (collision.gameObject.tag == "Speed3x")
        {
            speed = speed3;
        }
        if (collision.gameObject.tag == "Speed4x")
        {
            speed = speed4;
        }
        if (collision.gameObject.tag == "BlueOrb")
        {
            blue = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "YellowOrb")
        {
            yellow = true;
        }
        if (collision.gameObject.tag == "PinkOrb")
        {
            pink = true;
        }
        if (collision.gameObject.tag == "RedOrb")
        {
            red = true;
        }
        if (collision.gameObject.tag == "GreenOrb")
        {
            green = true;
        }
        if (collision.gameObject.tag == "BlackOrb")
        {
            black = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "YellowOrb")
        {
            yellow = false;
        }
        if (collision.gameObject.tag == "BlueOrb")
        {
            blue = false;
        }
        if (collision.gameObject.tag == "PinkOrb")
        {
            pink = false;
        }
        if (collision.gameObject.tag == "RedOrb")
        {
            red = false;
        }
        if (collision.gameObject.tag == "GreenOrb")
        {
            green = false;
        }
        if (collision.gameObject.tag == "BlackOrb")
        {
            black = false;
        }
        if (collision.gameObject.tag == "YellowPad")
        {
            yellow_p = false;
        }
        if (collision.gameObject.tag == "PinkPad")
        {
            pink_p = false;
        }
        if (collision.gameObject.tag == "RedPad")
        {
            red_p = false;
        }
        if (collision.gameObject.tag == "BluePad")
        {
            blue_p = false;
        }
    }

    private bool restartmusic = false;
    public override void Respawn()
    {
        able = false;
        check_death = false;
        if (restartmusic) { bgmusic.Stop(); }
        player_collider.enabled = false;
        ship_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;
        trail.emitting = false;
        flame_stream.Stop();
        flame_spray.Stop();
        jump = false;

        yellow = false; pink = false; red = false; green = false; blue = false; black = false;
        reversed = respawn_rev;
        mini = respawn_mini;
        ChangeSize();

        if (reversed)
        {
            player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 180, 0);
        }
        else
        {
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        player_renderer.SetActive(false);
        //player_renderer.enabled = false;
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, 1f);
        player_body.gravityScale = 0;

        Invoke("reposition", 1f);
        //player_body.transform.position += respawn - transform.position;

    }

    public void reposition()
    {
        //player_body.transform.position += respawn - transform.position;
        transform.position = new Vector3(respawn.x, respawn.y, transform.position.z);
        player_collider.enabled = true;
        Invoke("undead", .5f);
    }

    public void undead()
    {
        if (!enabled)
        {
            Debug.Log("KMY SORRY");
            jump = false;
            dead = true;
            player_renderer.SetActive(true);
            speed = respawn_speed;
            resetColliders();
            return;
        }

        player_collider.enabled = false;
        speed = respawn_speed;
        player_collider.enabled = true;
        ship_collider.enabled = true;
        player_renderer.SetActive(true);

        player_body.gravityScale = grav_scale;

        bgmusic.volume = 1;
        if (restartmusic) { bgmusic.Play(); }

        dead = false;
        able = true;
    }

    public override void setRespawn(Vector3 pos, bool rev, bool min)
    {
        respawn = pos;
        respawn_rev = rev;
        respawn_mini = min;
        respawn.z = transform.position.z;
    }

    public override void resetBooleans()
    {
        StopAllCoroutines();
        reversed = false; jump = false; yellow = false; pink = false; red = false; green = false; blue = false; black = false; teleA = false;
    }

    public override void setBGMusic(AudioSource audio)
    {
        bgmusic = audio;
    }

    public override void setRepawnSpeed(float s)
    {
        if (s == 0) { respawn_speed = speed0; }
        else if (s == 1) { respawn_speed = speed1; }
        else if (s == 2) { respawn_speed = speed2; }
        else if (s == 3) { respawn_speed = speed3; }
        else if (s == 4) { respawn_speed = speed4; }
    }

    public override void setSpeed(float s)
    {
        if (s == 0 || s == speed0) { speed = speed0; }
        else if (s == 1 || s == speed1) { speed = speed1; }
        else if (s == 2 || s == speed2) { speed = speed2; }
        else if (s == 3 || s == speed3) { speed = speed3; }
        else if (s == 4 || s == speed4) { speed = speed4; }
    }

    public override float getSpeed()
    {
        return speed;
    }

    public override void resetColliders()
    {
        ship.SetActive(false);
        flame_stream.Stop();
        flame_spray.Stop();

        jump = false;

        player_collider.isTrigger = false;
        player_collider.enabled = false;
        ship_collider.enabled = false;
    }
    public override void setColliders()
    {
        player_collider.enabled = false;

        check_death = false;
        //player_collider.isTrigger = true;
        ship_collider.enabled = true;
    }
    public override void setRestartMusicOnDeath(bool r)
    {
        restartmusic = r;
    }

    public override void setAble(bool a)
    {
        able = a;
    }
    public override void setVariables(bool j, bool r, bool m)
    {
        jump = j;
        reversed = r;
        mini = m;
    }
    public override bool getMini()
    {
        return mini;
    }
    public override bool getReversed()
    {
        return reversed;
    }
    public override bool isDead()
    {
        return dead;
    }
    public override string getMode()
    {
        return "auto_ship";
    }
}