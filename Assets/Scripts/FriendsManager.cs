using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FriendsSaveData
{
    public List<string> friends = new();
}

public class FriendsManager : MonoBehaviour
{
    public static FriendsManager Instance;

    private const string PlayerPrefsKey = "FriendsList";
    private FriendsSaveData data = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public List<string> GetFriends()
    {
        return new List<string>(data.friends);
    }

    public void AddFriend(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim();

        if (!data.friends.Contains(name))
        {
            data.friends.Add(name);
            Save();
        }
    }

    public void RemoveFriend(string name)
    {
        if (data.friends.Remove(name))
            Save();
    }

    private void Save()
    {
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return;

        var json = PlayerPrefs.GetString(PlayerPrefsKey);
        var loaded = JsonUtility.FromJson<FriendsSaveData>(json);
        if (loaded != null) data = loaded;
    }
}
