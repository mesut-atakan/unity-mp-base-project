using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Aventra.Game
{
    [RequireComponent(typeof(NetworkTransform))]
    public class PlayerNetwork : NetworkBehaviour
    {
        private const string HORIZONTAL = "Horizontal";
        private const string VERTICAL = "Vertical";

        [SerializeField] private Transform sphereObj;
        [SerializeField] private float moveSpeed = 2.0f;

        private Transform spawnedTransform;
        private NetworkVariable<int> _randomValue = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private NetworkVariable<MyCustomData> _customVariable = new NetworkVariable<MyCustomData>(
            new MyCustomData()
            {
                CustomInt = 1,
                CustomBool = true,
                Message = "All your base are bellong to us!"
            }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public struct MyCustomData : INetworkSerializable
        {
            public int CustomInt;
            public bool CustomBool;
            public FixedString128Bytes Message;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref CustomInt);
                serializer.SerializeValue(ref CustomBool);
                serializer.SerializeValue(ref Message);
            }
        }

        public float GetHorizontal() =>
            Input.GetAxisRaw(HORIZONTAL);

        public float GetVertical() =>
            Input.GetAxisRaw(VERTICAL);

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _randomValue.OnValueChanged += OnValueChanged;
            _customVariable.OnValueChanged += OnCustomDataChanged;
        }

        private void OnCustomDataChanged(MyCustomData previousValue, MyCustomData newValue)
        {
            Debug.Log($"{OwnerClientId}; {newValue.CustomInt}; {newValue.CustomBool}\nMessage: {newValue.Message}");
        }

        private void OnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"On Value Changed Previous Value: {previousValue}\tNew Value: {newValue}", gameObject);
        }

        private void Update()
        {
            if (IsServer && Input.GetKeyDown(KeyCode.Y))
            {
                Debug.Log($"Host");
                TestServerRpc(new ServerRpcParams());
            }

            if (!IsOwner)
                return;

            if (Input.GetKeyDown(KeyCode.T))
            {
                _randomValue.Value = Random.Range(0, 100);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _customVariable.Value = new MyCustomData
                {
                    CustomBool = true,
                    CustomInt = Random.Range(0, 100)
                };
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TestServerRpc(new ServerRpcParams());
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 0 } } });
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                spawnedTransform = Instantiate(sphereObj);
                spawnedTransform.GetComponent<NetworkObject>().Spawn(true);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                Destroy(spawnedTransform.gameObject);
            }
                CharacterMove();
        }

        private void CharacterMove()
        {
            Vector3 movDir = new Vector3(GetHorizontal(), 0, GetVertical()).normalized;
            transform.position += movDir * moveSpeed * Time.deltaTime;
        }

        [ServerRpc]
        private void TestServerRpc(ServerRpcParams serverRpcParams)
        {
            Debug.Log("Test Server Rpc; " + OwnerClientId + " " + serverRpcParams.Receive.SenderClientId);
            TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
        }

        [ClientRpc]
        private void TestClientRpc(ClientRpcParams clientRpcParams)
        {
            Debug.Log($"Test Client Rpc {OwnerClientId}");
        }
    }
}