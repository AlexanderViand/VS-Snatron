﻿using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Drive : MonoBehaviour {
	public Transform wallTemplate;
    private Transform _latestWallGameObject;

	static readonly Vector3 MinSpeed = Vector3.forward * 25;
	static readonly Vector3 MaxSpeed = Vector3.forward * 45;
	
	Vector3 _speed = MinSpeed;

	int _numberOfWallsNear = 0;

    private int HeightPixels;
	private int WidthPixels;

    private bool _showPauseMenu;
    private GUIStyle buttonStyle = new GUIStyle();

    Vector3 Offset {
        get {
            return Vector3.forward * 5;
        }
    }
    Vector3 WallOffset {
        get {
            return transform.rotation * Offset;
        }
    }

	Vector3 CurrentWallEnd {
		get {
		    if (_latestWallGameObject == null) return transform.position - WallOffset;
            if (Vector3.SqrMagnitude(_latestWallGameObject.GetComponent<WallBehaviour>().end - _latestWallGameObject.GetComponent<WallBehaviour>().start) > 20)
            {
		        return transform.position - WallOffset;
		    }
		    return transform.position;
		}
	}

	void AdjustSpeed ()
	{
	    _speed = Vector3.MoveTowards(_speed, _numberOfWallsNear > 1 ? MaxSpeed : MinSpeed, 30 * Time.deltaTime);
	}

// ReSharper disable once UnusedMember.Local
	void Start () {
	    buttonStyle.normal.background = Resources.Load<Texture2D>("TextBox");
	    buttonStyle.normal.textColor = Color.white;
	    var size = HeightPixels/50;
        buttonStyle.fontSize = size < 12 ? 12 : size;
        buttonStyle.alignment = TextAnchor.MiddleCenter;

		NewWall();
#if UNITY_Android
		using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
		    metricsClass = new AndroidJavaClass("android.util.DisplayMetrics")
		) {
            using(
    			AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
    			activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
    			windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
    			displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
			) {
				displayInstance.Call ("getMetrics", metricsInstance);
				HeightPixels = metricsInstance.Get<int> ("heightPixels");
				WidthPixels = metricsInstance.Get<int> ("widthPixels");
			}
		}
#else
        HeightPixels = Screen.height;
        WidthPixels = Screen.width;
#endif
	}
	
// ReSharper disable once UnusedMember.Local
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _showPauseMenu = !_showPauseMenu;
        }
	    AdjustSpeed();
	    transform.Translate(_speed*Time.deltaTime);
	    if (_latestWallGameObject != null) {
	        _latestWallGameObject.GetComponent<WallBehaviour>().updateWall(CurrentWallEnd);
	    }


	    if (GetComponent<NetworkView>().isMine) {
	        //Handling touch input
	        foreach (var touch in Input.touches.Where(touch => touch.phase == TouchPhase.Began)) {
	            if (touch.position.x > WidthPixels/2) {
	                TurnRight();
	            }
	            else {
	                TurnLeft();
	            }
	        }


	        // Input for preview
	        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
	            TurnLeft();
	        }
	        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
	            TurnRight();
	        }
	    }
	}

    void TurnLeft()
    {
        _latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        transform.Rotate(Vector3.up, 270);
        NewWall();
    }

    void TurnRight()
    {
        _latestWallGameObject.GetComponent<WallBehaviour>().updateWall(transform.position);
        transform.Rotate(Vector3.up, 90);
        
        NewWall();
    }

// ReSharper disable UnusedMember.Local
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "wall") {
			print (other.name + " " + name);
				_numberOfWallsNear++;
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "wall") {
			_numberOfWallsNear--;
		}
	}

    private void OnGUI() {
        if (_showPauseMenu) {
            if (GUI.Button(new Rect(15/30f*WidthPixels, 10/20f*HeightPixels, 1/10f*WidthPixels, 1/20f*HeightPixels),
                "Exit", buttonStyle)) {
                Application.Quit();
            }
        }
    }
// ReSharper restore UnusedMember.Local

	void NewWall() {
	    _latestWallGameObject = (Transform) Network.Instantiate(wallTemplate, CurrentWallEnd, Quaternion.identity, 0);
		_latestWallGameObject.GetComponent<WallBehaviour> ().start = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().end = CurrentWallEnd;
		_latestWallGameObject.GetComponent<WallBehaviour> ().updateWall (CurrentWallEnd);
	}
}