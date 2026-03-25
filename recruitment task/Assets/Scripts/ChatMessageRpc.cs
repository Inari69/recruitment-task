using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct ChatMessageRpc : IRpcCommand
{
    public int TargetID;
    public FixedString128Bytes Message;
}
