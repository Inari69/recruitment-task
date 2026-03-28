using System;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] public GameObject mainMenu;
    [SerializeField] public GameObject nickname;
    private TMP_Text _nicknameText;
    private MainMenu _menu;
    public int networkId;

    private void Awake()
    {
        _menu = mainMenu.GetComponent<MainMenu>();
        _nicknameText = nickname.GetComponent<TMP_Text>();
    }

    public void OnClick()
    {
        _menu.SetChatTarget(networkId);
    }

    public void SetNickname(string nickname)
    {
        _nicknameText.text = nickname;
    }
    
    public void SetNetworkId(int id)
    {
        networkId = id;
    }
}
