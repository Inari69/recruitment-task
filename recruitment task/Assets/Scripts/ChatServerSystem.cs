using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ChatServerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (rpc, entity) in
                 SystemAPI.Query<RefRO<ChatMessageRpc>>()
                     .WithAll<ReceiveRpcCommandRequest>()
                     .WithEntityAccess())
        {
            var em = state.EntityManager;

            var msg = rpc.ValueRO;
            
            if (msg.TargetID == -1)
            {
                foreach (var connection in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                {
                    Entity rpcEntity = em.CreateEntity();

                    em.AddComponentData(rpcEntity, new ChatMessageRpc
                    {
                        SenderNickname = msg.SenderNickname,
                        TargetID = msg.TargetID,
                        Message = msg.Message
                    });

                    em.AddComponentData(rpcEntity, new SendRpcCommandRequest
                    {
                        TargetConnection = connection.Item2
                    });
                }
            }
            else
            {
                foreach (var (id, connEntity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                {
                    if (id.ValueRO.Value == msg.TargetID)
                    {
                        Entity rpcEntity = em.CreateEntity();

                        em.AddComponentData(rpcEntity, msg);

                        em.AddComponentData(rpcEntity, new SendRpcCommandRequest
                        {
                            TargetConnection = connEntity
                        });

                        break;
                    }
                }
            }

            em.DestroyEntity(entity);
        }
    }
}