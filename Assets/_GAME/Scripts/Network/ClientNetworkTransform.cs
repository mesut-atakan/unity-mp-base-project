using Unity.Netcode.Components;
using UnityEngine;

namespace Aventra.Game
{
    [DisallowMultipleComponent]
    public sealed class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}