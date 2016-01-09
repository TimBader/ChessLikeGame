﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

    public float speed = 1.0f;
    public Vector2 hDirection = new Vector2(1.0f, 0.0f);
    public Vector2 vDirection = new Vector2(0.0f, 1.0f);

    public bool RightMouseButtonHeldDown = false;

	// Update is called once per frame
	void Update () 
    {
        Vector2 direction = new Vector2(0.0f, 0.0f);
        direction += hDirection * Input.GetAxisRaw("Horizontal") + vDirection * Input.GetAxisRaw("Vertical");

        // Mouse  Stuff thats wierd
        if (RightMouseButtonHeldDown)
        {
            direction += hDirection * Input.GetAxisRaw("Mouse X") + vDirection * Input.GetAxisRaw("Mouse Y");
        }
        
        if (direction != Vector2.zero)
        {
            direction.Normalize();
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        //Really Crappy way of doing this
        if (Input.GetButtonDown("Fire2"))
        {
            RightMouseButtonHeldDown = true;
        }
        if (Input.GetButtonUp("Fire2"))
        {
            RightMouseButtonHeldDown = false;
        }

	}
}
