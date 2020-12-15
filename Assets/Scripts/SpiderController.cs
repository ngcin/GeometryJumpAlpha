﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class SpiderController : PlayerController
{
    public Collider2D player_collider;
    public BoxCollider2D spider_collider;
    public TrailRenderer trail, spider_trail;

    public PulseTrigger pulse_trigger_p1, pulse_trigger_p2;

    public Transform Spider_Anim;
    public GameObject spider;

    private float jumpForce = 17f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float maxSpeed = 17 * 1.6f;

    public override void Awake2()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        spider_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        //eyes = GameObject.Find("Icon_Eyes");
        //setAnimation();
    }

    private void Start()
    {
        //bgmusic.Play();
        //player_body.freezeRotation = false;
    }

    public override void setAnimation()
    {
        player_collider.isTrigger = true;
        //upright = true;
        //facingright = player_body.velocity.x >= 0 ? !reversed : reversed;

        player_body.freezeRotation = true;
        player_body.gravityScale = 8f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        Vector3 newAngle = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(newAngle);
        //transform.localScale = new Vector3(1f, 1f, 1f);

        ChangeSize();

        icon.transform.localScale = new Vector3(1f, 1f, 1f);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        spider.SetActive(true);
        icon.SetActive(false);

        spider_trail.gameObject.SetActive(true);
        spider_trail.emitting = false;
        trail.transform.localPosition = new Vector3(0, 0, 0);

        Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
        Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
        Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
        Spider_Anim.GetComponent<Animator>().SetTrigger("stop");
    }
    public override void ChangeSize()
    {
        int rev = reversed ? -1 : 1;
        if (mini)
        {
            transform.localScale = new Vector2(.47f, rev * .47f);
            transform.position = transform.position + -new Vector3(0, rev * .29f, 0);
            jumpForce = 14f;
        }
        else
        {
            transform.localScale = new Vector2(1.05f, rev * 1.05f);
            transform.position = transform.position + new Vector3(0, rev * .29f, 0);
            jumpForce = 17f;
        }

        posJump = jumpForce;
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = /*Physics2D.IsTouchingLayers(player_collider, deathLayer) || */Physics2D.IsTouchingLayers(spider_collider, deathLayer);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.5f, .1f), 0f, Vector2.up, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(spider_collider, groundLayer));
                regate = -1;
            }
            else
            {//.9
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.5f, .1f), 0f, Vector2.down, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(spider_collider, groundLayer));
                regate = 1;
            }

            //facingright = player_body.velocity.x >= 0 ? !reversed : reversed;

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
            moveX = Input.GetAxisRaw("Horizontal") * speed;

            if (moveX > 0) { facingright = true; }
            else if (moveX < 0) { facingright = false; }
            upright = !reversed;

            // JUMP!
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                if (!grounded || yellow || pink || red || green || blue || black)
                {
                    isjumping = true;
                }

                jump = true;
            }

            // RELEASE JUMP
            if (Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                isjumping = false;
                jump = false;
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
                dead = false;
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
       /* if (!reversed && !upright)
        {
            // ... flip the player.
            negate = 1;
            //upright = !upright;
            FlipY();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (reversed && upright)
        {
            // ... flip the player.
            negate = -1;
            upright = !upright;
            FlipY();
        }*/

        /*if (moveX > 0 && !facingright)
        {
            // ... flip the player.
            facingright = true;
            FlipX();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (moveX < 0 && facingright)
        {
            // ... flip the player.
            facingright = false;
            FlipX();
        }*/
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (facingright ? 1 : -1), Mathf.Abs(transform.localScale.y) * (upright ? 1 : -1), transform.localScale.z);


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

        //Rotate();
        //Eyes();
        Pad();      // check if hit pad
        Jump();     // check if jumping
        Portal();   // check if on portal

        // IF GROUNDED --> TURN OFF TRAIL
        if (grounded && Mathf.Abs(player_body.velocity.y) <= 0.01 && (!red_p && !yellow_p && !blue_p && !pink_p && !gravN && !grav))
        {
            spider_trail.emitting = false;
            trail.emitting = false;

            if(moveX == 0)
            {
                Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
                Spider_Anim.GetComponent<Animator>().SetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().Play("stop", -1, 0f);

                spider_trail.textureMode = LineTextureMode.RepeatPerSegment;
                spider_trail.time = .5f;
            }
            else
            {
                Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().SetTrigger("run");
                Spider_Anim.GetComponent<Animator>().speed = (Mathf.Abs(moveX) / 60) * (mini ? 1.5f : 1);

                spider_trail.time = .2f; //.15
                spider_trail.textureMode = LineTextureMode.RepeatPerSegment;
            }
        }

        spider_trail.transform.localPosition = new Vector3(/*Mathf.Abs(moveX) / 200*/0, 0, 0);
    }

    //private bool resetRotation = true;

    public void Rotate()
    {
        float step = -speed / 6f;

        if (reversed)
        {
            step = speed / 6f;
        }

        //Debug.Log(grounded);

        if (grounded)
        {
            Vector3 newAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + step);
            transform.rotation = Quaternion.Euler(newAngle);
        }
        else
        {
            Vector3 newAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - step * .7f);
            transform.rotation = Quaternion.Euler(newAngle);
        }
    }

    /*public void Eyes()
    {
        int rev = 1;
        if (reversed) { rev = -1; }
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
    }*/

    public override void Jump()
    {
        if (yellow && jump)
        {
            jump = false;
            yellow = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            trail.emitting = true;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().Play("jump", -1, 0f);
            Spider_Anim.GetComponent<Animator>().speed = 2;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
        }
        else if (pink && jump)
        {
            jump = false;
            pink = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().Play("jump", -1, 0f);
            Spider_Anim.GetComponent<Animator>().speed = 2;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
        }
        else if (red && jump)
        {
            jump = false;
            red = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.4f);

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().Play("jump", -1, 0f);
            Spider_Anim.GetComponent<Animator>().speed = 2;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
        }
        else if (blue && jump)
        {
            jump = false;
            blue = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            grounded = false;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 2;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
        }
        else if (green && jump)
        {
            jump = false;
            green = false;
            reversed = !reversed;

            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 2;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
        }
        else if (black && jump)
        {
            black = false;
            //jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, -jumpForce * 1.6f);

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 2;
        }
        else if (grounded && jump && (!yellow_p && !red_p && !pink_p && !blue_p))
        {
            jump = false; grounded = false;

            int rev = 1;
            if (reversed) { rev = -1; }
            RaycastHit2D groundhit, deathhit;

            if (!reversed)
            {
                groundhit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 0), Vector2.up, 120, groundLayer);
                deathhit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 0), Vector2.up, 120, deathLayer);
            }
            else
            {
                groundhit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0), -Vector2.up, 120, groundLayer);
                deathhit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0), -Vector2.up, 120, deathLayer);
            }

            //bool head = grounded && Physics2D.BoxCast(player_body.transform.position, new Vector2(.95f, .1f), 0f, rev * Vector2.up, .01f, groundLayer);

            if (deathhit.collider != null)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, 0);
                spider_trail.emitting = true;

                //Debug.Log(deathhit.distance);
                reversed = !reversed;
                player_body.gravityScale *= -1;
                grav_scale *= -1;
                transform.position = new Vector2(transform.position.x, transform.position.y + rev * (deathhit.distance - .5f));
            }
            else if (groundhit.collider != null)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, 0);
                spider_trail.emitting = true;

                Debug.Log(groundhit.distance - .5f);
                reversed = !reversed;
                player_body.gravityScale *= -1;
                grav_scale *= -1;
                transform.position = new Vector2(transform.position.x, transform.position.y + rev * (groundhit.distance - .5f));
            }
            else
            {
                spider_trail.emitting = true;
            }

            pulse_trigger_p1.Enter();
            pulse_trigger_p2.Enter();

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }
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
        if (yellow_p)
        {
            //yellow_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            //animator.SetBool("Orb", true);
            //jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.3f);
            yellow_p = false;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 1;
            spider_trail.emitting = false;

            checkGrounded = true;
        }
        else if (pink_p)
        {
            checkGrounded = false;
            grounded = false;
            jump = false;

            //jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .9f);
            pink_p = false;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 1;
            spider_trail.emitting = false;

            checkGrounded = true;
        }
        else if (red_p)
        {
            checkGrounded = false;
            grounded = false;
            jump = false;

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.6f);
            red_p = false;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 1;
            spider_trail.emitting = false;

            checkGrounded = true;
        }
        else if (blue_p)
        {
            jump = false;
            checkGrounded = false;
            blue_p = false;
            grounded = false;

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
            Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
            Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
            Spider_Anim.GetComponent<Animator>().speed = 1;
            spider_trail.emitting = false;

            checkGrounded = true;
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

                if (player_body.velocity.y <= -10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -10f);
                }

                Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().speed = 1;
                spider_trail.emitting = false;

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }

            trail.emitting = true;
        }
        else if (gravN)
        {
            gravN = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;

                if (player_body.velocity.y >= 10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 10f);
                }

                Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().speed = 1;
                spider_trail.emitting = false;

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }

            trail.emitting = true;
        }
        else if (gravC)
        {
            gravC = false;
            fromGround = false;
            released = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;

                if (player_body.velocity.y >= 10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 10f);
                }

                Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().speed = 1;
                spider_trail.emitting = false;

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
            else if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;

                if (player_body.velocity.y <= -10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -10f);
                }

                Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
                Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
                Spider_Anim.GetComponent<Animator>().SetTrigger("curl");
                Spider_Anim.GetComponent<Animator>().speed = 1;
                spider_trail.emitting = false;

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (teleA)
        {
            Vector3 positionDelta = (transform.position + teleB) - transform.position;
            //trail.emitting = false;
            //trail.Clear();
            spider_trail.emitting = false;
            trail.enabled = true;
            teleA = false;
            player_body.transform.position += teleB;
            //trail.enabled = true;

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(activeCamera.Follow, positionDelta);
        }
    }


    public override void Flip()
    {
        throw new System.NotImplementedException();
    }

    public void FlipY()
    {
        // Switch the way the player is labelled as facing.
        //facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.y *= -1;
        transform.localScale = theScale;
    }

    public void FlipX()
    {
        // Switch the way the player is labelled as facing.
        //facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        if (restartmusic) { bgmusic.Stop(); }
        player_collider.enabled = false;
        spider_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;
        trail.emitting = false;
        spider_trail.emitting = false;
        jump = false;
        yellow = false; pink = false; red = false; green = false; blue = false; black = false;
        reversed = respawn_rev;
        mini = respawn_mini;
        ChangeSize();

        if (reversed)
        {
            player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
        }
        else
        {
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
        }

        player_renderer.SetActive(false);
        //player_renderer.enabled = false;
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, 1f);
        player_body.gravityScale = 0;

        Spider_Anim.GetComponent<Animator>().ResetTrigger("curl");
        Spider_Anim.GetComponent<Animator>().ResetTrigger("jump");
        Spider_Anim.GetComponent<Animator>().ResetTrigger("run");
        Spider_Anim.GetComponent<Animator>().ResetTrigger("stop");
        Spider_Anim.GetComponent<Animator>().SetTrigger("stop");

        Invoke("reposition", 1f);
        //player_body.transform.position += respawn - transform.position;

    }

    public void reposition()
    {
        Vector3 positionDelta = respawn - transform.position;
        player_body.transform.position += respawn - transform.position;
        player_collider.enabled = true;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(activeCamera.Follow, positionDelta);

        undead();

        //Invoke("undead", .5f);
    }

    public void undead()
    {
        player_collider.enabled = false;
        speed = respawn_speed;
        //player_collider.enabled = true;
        spider_collider.enabled = true;
        player_renderer.SetActive(true);
        //player_renderer.enabled = true;
        player_body.gravityScale = grav_scale;

        bgmusic.volume = 1;
        if (restartmusic) { bgmusic.Play(); }

        Vector2 targetVelocity = new Vector2(speed * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        player_body.velocity = targetVelocity;

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
        player_collider.isTrigger = false;
        spider_trail.gameObject.SetActive(false);
        spider.SetActive(false);

        player_collider.enabled = true;
        spider_collider.enabled = false;
    }
    public override void setColliders()
    {
        player_collider.enabled = true;
        player_collider.isTrigger = true;
        spider_collider.enabled = true;
    }

    public override string getMode()
    {
        return "spider";
    }
}