using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using bottlenoselabs.C2CS.Runtime;
using Dojo;
using Dojo.Starknet;
using Dojo.Torii;
using TMPro;
using UnityEngine;
using static EncodingService;

interface IPlayerEntity
{
    depths_of_dread_PlayerData PlayerData { get; set; }
    depths_of_dread_PlayerState PlayerState { get; set; }
    depths_of_dread_PlayerPowerUps PlayerPowerUps { get; set; }
}

public class DojoWorker : MonoBehaviour
{
    [SerializeField] WorldManager worldManager;
    [SerializeField] WorldManagerData dojoConfig;
    [SerializeField] DojoWorkerData dojoWorkerData;
    public Actions actions;
    public JsonRpcClient provider;
    public Account masterAccount;
    public BurnerManager burnerManager;
    private Account account;
    private TMP_Text addressText;
    private TMP_Text usernameText;
    private IPlayerEntity playerEntity;
    // Update is called once per frame

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        provider = new JsonRpcClient(dojoConfig.rpcUrl);
        masterAccount = new Account(provider, new SigningKey(dojoWorkerData.masterPrivateKey), new FieldElement(dojoWorkerData.masterAddress));
        burnerManager = new BurnerManager(provider, masterAccount);

        addressText = GameObject.FindGameObjectWithTag("AddressText").GetComponent<TMP_Text>();
        usernameText = GameObject.FindGameObjectWithTag("UsernameText").GetComponent<TMP_Text>();

        account = masterAccount;
        addressText.text = account.Address.Hex();

        string hashedKey = GetPoseidonHash(account.Address);
        Debug.Log($"Address: {account.Address.Hex()}");
        Debug.Log($"Hash: {hashedKey}");
        GameObject playerEntityGameObject = worldManager.Entity(hashedKey);

        if (playerEntityGameObject != null) {
            if (playerEntityGameObject.TryGetComponent(out depths_of_dread_PlayerData playerData))
            {
                playerEntity.PlayerData.OnUpdatedModel.AddListener(OnPlayerDataUpdate);
                playerEntity.PlayerData = playerData;
            }

            if (playerEntityGameObject.TryGetComponent(out depths_of_dread_PlayerState playerState))
            {
                playerEntity.PlayerState.OnUpdatedModel.AddListener(OnPlayerStateUpdate);
                playerEntity.PlayerState = playerState;
            }

            if (playerEntityGameObject.TryGetComponent(out depths_of_dread_PlayerPowerUps playerPowerUps))
            {
                playerEntity.PlayerPowerUps.OnUpdatedModel.AddListener(OnPlayerPowerUpsUpdate);
                playerEntity.PlayerPowerUps = playerPowerUps;
            }
        }

        // worldManager.synchronizationMaster.OnEntitySpawned.AddListener(HandleSpawn);
        // worldManager.synchronizationMaster.OnEntityUpdated.AddListener(HandleUpdate);
    }

    public async void CreateBurner() {
        var burner = await burnerManager.DeployBurner();
        account = burner;
        addressText.text = account.Address.Hex();
    }

    public async void CreatePlayer() {
        string username = GameObject.FindGameObjectWithTag("UsernameInput").GetComponent<TMP_InputField>().text;
        BigInteger encodedUsername = ASCIIToBigInt(username);
        await actions.create_player(account, new FieldElement(encodedUsername));
    }

    // void HandleSpawn(GameObject spawnedEntity)
    // {
    //     Debug.Log($"Entity Spawned: {spawnedEntity}");
    //     if (spawnedEntity.TryGetComponent(out depths_of_dread_PlayerData playerData))
    //     {
    //         OnPlayerDataInit(spawnedEntity);
    //         playerData.OnUpdated.AddListener(OnPlayerDataUpdate);
    //     }
    // }

    void OnPlayerDataUpdate(Model playerData) {
        GameObject entity = FindObjectOfType<depths_of_dread_PlayerData>().gameObject;
        Debug.Log($"Updated {entity}");

        string usernameHex = entity.GetComponent<depths_of_dread_PlayerData>().username.Hex();
        usernameText.text = HexToASCII(usernameHex);
    }

    void OnPlayerStateUpdate(Model playerData) {
        GameObject entity = FindObjectOfType<depths_of_dread_PlayerData>().gameObject;
        Debug.Log($"Updated {entity}");

        string usernameHex = entity.GetComponent<depths_of_dread_PlayerData>().username.Hex();
        usernameText.text = HexToASCII(usernameHex);
    }

    void OnPlayerPowerUpsUpdate(Model playerData) {
        GameObject entity = FindObjectOfType<depths_of_dread_PlayerData>().gameObject;
        Debug.Log($"Updated {entity}");

        string usernameHex = entity.GetComponent<depths_of_dread_PlayerData>().username.Hex();
        usernameText.text = HexToASCII(usernameHex);
    }
}
