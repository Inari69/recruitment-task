using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct PlayerListClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<PlayerListEntryRpc> rpc, Entity entity)
                 in SystemAPI.Query<RefRO<PlayerListEntryRpc>>().WithEntityAccess())
        {
            Debug.Log($"PLAYER: {rpc.ValueRO.Nickname} | ID: {rpc.ValueRO.NetworkId}");

            MainMenu menu = Object.FindFirstObjectByType<MainMenu>();
            if (menu != null)
            {
                menu.AddPlayerToList(rpc.ValueRO.Nickname.ToString(), rpc.ValueRO.NetworkId);
            }
            
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }
}
