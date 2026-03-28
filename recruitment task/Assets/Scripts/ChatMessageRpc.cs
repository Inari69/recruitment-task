using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

public struct ChatMessageRpc : IRpcCommand
{
    public FixedString64Bytes SenderNickname;
    public int TargetID;
    public FixedString128Bytes Message;
}
