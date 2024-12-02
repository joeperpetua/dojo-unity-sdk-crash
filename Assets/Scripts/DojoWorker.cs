using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dojo;
using Dojo.Starknet;
using TMPro;
using UnityEngine;

public class DojoWorker : MonoBehaviour
{
    [SerializeField] WorldManager worldManager;
    [SerializeField] WorldManagerData dojoConfig;
    [SerializeField] DojoWorkerData dojoWorkerData;
    public Actions actions;
    public JsonRpcClient provider;
    public Account masterAccount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        worldManager.synchronizationMaster.OnEventMessage.AddListener(HandleEvent);
        worldManager.synchronizationMaster.OnSynchronized.AddListener(HandleSync);
        worldManager.synchronizationMaster.OnEntitySpawned.AddListener(HandleSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateAccount() {
        provider = new JsonRpcClient(dojoConfig.rpcUrl);
        masterAccount = new Account(provider, new SigningKey(dojoWorkerData.masterPrivateKey), new FieldElement(dojoWorkerData.masterAddress));
    }

    public async void CreatePlayer() {
        string username = GameObject.FindGameObjectWithTag("UsernameInput").GetComponent<TMP_InputField>().text;
        
        // Encode to avoid "Invalid decimal string" error when creating FieldElement
        byte[] bytes = Encoding.UTF8.GetBytes(username);
        BigInteger encodedUsername = new BigInteger(bytes, isUnsigned: true);

        await actions.create_player(masterAccount, new FieldElement(encodedUsername));
    }

    void HandleEvent(ModelInstance modelInstance)
    {
        // Debug.Log(modelInstance);
    }

    void HandleSpawn(GameObject spawnedEntity)
    {
        Debug.Log($"Entity Spawned: {spawnedEntity.name} || {spawnedEntity.transform.position}");
    }

    void HandleSync(List<GameObject> syncedObjects)
    {
        foreach (var item in syncedObjects)
        {
            Debug.Log($"Synced Objects: {item}");
        }
    } 
}
