using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	// References.
	[SerializeField] private CharacterController2D controller;

	// Fields.
	public float moveSpeed = 30f;

	float horizontalMove;
	bool jump = false;
	bool crouch = false;

	private void Awake()
	{
		controller = GetComponent<CharacterController2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

		if (Input.GetKeyDown(KeyCode.Space))
			jump = true;

		if (Input.GetButtonDown("Crouch"))
			crouch = true;
		else if (Input.GetButtonUp("Crouch"))
			crouch = false;
	}

	private void FixedUpdate()
	{
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;
	}

	public void OnLanding()
	{
		Debug.Log("Landed.");
	}

	public void OnCrouching(bool isCrouching)
	{
		Debug.Log("Crouching: " + isCrouching);
	}
}
