using System.Collections;
using UnityEngine;
using Cinemachine;

public class CubeController : PlayerController
{
    public Collider2D player_collider, crouch_collider;

    public TrailRenderer trail;

    public GameObject copter;
    public GameObject jetpack;
    public GameObject ship;
    public GameObject ufo;
    public GameObject wave;
    public GameObject ball;
    public GameObject spider;

    private float jumpForce = 21f;//20f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float time = 0;

    private float maxSpeed = 110f;

    private Vector3 impact_position = Vector3.zero;

    public override void Awake2()
    {
        speed = speed1;
        moveX = 0;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        crouch_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        setAnimation();
    }

    private void Start()
    {
        /*if (!bgmusic.isPlaying)
        {
            bgmusic.Play();
        }*/
    }

    public override void setAnimation()
    {
        player_body.freezeRotation = true;
        player_body.gravityScale = 9.81f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        grounded_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);

        grounded_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ground_impact_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        ChangeSize();

        icon.transform.localScale = new Vector3(1f, 1f, 1f);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        copter.SetActive(false);
        jetpack.SetActive(false);
        ship.SetActive(false);
        ufo.SetActive(false);
        ball.SetActive(false);
        spider.SetActive(false);
        wave.SetActive(false);
        icon.SetActive(true);

        trail.transform.localPosition = new Vector3(0, 0, 0);

        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");

        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
    }

    public override void ChangeSize()
    {
        int rev = reversed ? -1 : 1;
        bool currMini = transform.localScale.x < .5;

        if (mini)
        {
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);
            transform.localScale = new Vector2(.47f, .47f);
            transform.position = transform.position - new Vector3(0, (currMini ? 0 : 1 ) * rev * .29f, 0);
            jumpForce = 16.5f;
        }
        else
        {
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);
            transform.localScale = new Vector2(1.05f, 1.05f);
            transform.position = transform.position + new Vector3(0, (!currMini ? 0 : 1) * rev * .29f, 0);
            jumpForce = 21f;
        }

        posJump = jumpForce;
        if (reversed) { jumpForce *= -1; }
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = Physics2D.IsTouchingLayers(player_collider, deathLayer) || Physics2D.IsTouchingLayers(crouch_collider, deathLayer);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.up, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
                regate = -1;
                grounded_particles.gravityModifier = -Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = -Mathf.Abs(ground_impact_particles.gravityModifier);
            }
            else
            {//.9
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.down, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
                regate = 1;
                grounded_particles.gravityModifier = Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = Mathf.Abs(ground_impact_particles.gravityModifier);
            }
            
            //Debug.Log("Grounded: " + grounded);

            // IF GROUNDED --> TURN OFF TRAIL
            if (grounded)
            {
                trail.emitting = false;
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);

                if (!prev_grounded && prev_velocity > 13)
                {
                    Cube_Anim.ResetTrigger("Crouch");
                    Cube_Anim.ResetTrigger("Default");
                    Cube_Anim.ResetTrigger("Stretch");
                    Cube_Anim.SetTrigger("Squash");
                }
            }

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

            // Grounded Particles
            if(grounded && (Mathf.Abs(player_body.velocity.x) > .1f || jump))
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

            if ((prev_grounded && !grounded) || (!prev_grounded && grounded && prev_velocity > 10f))
            {
                ground_impact_particles.Play();
            }

            // JUMP!
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                jump = true;
                released = false;
                fromGround = ((grounded || time < .07f) && jump);

                if (!reversed && player_body.velocity.y <= 1)
                {
                    downjump = true;
                }
                else if (reversed && player_body.velocity.y >= -1)
                {
                    downjump = true;
                }
                else
                {
                    downjump = false;
                }
            }

            // RELEASE JUMP
            if (Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                jump = false;
                released = true;
            }

            float hitDist = mini ? 0 : .6f;
            if (!reversed)
            {
                headHit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - .2f), Vector2.up, hitDist, groundLayer);
            }
            else
            {
                headHit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + .2f), -Vector2.up, hitDist, groundLayer);
            }
            int rev = reversed ? -1 : 1;
            Debug.DrawLine(transform.position - new Vector3(-1, rev * .2f, 0), transform.position + new Vector3(1, rev * hitDist, 0), Color.red);
            //Debug.Log("headHit: " + headHit.distance);

            // CROUCH
            if (Input.GetAxisRaw("Vertical") < 0 || Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1) || headHit.distance > 0)
            {
                crouch = true;
            }
            else
            {
                crouch = false;
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

            prev_grounded = grounded;
            prev_velocity = Mathf.Abs(player_body.velocity.y);

            time += Time.deltaTime;
            if(prev_grounded) { time = 0; }
        }
    }

    void FixedUpdate()
    {
        if(able)
        {
            Move();
            Interpolate(-1, -1);
        }
    }

    public override void Move()
    {
        // If the input is moving the player right and the player is facing left...
        if ((!reversed && moveX > 0 && !facingright) || (reversed && moveX < 0 && !facingright && (fromGround || grounded)))
        {
            // ... flip the player.
            negate = 1;
            facingright = !facingright;
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if ((!reversed && moveX < 0 && facingright) || (reversed && moveX > 0 && facingright && (fromGround || grounded)))
        {
            // ... flip the player.
            negate = -1;
            facingright = !facingright;
        }

        // if crouching, change movement controls
        if (headHit.distance > 0)
        {
            crouch_collider.enabled = true;
            player_collider.enabled = false;

            Cube_Anim.SetTrigger("Crouch");

            crouched = true;

            if (!grounded)
            {
                Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, 0);
            }
        }
        else if (crouch && grounded)
        {
            if (!crouched)
            {
                Cube_Anim.ResetTrigger("Default");
                Cube_Anim.ResetTrigger("Squash");
                Cube_Anim.ResetTrigger("Stretch");
                Cube_Anim.SetTrigger("Crouch");
            }

            moveX = 0;
            crouch_collider.enabled = true;
            player_collider.enabled = false;
            Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);

            if (!crouched)
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity * 1.6f, targetVelocity, ref v_Velocity, smoothing * 7f);
                crouched = true;
            }
            else
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 7f);
            }

            //crouched = true;
            //player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 7);
            //speed 55      *7      *1      *1.5
        }
        else
        {
            if (crouched)
            {
                crouched = false;
                Cube_Anim.ResetTrigger("Crouch");
                Cube_Anim.ResetTrigger("Squash");
                Cube_Anim.ResetTrigger("Stretch");
                Cube_Anim.SetTrigger("Default");
            }

            player_collider.enabled = true;
            crouch_collider.enabled = false;

            Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);

            if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * .7f); // .9
            }
            else
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.1f); //1.1
            }
        }

        Eyes();
        Jump();     // check if jumping
        Pad();      // check if hit pad
        Portal();   // check if on portal
        Rotate();
    }

    private bool launched = false;
    public void Rotate()
    {
        bool touchingGround = (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
        //Debug.Log("Rotation: " + player_body.rotation);

        if(grounded)
            launched = false;

        if (touchingGround)
        {
            player_body.rotation = reversed ? 180 : 0;
            //transform.rotation = Quaternion.Euler(0, 0, reversed ? 180 : 0);
        }
        else if (!grounded && launched)
        {
            float step = 12.5f;
            int rev = reversed ? -1 : 1;

            if(touchingGround)
                launched = false;

            if (Mathf.Abs(player_body.velocity.x) < 2)
            {
                player_body.rotation = Mathf.Lerp(player_body.rotation, reversed ? 180 : 0, .04f);
            }
            else
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step * rev, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step * rev, .8f);
                }
            }

            /*if (Mathf.Abs(player_body.velocity.x) < 2)
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step * rev * player_body.velocity.y / 20f, .2f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step * rev * player_body.velocity.y / 20f, .2f);
                }
            }
            else
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step * rev, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step * rev, .8f);
                }
            }*/
        }
        else if(!grounded && !launched)
        {
            float step = 12.5f;
            if(reversed && player_body.rotation != 180)
            {
                if(player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step, .8f);
                }
            }
            else if (!reversed && player_body.rotation != 0)
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step, .8f);
                }
            }
            //player_body.rotation = Mathf.Lerp(player_body.rotation, reversed ? 180 : 0, .5f);
        }
    }

    public void Eyes()
    {
        int rev = 1;
        if (reversed) { rev = -1; }
        eyes.transform.localPosition = Vector3.Lerp(eyes.transform.localPosition, new Vector3(rev * (moveX/800), 0 * rev * (player_body.velocity.y/200), 0), .4f);
    }

    public override void Jump()
    {
        OrbComponent orbscript = new OrbComponent();
        if (OrbTouched != null) { orbscript = OrbTouched.GetComponent<OrbComponent>(); }

        if (teleorb && jump)
        {
            Vector3 positionDelta = (transform.position + teleOrb_translate) - transform.position;
            jump = false;
            teleorb = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            player_body.transform.position += teleOrb_translate;

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(activeCamera.Follow, positionDelta);
        }

        if(triggerorb && jump)
        {
            triggerorb = false;
            SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
            StartCoroutine(spawn.Begin());

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        
        if (yellow && jump)
        {
            launched = true;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            yellow = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.1f);
            trail.emitting = true;
            //StartCoroutine(RotateArc(Vector3.forward, negate * -30.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if(OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (pink && jump)
        {
            launched = true;
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            pink = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .95f);
            //StartCoroutine(RotateArc(Vector3.forward, negate * -25.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (red && jump)
        {
            launched = true;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            red = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.45f);

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (blue && jump)
        {
            launched = false;
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            playGravityParticles();

            fromGround = false;
            released = false;
            jump = false;
            blue = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            grounded = false;
            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.4f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (green && jump)
        {
            launched = false;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);

            playGravityParticles();

            fromGround = false;
            released = false;
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
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * 180.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (black && jump)
        {
            launched = false;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            black = false;
            released = false;
            jump = true;
            downjump = true;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, -jumpForce * 1.1f);

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if ((grounded || time < .07f) && jump && downjump)
        {
            launched = false;
            time = 1;
            Cube_Anim.ResetTrigger("Default");
            Cube_Anim.ResetTrigger("Squash");
            Cube_Anim.SetTrigger("Stretch");
            trail.emitting = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);
            grounded = false;

            /* fuck me
            if (Mathf.Abs(moveX) > 0)
            {
                checkGrounded = false;
                StartCoroutine(RotateArc(Vector3.forward, negate * -10.0f, 0.2f));
                checkGrounded = true;
            }//*/

            fromGround = true;
            jump = false;
            downjump = false;
        }
        else if (fromGround && ((!reversed && released && player_body.velocity.y > 0) || (reversed && released && player_body.velocity.y < 0)))
        {
            Cube_Anim.ResetTrigger("Crouch");
            Cube_Anim.ResetTrigger("Squash");
            Cube_Anim.ResetTrigger("Stretch");
            Cube_Anim.SetTrigger("Default");
            player_body.velocity /= 2;
            released = false;
            fromGround = false;
        }
    }

    public override void Pad()
    {
        if (yellow_p)
        {
            launched = true;
            yellow_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.4f);

            checkGrounded = true;
        }
        else if (pink_p)
        {
            launched = true;
            pink_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .9f);

            checkGrounded = true;
        }
        else if (red_p)
        {
            launched = true;
            red_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.6f);
            grounded = false;

            checkGrounded = true;
        }
        else if (blue_p)
        {
            launched = false;
            blue_p = false;
            checkGrounded = false;
            grounded = false;
            fromGround = false;
            released = false;

            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.4f));
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
                fromGround = false;
                released = false;

                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/
                if (player_body.velocity.y <= -15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -15f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
            }
        }
        else if (gravN)
        {
            gravN = false;

            if (reversed)
            {
                fromGround = false;
                released = false;

                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }*/
                if (player_body.velocity.y >= 15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 15f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
            }
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
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }*/
                if (player_body.velocity.y >= 15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 15f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
            }
            else if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/
                if (player_body.velocity.y <= -15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -15f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
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
    private IEnumerator RotateAround(Vector3 axis, float angle, float duration)
    {
        float elapsed = 0.0f;
        float rotated = 0.0f;
        while (elapsed < duration)
        {
            float step = angle / duration * Time.deltaTime;
            transform.RotateAround(transform.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;

            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.up, .59f, groundLayer) && checkGrounded;
            }
            else
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.down, .59f, groundLayer) && checkGrounded;
            }

            if (grounded)
            {
                transform.RotateAround(transform.position, axis, angle - rotated);
                yield break;
            }

            yield return null;
            //yield return new WaitForFixedUpdate();
        }
        transform.RotateAround(transform.position, axis, angle - rotated);
    }

    private IEnumerator RotateArc(Vector3 axis, float angle, float duration)
    {
        float elapsed = 0.0f;
        float rotated = 0.0f;
        duration /= 2;

        while (elapsed < duration)
        {
            float step = angle / duration * Time.deltaTime;
            transform.RotateAround(transform.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;


            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.up, .59f, groundLayer) && checkGrounded;
            }
            else
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.down, .59f, groundLayer) && checkGrounded;
            }

            if (grounded)
            {
                transform.RotateAround(transform.position, axis, -rotated);
                yield break;
            }

            yield return null;
            //yield return new WaitForFixedUpdate();
        }
        transform.RotateAround(transform.position, axis, angle - rotated);

        angle *= -1;
        elapsed = 0.0f;
        rotated = 0.0f;

        while (elapsed < duration)
        {
            float step = angle / duration * Time.deltaTime;
            transform.RotateAround(transform.position, axis, step);
            elapsed += Time.deltaTime;
            rotated += step;

            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.up, 1f, groundLayer) && checkGrounded;
            }
            else
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(.9f, .1f), 0f, Vector2.down, 1f, groundLayer) && checkGrounded;
            }

            if (grounded)
            {
                transform.RotateAround(transform.position, axis, angle - rotated);
                yield break;
            }

            yield return null;
            //yield return new WaitForFixedUpdate();
        }
        transform.RotateAround(transform.position, axis, angle - rotated);
    }

    public override void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingright = !facingright;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        if (restartmusic) { bgmusic.Stop(); }

        grounded_particles.Stop();
        ground_impact_particles.Stop();

        crouch_collider.enabled = false;
        player_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;
        trail.emitting = false;
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

        //player_renderer.enabled = false;
        player_renderer.SetActive(false);
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, gamemanager.sfx_volume);
        player_body.gravityScale = 0;

        Invoke("undead", 1);
    }

    public void undead()
    {
        speed = respawn_speed;
        Vector3 positionDelta = respawn - transform.position;
        transform.position = respawn;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(activeCamera.Follow, positionDelta);

        crouch = false;
        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
        Cube_Anim.transform.localPosition = new Vector3(0, 0, 0);

        player_renderer.SetActive(true);
        player_collider.enabled = true;
        player_body.gravityScale = grav_scale;

        //bgmusic.volume = 1;
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
        reversed = false;  yellow = false; pink = false; red = false; green = false; blue = false; black = false; teleA = false;
    }

    public override void setRepawnSpeed(float s)
    {
        if(s == 0) { respawn_speed = speed0; }
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
        player_collider.enabled = false;
        crouch_collider.enabled = false;

        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
    }

    public override void setColliders()
    {
        player_collider.enabled = true;
        crouch_collider.enabled = false;
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }
    public override string getMode()
    {
        return "cube";
    }
}