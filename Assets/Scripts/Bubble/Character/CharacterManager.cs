using System;
using Bubble.Character.Interface;
using ProtoLib.Library.Facet;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;

namespace Bubble.Character
{
    public class CharacterManager : ScriptManager<ICharacterComponent>
    {
        public FacetAPI CharAPI { get; set; } = FacetAPI.Create()
            .Realtime<Action<int>>("attributeShields")
            .Callback<Action>("onDeath")
            .Realtime<Action<float>>("attributeDashCooldownProgress")
            .Build();
    }
}
