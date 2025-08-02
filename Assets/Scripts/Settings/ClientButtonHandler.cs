using Unity.Netcode;
using UnityEngine;

public class ClientButtonHandler : MonoBehaviour
{
    public LetterManager letterManager;

    public void OnRightPressed()
    {
        if (letterManager != null && NetworkManager.Singleton.IsClient)
        {
            letterManager.RightBtnServerRpc();
        }
    }

    public void OnIncorrectPressed()
    {
        if (letterManager != null && NetworkManager.Singleton.IsClient)
        {
            letterManager.IncorrectBtnServerRpc();
        }
    }
}
