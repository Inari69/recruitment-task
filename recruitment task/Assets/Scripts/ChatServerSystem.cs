using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ChatServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (rpc, entity) in
                 SystemAPI.Query<RefRO<ChatMessageRpc>>()
                     .WithAll<ReceiveRpcCommandRequest>()
                     .WithEntityAccess())
        {
            var msg = rpc.ValueRO;
            
            if (msg.TargetID == -1)
            {
                foreach (var (netId, connEntity) in
                         SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                {
                    Entity rpcEntity = ecb.CreateEntity();

                    ecb.AddComponent(rpcEntity, new ChatMessageRpc
                    {
                        SenderNickname = msg.SenderNickname,
                        TargetID = msg.TargetID,
                        Message = msg.Message
                    });

                    ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
                    {
                        TargetConnection = connEntity
                    });
                }
            }

            else
            {
                foreach (var (netId, connEntity) in
                         SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                {
                    if (netId.ValueRO.Value == msg.TargetID)
                    {
                        Entity rpcEntity = ecb.CreateEntity();

                        ecb.AddComponent(rpcEntity, msg);

                        ecb.AddComponent(rpcEntity, new SendRpcCommandRequest
                        {
                            TargetConnection = connEntity
                        });

                        break;
                    }
                }
            }

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}