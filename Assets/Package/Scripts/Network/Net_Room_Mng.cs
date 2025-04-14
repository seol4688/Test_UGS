using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public partial class Net_MNG : MonoBehaviour
{
    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrEmpty(inputJoinCode))
        {
            Debug.Log("유효하지 않은 Join Code입니다.");
            return;
        }

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            StartClient();
            Debug.Log("Join Code로 게임에 접속 성공");
        }
        catch (RelayServiceException e)
        {
            Debug.Log("게임 접속 실패" + e);
        }
    }

    public async void StartMachmaking()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인되지 않았습니다.");
            return;
        }

        currentLobby = await FindAvailableLooby();

        if (currentLobby == null)
        {
            await CreateNewLobby();
        }
        else
        {
            await JoinLobby(currentLobby.Id);
        }

    }

    private async Task<Lobby> FindAvailableLooby()
    {
        try
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            if (queryResponse.Results.Count > 0)
            {
                return queryResponse.Results[0];
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비 찾기 실패" + e);
        }
        return null;
    }

    private async Task CreateNewLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("랜덤매칭방", maxPlayers);
            Debug.Log("새로운 방 생성됨" + currentLobby.Id);
            await AllocateRelayServerAndjoin(currentLobby);
            StartHost();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비 생성 실패" + e);
        }
    }

    private async Task JoinLobby(string lobbyID)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID);
            Debug.Log("방에 접속되었습니다." + currentLobby.Id);
            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비 참가 실패 : " + e);
        }
    }

    private async Task AllocateRelayServerAndjoin(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay 서버 할당 완료. Join Code : " + joinCode);
            this.joinCode.text = joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Relay 서버 할당 실패" + e);
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트가 시작되었습니다.");

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnHostDisconnected;

    }

    private void OnClientConnected(ulong clientID)
    {
        OnPlayerJoined();
    }

    private void OnHostDisconnected(ulong clientID)
    {
        if(clientID == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostDisconnected;
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언드가 연결되었습니다.");
    }
}
