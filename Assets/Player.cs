﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    // Movement Stuff
    Rigidbody2D rb;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    public bool isGrounded = false;
    public bool gotHit = false;
    public float recoveryTime;

    // Animator Stuff
    private Animator animator;
    private SpriteRenderer sprite;
    [SerializeField] float runAnimationSpeed = 4;

    // Ability stuff
    [SerializeField] bool dashing = false;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDistance;
    [SerializeField] float dashAnimationSpeed;

    void Start () {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }
	
	void Update () {
        MovementControl();
        JumpControl();
        setAnimations();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // Did we hit an enemy's head?
        if (collision.transform.tag == "Enemy_Hit") {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed / 2);
            collision.gameObject.SendMessage("GotHit", this.gameObject);
        }
        else if (collision.transform.tag == "Enemy_Danger") {
            rb.AddRelativeForce(Vector2.left * 1000, ForceMode2D.Force);
            //rb.AddRelativeForce(Vector2.left * 100); // Knockback effect
            StartCoroutine("HitRecovery");
            FindObjectOfType<SoundManager>().Play("PlayerDamage");
            print("Uh oh! You just got hit by the enemy!");
        }
        else { // Probably just on a platform.
            isGrounded = true;
        }
    }

    private void setAnimations() {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

        setSpriteDirection();

        // Set animation speed to something specific if we're dashing.
        if (dashing) {
            animator.speed = dashAnimationSpeed;
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) >= 1) {
            animator.speed = runAnimationSpeed;
            if (isGrounded) {
                FindObjectOfType<SoundManager>().PlayLooping("PlayerRun");
            }
        }
        else {
            animator.speed = 1;
        }
    }

    private void setSpriteDirection() {
        // Check if we are facing left or right, change the sprite direction accordingly.
        if (rb.velocity.x >= 0) {    // Moving Right
            sprite.flipX = false;   // Face Right
        }
        else {                      // Moving Left
            sprite.flipX = true;    // Face Left
        }
    }
    
    private void MovementControl() {
        float h = Input.GetAxis("Horizontal");
        if (!gotHit) {
            rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        }
        else {
            if (rb.velocity.x < 0) { // We're moving left from a bounce
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x + 0.5f, -moveSpeed, 0), rb.velocity.y);
            }
            else if (rb.velocity.x > 0) {
                if (rb.velocity.x < 0) { // We're moving left from a bounce
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x + 0.5f, 0, moveSpeed), rb.velocity.y);
                }
            }
        }
    }

    IEnumerator HitRecovery() {
        gotHit = true;
        yield return new WaitForSeconds(recoveryTime);
        gotHit = false;
    }

    IEnumerator doDash(float h, float waitTime) {
        dashing = true;
        isGrounded = true; // Set this to true so we can dash in the air.
        animator.SetBool("Dashing", dashing);
        rb.velocity = new Vector2(h * moveSpeed * dashSpeed, rb.velocity.y);
        yield return new WaitForSeconds(waitTime);
        dashing = false;
        animator.SetBool("Dashing", dashing);
    }

    private void JumpControl() {
        if (isGrounded && Input.GetButtonDown("Jump")) {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            isGrounded = false;
            FindObjectOfType<SoundManager>().Play("PlayerJump");
        }
    }
}
