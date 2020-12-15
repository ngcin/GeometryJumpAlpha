﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class AutoShipController : PlayerController
{
    public Collider2D player_collider;
    public CapsuleCollider2D ship_collider;

    public TrailRenderer trail;

    public ParticleSystem flame_stream, flame_spray;
    public GameObject ship;

    private float jumpForce = 10f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;
    private float time;

    private float maxSpeed = 12f;

    public override void Awake2()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        ship_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        //eyes = GameObject.Find("Icon_Eyes");
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
        transform.rotation = Quaternion.Euler(0,0,0);
        upright = true;

        maxSpeed = 12;
        player_body.gravityScale = 3.3f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        grounded_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);

        grounded_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ground_impact_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        ChangeSize();

        icon.transform.localScale = new Vector3(.65f, .65f, 1f);
        icon.transform.localPosition = new Vector3(.08f, .17f, 0);
        ship.SetActive(true);
        icon.SetActive(true);

        trail.transform.localPosition = new Vector3(-.72f, -.23f, 0);

        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
    }

    public override void ChangeSize()
    {
        int rev = reversed ? -1 : 1;
        if (mini)
        {
            //player_body.gravityScale = 3.5f;
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);

            maxSpeed = 15;
            transform.localScale = new Vector2(.47f, rev * .47f);
            jumpForce = 7f;
        }
        else
        {
            //player_body.gravityScale = 3.3f;
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);

            maxSpeed = 12;
            transform.localScale = new Vector2(1.05f, rev * 1.05f);
            jumpForce = 10f;
        }

        posJump = jumpForce;
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

            bool grounded_indirection = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, reversed ? Vector2.up : Vector2.down, .51f, groundLayer);

            if (reversed)
            {
                regate = -1;

                grounded_particles.gravityModifier = -Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = -Mathf.Abs(ground_impact_particles.gravityModifier);
            }
            else
            {//.9
                regate = 1;

                grounded_particles.gravityModifier = Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = Mathf.Abs(ground_impact_particles.gravityModifier);
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

            if (grounded_indirection && (Mathf.Abs(player_body.velocity.x) > .2f || jump))
            {
                if (!grounded_particles.isPlaying)
                {
                    grounded_particles.Play();
                }
            }
            else
            {
                grounded_particles.Stop();
            }

            if ((prev_grounded && !grounded_indirection) || (!prev_grounded && grounded_indirection && prev_velocity > 10f))
            {
                ground_impact_particles.Play();
            }

            // JUMP!
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                if (!grounded || yellow_j || pink_j || red_j || green_j || blue_j || black_j || triggerorb_j || teleorb_j)
                {
                    isjumping = true;
                }
                if (triggerorb) { triggerorb_j = true; }
                if (teleorb) { teleorb_j = true; }
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
            if (Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
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
            //transform.rotation = new Quaternion(0, 0, 0, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0,0,0), .5f);
        }
        else if (player_body.velocity.y >= 0)
        {
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, player_body.velocity.y / .25f);
            Vector3 newAngle = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan(player_body.velocity.y / player_body.velocity.x));
            //transform.rotation = Quaternion.Euler(newAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newAngle), mini ? 1f : .3f);
        }
        else
        {
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, 360 + (player_body.velocity.y / .25f));
            Vector3 newAngle = new Vector3(0, 0, 360 + (Mathf.Rad2Deg * Mathf.Atan(player_body.velocity.y / player_body.velocity.x)));
            //transform.rotation = Quaternion.Euler(newAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newAngle), mini ? 1f : .3f);
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

        if(maxSpeed != (mini ? 15 : 12))
        {
            maxSpeed = Mathf.Lerp(maxSpeed, (mini ? 15 : 12), time);
            time += 1f * Time.deltaTime;

            if (time > 1.0f)
            {
                time = 0.0f;
            }
        }

        if (teleorb_j && jump)
        {
            teleorb_j = false;
            teleorb = false;
            player_body.transform.position += teleOrb_translate;
        }

        if (triggerorb_j && jump)
        {
            triggerorb_j = false;
            triggerorb = false;
            SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
            StartCoroutine(spawn.Begin());
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

            maxSpeed = (mini ? 15 : 12);
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);
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

            maxSpeed = (mini ? 15 : 12);
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
            player_body.AddForce(new Vector2(0, 24f * grav_scale));
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
        if(maxSpeed != (mini ? 15 : 12))
        {
            maxSpeed = Mathf.Lerp(maxSpeed, (mini ? 15 : 12), time);
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

            maxSpeed = (mini ? 15 : 12);
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);
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
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }
                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (gravC)
        {
            gravC = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }
                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
            else if (!reversed)
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
        //facingright = !facingright;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.y *= -1;
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        check_death = false;
        if (restartmusic) { bgmusic.Stop(); }

        grounded_particles.Stop();
        ground_impact_particles.Stop();

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
            //upright = false;
            //transform.rotation = new Quaternion(0, 0, 180, 0);
        }
        else
        {
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            //upright = true;
            //transform.rotation = new Quaternion(0, 0, 0, 0);
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
        Vector3 positionDelta = respawn - transform.position;
        transform.position = respawn;
        player_collider.enabled = true;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(activeCamera.Follow, positionDelta);

        //Invoke("undead", .5f);
        undead();
    }

    public void undead()
    {
        if (!enabled)
        {
            Debug.Log("KMS SORRY");
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
        player_collider.enabled = true;
        ship_collider.enabled = false;
    }
    public override void setColliders()
    {
        player_collider.enabled = false;

        check_death = false;
        //player_collider.isTrigger = true;
        ship_collider.enabled = true;
    }
    public override string getMode()
    {
        return "auto_ship";
    }
}