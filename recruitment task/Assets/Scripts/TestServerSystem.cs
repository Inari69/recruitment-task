using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter((WorldSystemFilterFlags.ServerSimulation))]
partial struct TestServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ChatMessageRpc> chatMessageRpc, RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity entity)
                 in SystemAPI.Query<RefRO<ChatMessageRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            Debug.Log("Received Rpc: " + chatMessageRpc.ValueRO.TargetID  + " " + chatMessageRpc.ValueRO.Message);
            entityCommandBuffer.DestroyEntity(entity);
        } 
        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
