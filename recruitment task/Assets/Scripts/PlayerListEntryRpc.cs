using Unity.Collections;
using Unity.NetCode;


public struct PlayerListEntryRpc : IRpcCommand
{
    public FixedString64Bytes Nickname;
    public int NetworkId;
}
