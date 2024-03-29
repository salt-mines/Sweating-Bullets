﻿using Game;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    private GameInput input;
    private Transform player;
    private Vector2 viewAngles = Vector2.zero;

    public Vector2 ViewAngles => viewAngles;

    private void Awake()
    {
        input = FindObjectOfType<GameInput>();
        player = transform.parent;
    }

    private void Start()
    {
        GameInput.MouseLocked = true;
    }

    private void Update()
    {
        viewAngles.x += input.MouseX * Time.deltaTime;
        viewAngles.x %= 360;
        viewAngles.y = Mathf.Clamp(viewAngles.y - input.MouseY * Time.deltaTime, -89.99f, 89.99f);

        player.localRotation = Quaternion.AngleAxis(viewAngles.x, Vector3.up);
        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }

    public void SetAngles(Vector2 angles)
    {
        viewAngles = angles;
        player.localRotation = Quaternion.AngleAxis(viewAngles.x, Vector3.up);
        transform.localRotation = Quaternion.AngleAxis(viewAngles.y, Vector3.right);
    }
}