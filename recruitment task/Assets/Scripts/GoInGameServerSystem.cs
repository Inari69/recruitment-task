using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
    
        foreach ((RefRO<GoInGameRequestRpc> rpc, RefRO<ReceiveRpcCommandRequest> req, Entity entity)
                 in SystemAPI.Query<RefRO<GoInGameRequestRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            Entity connection = req.ValueRO.SourceConnection;

            entityCommandBuffer.AddComponent<NetworkStreamInGame>(connection);
            Debug.Log("Client Connected: " + rpc.ValueRO.Nickname);
            
            entityCommandBuffer.AddComponent(connection, new PlayerNickname
            {
                Value = rpc.ValueRO.Nickname
            });

            Entity playerEntity = entityCommandBuffer.Instantiate(entitiesReferences.PlayerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(
                new float3(UnityEngine.Random.Range(-10, 10), 0.5f, 0)));

            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(connection);

            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value,
            });

            entityCommandBuffer.AddComponent(playerEntity, new PlayerNickname
            {
                Value = rpc.ValueRO.Nickname
            });

            entityCommandBuffer.AppendToBuffer(connection, new LinkedEntityGroup
            {
                Value = playerEntity,
            });

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
