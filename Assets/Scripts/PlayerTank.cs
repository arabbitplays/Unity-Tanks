using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTank : Tank
{
    private Camera mainCamera;

    protected override void Awake()
    {
        mainCamera = Camera.main;
        base.Awake();
    }

    protected override void Update()
    {
        MoveHeadToCursor();
        base.Update();

        if (Input.GetMouseButtonDown(0)) {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Move(true);
        } else if (Input.GetKey(KeyCode.S))
        {
            Move(false);
        }

        if (Input.GetKey(KeyCode.D))
        {
            Turn(-turningSpeed * Time.fixedDeltaTime);
        } else if (Input.GetKey(KeyCode.A))
        {
            Turn(turningSpeed * Time.fixedDeltaTime);
        }
    }

    private void MoveHeadToCursor()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        currHeadDirection = (mousePos - (Vector2)transform.position).normalized;
    }

    public override void Destroy()
    {
        Debug.Log("You died");
    }
}
