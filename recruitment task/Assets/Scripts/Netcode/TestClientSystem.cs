using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter((WorldSystemFilterFlags.ClientSimulation))]
partial struct TestClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            Entity rpcEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(rpcEntity, new ChatMessageRpc
            {
                TargetID = -1,
                Message = "Hello World!"
            });
            state.EntityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest());
            Debug.Log("Sending Rpc");
        }*/
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    public void SendChatRpc(int targetID, string message)
    {
        
    }
}
