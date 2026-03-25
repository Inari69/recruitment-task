using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    public static float2 Move;

    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        Debug.Log($"Awake: {this.GetType().Name}");
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    void Update()
    {
        Vector2 move = _inputActions.Player.Move.ReadValue<Vector2>();
        Move = move;
        
        Debug.Log($"Move: {move}");
    }
}
