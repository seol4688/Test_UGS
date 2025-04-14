using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public partial class Net_MNG : MonoBehaviour
{
    private Lobby currentLobby;

    private const int maxPlayers = 2;
    private string gameplaySceneName = "GamePlayScene";
    public Button startMatchBTN, joinMatchBTN;
    public TextMeshProUGUI joinCode;
    public TMP_InputField field;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        startMatchBTN.onClick.AddListener(() => StartMachmaking());
        joinMatchBTN.onClick.AddListener(() => JoinGameWithCode(field.text));
    }

    
}
