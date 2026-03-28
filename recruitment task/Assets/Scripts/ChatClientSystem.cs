using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ChatClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (rpc, entity) in
                 SystemAPI.Query<RefRO<ChatMessageRpc>>()
                     .WithAll<ReceiveRpcCommandRequest>()
                     .WithEntityAccess())
        {
            int targetId = rpc.ValueRO.TargetID;
            string senderNickname = rpc.ValueRO.SenderNickname.ToString();
            string message = rpc.ValueRO.Message.ToString();

            Debug.Log("Received chat: " + message);

            MainMenu menu = Object.FindFirstObjectByType<MainMenu>();
            if (menu != null)
            {
                menu.UpdateChatOutput(senderNickname, message, targetId);
            }
            
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}