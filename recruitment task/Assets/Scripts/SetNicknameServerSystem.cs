using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct SetNicknameServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<SetNicknameRpc> rpc, RefRO<ReceiveRpcCommandRequest> req, Entity entity)
                 in SystemAPI.Query<RefRO<SetNicknameRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            Entity connection = req.ValueRO.SourceConnection;

            ecb.AddComponent(connection, new PlayerNickname
            {
                Value = rpc.ValueRO.Nickname
            });

            Debug.Log("Nickname set: " + rpc.ValueRO.Nickname);

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}

public struct SetNicknameRpc : IRpcCommand
{
    public FixedString64Bytes Nickname;
}
