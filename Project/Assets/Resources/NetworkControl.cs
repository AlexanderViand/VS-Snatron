﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets;
using UnityEngine;
using System.Collections;

public class NetworkControl : MonoBehaviour {
    public static bool IsSearching = true;
    public static readonly List<Server> Servers = new List<Server>();
    public static int PlayerID;
    private static readonly Dictionary<int, String> PlayerId2Username = new Dictionary<int, String>();
    private static int _currentPlayerID = 0;
    public static string HostName = "";
    public static int NumberOfPlayers = 0;
    public static int NumberOfCubes = 5;
    private const int FieldBorderCoordinates = 200;
    private static Dictionary<int, Vector3> StartPositions = new Dictionary<int, Vector3>();  

    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void StartListeningForNewServers() {
        (new Thread(() =>
        {
            while (IsSearching)
            {
                var newServer = ServerDiscoverer.DiscoverServers();
                Debug.Log("Discovered new Server");
                var addServer = true;
                foreach (var server in Servers.Where(server => server.Ip.Equals(newServer.Ip)))
                {
                    addServer = false;
                }
                if (addServer && newServer != null && newServer.Name != null)
                {
                    Servers.Add(newServer);
                }
            }
        })
            ).Start();
    }

    public static void AnnounceServer() {
        ServerHoster.HostServer(HostName);
        Network.InitializeServer(NumberOfPlayers, Protocol.GamePort, false);
        Network.sendRate = 30;
        InitializeStartPositions();
    }

    private static void InitializeStartPositions() {
        for (int i = 0; i < NumberOfPlayers + 1; i++) {
            double angle = 360/(i + 1) * Math.PI / 180;
            //TODO: check calculations
            StartPositions.Add(i, new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle)));
        }
    }

    public static void StopAnnouncingServer() {
        ServerHoster.IsHosting = false;
    }

    public static void Connect(string ip, int port) {
        Network.Connect(ip, port);
    }

    public static void AddPlayer(string playerName) {
        PlayerId2Username.Add(_currentPlayerID++, playerName);
    }

    public static void InstantiateCubes() {
        List<Transform> cubes = new List<Transform>();
        System.Random random = new System.Random();
        var shader = Shader.Find("Diffuse");
        for(int i = 0; i < NumberOfCubes; ++i) {
            int _x = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
            int _z = random.Next(-(int) FieldBorderCoordinates, (int) FieldBorderCoordinates);
            cubes.Add(Network.Instantiate(Resources.Load<Transform>("Cube"), new Vector3((float)_x, 1.5f, (float)_z), Quaternion.identity, 0) as Transform);
            cubes[i].renderer.material.shader = shader;
        }
    }

    public static void InstantiateGameBorders() {
        var leftWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var frontWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var rightWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var backWall =
            Network.Instantiate(Resources.Load<Transform>("Wall"), Vector3.zero, Quaternion.identity, 0) as Transform;
        var wallColor = new Color(204 / 255f, 204 / 255f, 204 / 255f, 1f);
        var shader = Shader.Find("Diffuse");
        if (leftWall != null)
        {
            leftWall.transform.localScale = new Vector3(1, 5, 1);
            leftWall.GetComponent<WallBehaviour>().start = new Vector3(-FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates);
            leftWall.GetComponent<WallBehaviour>().updateWall(new Vector3(FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates));
            leftWall.GetComponent<WallBehaviour>().setDefaultColor(wallColor);
            leftWall.renderer.material.shader = shader;
        }
        if (frontWall != null)
        {
            frontWall.transform.localScale = new Vector3(1, 5, 1);
            frontWall.GetComponent<WallBehaviour>().start = new Vector3(FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates);
            frontWall.GetComponent<WallBehaviour>().updateWall(new Vector3(FieldBorderCoordinates, 1.5f, FieldBorderCoordinates));
            frontWall.GetComponent<WallBehaviour>().setDefaultColor(wallColor);
            frontWall.renderer.material.shader = shader;
        }
        if (rightWall != null)
        {
            rightWall.transform.localScale = new Vector3(1, 5, 1);
            rightWall.GetComponent<WallBehaviour>().start = new Vector3(FieldBorderCoordinates, 1.5f, FieldBorderCoordinates);
            rightWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-FieldBorderCoordinates, 1.5f, FieldBorderCoordinates));
            rightWall.GetComponent<WallBehaviour>().setDefaultColor(wallColor);
            rightWall.renderer.material.shader = shader;
        }
        if (backWall != null)
        {
            backWall.transform.localScale = new Vector3(1, 5, 1);
            backWall.GetComponent<WallBehaviour>().start = new Vector3(-FieldBorderCoordinates, 1.5f, FieldBorderCoordinates);
            backWall.GetComponent<WallBehaviour>().updateWall(new Vector3(-FieldBorderCoordinates, 1.5f, -FieldBorderCoordinates));
            backWall.GetComponent<WallBehaviour>().setDefaultColor(wallColor);
            backWall.renderer.material.shader = shader;
        }
    }

    public static void StopSearching() {
        IsSearching = false;
    }

    private static int NextPlayerID() {
        return ++_currentPlayerID;
    }

    [RPC]
    private void SetPlayerID(int id)
    {
        Debug.Log("received RPC");
        PlayerID = id;
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        GetComponent<NetworkView>().RPC("SetPlayerID", player, NextPlayerID());
    }

    private class Game
    {
        private readonly Transform _playerPrefab;
        private readonly Transform _gridPrefab;

        public Game(Transform playerPrefab, Transform gridPrefab)
        {
            _playerPrefab = playerPrefab;
            _gridPrefab = gridPrefab;
        }

        private void SpawnPlayer()
        {
            var player = Network.Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity, 0) as Transform;
            var cam = GameObject.Find("Main Camera");
            cam.AddComponent<SmoothFollow>().target = player;

            Instantiate(_gridPrefab, Vector3.zero, Quaternion.identity);
            Instantiate(_gridPrefab, Vector3.zero, Quaternion.FromToRotation(Vector3.forward, Vector3.right));
        }

        public void StartGame()
        {
            SpawnPlayer();
        }
    }

    public static void StartGame() {
        var game = new Game(Resources.Load<Transform>("Player"), Resources.Load<Transform>("Lines"));
        game.StartGame();
    }
}
