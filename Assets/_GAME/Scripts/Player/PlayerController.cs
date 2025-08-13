using UnityEngine;
using Unity.Netcode;

namespace Aventra.Game
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : NetworkBehaviour
    {
        private const string HORIZONTAL = "Horizontal";
        private const string VERTICAL = "Vertical";

        [SerializeField] private float speed = 20.0f;

        private ClientNetworkTransform _transform;

        private float GetHorizontalInput() => Input.GetAxis(HORIZONTAL);
        private float GetVerticalInput() => Input.GetAxis(VERTICAL);

        private void Start()
        {
            _transform = GetComponent<ClientNetworkTransform>();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            Vector2 movement = new Vector2(GetHorizontalInput(), GetVerticalInput());
            _transform.transform.position += (Vector3)(movement * speed * Time.deltaTime);
        }
    }   
}