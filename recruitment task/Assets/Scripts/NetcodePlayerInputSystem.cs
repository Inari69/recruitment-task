using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
partial struct NetcodePlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<NetcodePlayerInput>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float2 move = PlayerInputBridge.Move;
        foreach (RefRW<NetcodePlayerInput> netcodePlayerInput in SystemAPI.Query<RefRW<NetcodePlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            netcodePlayerInput.ValueRW.inputVector = move;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
