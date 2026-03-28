using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct PlayerListRequestServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> req, Entity entity)
                 in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>()
                     .WithAll<RequestPlayerListRpc>()
                     .WithEntityAccess())
        {
            Entity requester = req.ValueRO.SourceConnection;
            
            foreach ((RefRO<NetworkId> netId, RefRO<PlayerNickname> nickname, Entity connection)
                     in SystemAPI.Query<RefRO<NetworkId>, RefRO<PlayerNickname>>().WithEntityAccess())
            {
                Entity rpc = ecb.CreateEntity();

                ecb.AddComponent(rpc, new PlayerListEntryRpc
                {
                    Nickname = nickname.ValueRO.Value,
                    NetworkId = netId.ValueRO.Value
                });

                ecb.AddComponent(rpc, new SendRpcCommandRequest
                {
                    TargetConnection = requester
                });
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}
