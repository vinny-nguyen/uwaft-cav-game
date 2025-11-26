using System;
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
        // return a copy so callers can't mutate internal list by accident
        return new List<string>(data.friends);
    }

    public void AddFriend(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return;
        fullName = fullName.Trim();

        // avoid duplicates ignoring case
        bool exists = data.friends.Exists(f =>
            f.Equals(fullName, StringComparison.OrdinalIgnoreCase));

        if (!exists)
        {
            data.friends.Add(fullName);
            Save();
            Debug.Log("[FriendsManager] Added friend: " + fullName);
        }
    }

    public void RemoveFriend(string fullName)
    {
        bool removed = data.friends.RemoveAll(f =>
            f.Equals(fullName, StringComparison.OrdinalIgnoreCase)) > 0;

        if (removed)
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
        if (loaded != null && loaded.friends != null)
            data = loaded;
    }
}
