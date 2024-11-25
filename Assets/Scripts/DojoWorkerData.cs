using UnityEngine;

[CreateAssetMenu(fileName = "DojoWorkerData", menuName = "ScriptableObjects/DojoWorkerData", order = 2)]
public class DojoWorkerData : ScriptableObject
{
    public string masterAddress;
    public string masterPrivateKey;
    
}