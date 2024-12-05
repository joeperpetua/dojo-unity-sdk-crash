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
    private TMP_Text playerPositionText;
    private TMP_Text gameIDText;
    private TMP_Text gameFloorText;
    private GameObject playerEntity;
    private GameObject gameEntity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        provider = new JsonRpcClient(dojoConfig.rpcUrl);
        masterAccount = new Account(provider, new SigningKey(dojoWorkerData.masterPrivateKey), new FieldElement(dojoWorkerData.masterAddress));
        burnerManager = new BurnerManager(provider, masterAccount);

        addressText = GameObject.FindGameObjectWithTag("AddressText").GetComponent<TMP_Text>();
        usernameText = GameObject.FindGameObjectWithTag("UsernameText").GetComponent<TMP_Text>();
        playerPositionText = GameObject.FindGameObjectWithTag("PlayerPositionText").GetComponent<TMP_Text>();
        gameIDText = GameObject.FindGameObjectWithTag("GameIDText").GetComponent<TMP_Text>();
        gameFloorText = GameObject.FindGameObjectWithTag("GameFloorText").GetComponent<TMP_Text>();

        account = masterAccount;
        addressText.text = account.Address.Hex();

        // string hashedKey = GetPoseidonHash(account.Address);
        // Debug.Log($"Address: {account.Address.Hex()}");
        // Debug.Log($"Hash: {hashedKey}");
        // GameObject playerEntityGameObject = worldManager.Entity(hashedKey);

        worldManager.synchronizationMaster.OnEntitySpawned.AddListener(HandleSpawn);
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

    public async void CreateGame() {
        await actions.create_game(account);
    }

    public async void EndGame() {
        await actions.end_game(account);
    }

    public async void Move(int direction) {
        Direction dir = (Direction)Direction.FromIndex(typeof(Direction), direction);
        await actions.move(account, dir);
    }

    void HandleSpawn(GameObject spawnedEntity)
    {
        var playerState = playerEntity == null ? null : playerEntity.GetComponent<depths_of_dread_PlayerState>();

        string playerKey = GetPoseidonHash(account.Address);
        var gameKey = playerState == null ? null : GetPoseidonHash(new FieldElement(playerState.game_id));
        
        if (spawnedEntity == null) { return; }
        // Debug.Log($"Entity Spawned: {spawnedEntity}");
        
        if (spawnedEntity.name == playerKey)
        {
            if (spawnedEntity.TryGetComponent(out depths_of_dread_PlayerData _playerData))
            {
                playerEntity = spawnedEntity;
                playerEntity.GetComponent<depths_of_dread_PlayerData>().OnUpdated.AddListener(OnPlayerDataUpdate);

                OnPlayerDataUpdate();
            }

            if (spawnedEntity.TryGetComponent(out depths_of_dread_PlayerState _playerState))
            {
                playerEntity = spawnedEntity;
                playerEntity.GetComponent<depths_of_dread_PlayerState>().OnUpdated.AddListener(OnPlayerStateUpdate);

                OnPlayerStateUpdate();
            }

            if (spawnedEntity.TryGetComponent(out depths_of_dread_PlayerPowerUps _playerPowerUps))
            {
                playerEntity = spawnedEntity;
                playerEntity.GetComponent<depths_of_dread_PlayerPowerUps>().OnUpdated.AddListener(OnPlayerPowerUpsUpdate);

                OnPlayerPowerUpsUpdate();
            }
        }
        
        // This is always 0 for some reason
        Debug.Log($"Game_ID: {playerState?.game_id}");
        if (spawnedEntity.name == gameKey) 
        {
            if (spawnedEntity.TryGetComponent(out depths_of_dread_GameData _gameData))
            {
                gameEntity = spawnedEntity;
                gameEntity.GetComponent<depths_of_dread_GameData>().OnUpdated.AddListener(OnGameDataUpdate);

                OnGameDataUpdate();
            }

            if (spawnedEntity.TryGetComponent(out depths_of_dread_GameFloor _gameFloor))
            {
                gameEntity = spawnedEntity;
                gameEntity.GetComponent<depths_of_dread_GameFloor>().OnUpdated.AddListener(OnGameFloorUpdate);

                OnGameFloorUpdate();
            }

            if (spawnedEntity.TryGetComponent(out depths_of_dread_GameCoins _gameCoins))
            {
                gameEntity = spawnedEntity;
                gameEntity.GetComponent<depths_of_dread_GameCoins>().OnUpdated.AddListener(OnGameCoinsUpdate);

                OnGameCoinsUpdate();
            }
        }  
    }

    void OnPlayerDataUpdate() {
        Debug.Log($"Updated {playerEntity} data");

        string usernameHex = playerEntity.GetComponent<depths_of_dread_PlayerData>().username.Hex();
        usernameText.text = HexToASCII(usernameHex);
    }

    void OnPlayerStateUpdate() {
        Debug.Log($"Updated {playerEntity} state");
        var playerState = playerEntity.GetComponent<depths_of_dread_PlayerState>();

        gameIDText.text = $"Game ID: {playerState.game_id}";
        gameFloorText.text = $"Floor: {playerState.current_floor}";
        playerPositionText.text = $"Position: {playerState.position.x}, {playerState.position.y}";
    }

    void OnPlayerPowerUpsUpdate() {
        Debug.Log($"Updated {playerEntity} powerups");
    }

    void OnGameDataUpdate() {
        Debug.Log($"Updated {gameEntity} data");
    }

    void OnGameFloorUpdate() {
        Debug.Log($"Updated {gameEntity} floor");
    }

    void OnGameCoinsUpdate() {
        Debug.Log($"Updated {gameEntity} coins");
    }
}
