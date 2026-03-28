using Unity.Collections;
using Unity.Entities;

public struct PlayerNickname : IComponentData
{
    public FixedString64Bytes Value;
}
