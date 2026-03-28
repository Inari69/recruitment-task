using System;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] public GameObject mainMenu;
    private MainMenu _menu;
    public int NetworkId;

    private void Awake()
    {
        _menu = mainMenu.GetComponent<MainMenu>();
    }

    public void OnClick()
    {
        _menu.SetChatTarget(NetworkId);
    }
}
