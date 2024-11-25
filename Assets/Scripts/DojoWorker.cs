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
}
