using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//
// StreamingDriver simply drives the streaming system. It doesn't currently need any data,
// it's just a tag component.
//

[Serializable]
public struct StreamingDriver : IComponentData
{    
}
