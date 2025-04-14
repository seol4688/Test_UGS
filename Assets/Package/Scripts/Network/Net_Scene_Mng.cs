using Unity.Netcode;
using UnityEngine;

public partial class Net_MNG : MonoBehaviour
{
    private void OnPlayerJoined()
    {
        if(NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    private void ChangeSceneForAllPlayers()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    //private void Update()
    //{
    //    if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClientsList.Count >= maxPlayers)
    //    {
    //        ChangeSceneForAllPlayers();
    //    }
    //}
}
