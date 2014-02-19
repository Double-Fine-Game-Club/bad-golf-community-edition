using UnityEngine;
using System.Collections;

public class guiMainMenuSkinned : MonoBehaviour {

	public Texture Logo;
	public Vector2 ScrollPosition;

	private enum GUI_STATE {
		UI_STATE_DEFAULT = 0,
		UI_STATE_ONLINE = 1,
		UI_STATE_OFFLINE = 2,
		UI_STATE_OPTIONS = 3,
		UI_STATE_CREDITS = 4,
		UI_STATE_QUIT = 5
	};
	private GUI_STATE GuiState = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void DrawBaseGUI() {
		GUILayout.BeginArea(new Rect(0,0,200, Screen.height));

		GUILayout.BeginVertical();
		GUILayout.Box(Logo);

		if(GUILayout.Button("Online")) {
			GuiState = GUI_STATE.UI_STATE_ONLINE;
		}

		if(GUILayout.Button("Offline")) {
			GuiState = GUI_STATE.UI_STATE_OFFLINE;
		}

		if(GUILayout.Button("Options")) {
			GuiState = GUI_STATE.UI_STATE_OPTIONS;
		}

		if(GUILayout.Button("Credits")) {
			GuiState = GUI_STATE.UI_STATE_CREDITS;
		}

		if(GUILayout.Button("Quit to desktop")) {
			GuiState = GUI_STATE.UI_STATE_QUIT;
		}

		GUILayout.EndVertical();

		GUILayout.EndArea();
	}

	private void DrawOnlineGUI() {
		GUILayout.BeginArea(new Rect(200, 0, Screen.width - 200, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.Button("Host Game");
		GUILayout.Button("Join Game");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void DrawOfflineGUI() {
		GUILayout.BeginArea(new Rect(200, 0, Screen.width - 200, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.Label("Offline GUI");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	private void DrawOptionsGUI() {
		GUILayout.BeginArea(new Rect(200, 0, Screen.width - 200, Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Options GUI");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private void DrawCreditsGUI() {
		GUILayout.BeginArea(new Rect(200, 0, Screen.width - 200, Screen.height));
		ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Width(Screen.width - 220), GUILayout.Height(Screen.height - 20));
		GUILayout.Label("Lorem ipsum");
		GUILayout.Label("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam tempus mauris et convallis hendrerit. Nam eget lacus non nibh lobortis vehicula et vel sem. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam tincidunt luctus neque ut blandit. Quisque ac ante at lorem pulvinar euismod. Quisque imperdiet sagittis massa. Ut vel est velit. In hac habitasse platea dictumst. Pellentesque sed mauris eu nibh mattis tincidunt. Maecenas eu dolor id erat gravida mollis. Proin est dui, eleifend cursus suscipit sit amet, tincidunt at massa. Phasellus purus purus, gravida ut fermentum nec, ornare in lacus. Mauris ac commodo neque. Aliquam at dignissim lacus. Donec vitae erat sed massa iaculis congue.");
		GUILayout.Label("Curabitur lorem mi, ullamcorper non felis at, suscipit sollicitudin est. Integer non lobortis justo, rutrum facilisis nibh. Proin feugiat in dui eu sollicitudin. Etiam tristique tempus luctus. Donec porta elementum aliquam. Sed consequat dolor mi, sit amet tincidunt tortor pellentesque sit amet. Nam purus metus, lobortis sed lacinia vel, vestibulum ut mi. Fusce hendrerit et quam quis lobortis. Nullam vitae feugiat ligula, vel pellentesque augue. Curabitur quis mattis leo. Quisque pharetra nibh et dui feugiat adipiscing id et mauris. Sed suscipit magna quis nibh lacinia fringilla. Vestibulum porttitor, nunc quis congue hendrerit, est purus condimentum purus, vel venenatis sapien sapien quis sapien. Ut ante ipsum, vehicula eget varius nec, lobortis sed ligula. Suspendisse placerat, ante a tincidunt dapibus, ligula nulla bibendum lacus, eu dignissim elit massa non mauris.");
		GUILayout.Label("Aenean varius mauris dui, non congue tortor ultricies id. Nullam ut orci diam. Praesent ultrices fringilla purus, non congue erat venenatis vel. Suspendisse at dapibus arcu. Vestibulum facilisis erat magna, sit amet dignissim sapien ullamcorper nec. Suspendisse eros nibh, convallis vulputate massa ut, dignissim scelerisque augue. Nullam pharetra et elit et viverra. Proin ac magna nec turpis fringilla laoreet sit amet eget est.");
		GUILayout.Label("Donec enim nisi, posuere in diam et, interdum pellentesque turpis. Integer ac malesuada turpis. Donec augue nisi, gravida sit amet turpis vitae, hendrerit rutrum velit. Nullam ac ipsum sem. Morbi eros enim, rutrum vel mi venenatis, congue elementum sem. Integer mattis semper accumsan. Etiam eleifend iaculis lectus, eu porttitor mauris interdum quis. Suspendisse blandit purus non orci vestibulum laoreet. Nullam vel lectus vitae libero tempus porttitor sit amet quis tellus. Sed sed placerat nibh. Ut rhoncus non odio quis ullamcorper. Vivamus sit amet massa ut mauris laoreet dignissim id a purus. Duis eu interdum nulla. Phasellus iaculis mattis felis, at accumsan est tristique at. Morbi tincidunt tortor sed aliquet egestas.");
		GUILayout.Label("Quisque est purus, euismod vitae turpis in, placerat feugiat mi. Proin scelerisque at tellus nec adipiscing. Sed neque justo, hendrerit eget feugiat quis, ornare non purus. Integer egestas suscipit luctus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In nisi nulla, lobortis ac tempor at, consectetur nec ligula. Donec est massa, dapibus quis massa vel, lobortis hendrerit velit. Praesent mollis lectus at magna pretium tincidunt. Nunc ac nunc sit amet nulla bibendum varius vel ut turpis. Praesent auctor malesuada orci, in semper nunc volutpat");
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	
	void OnGUI () {
		DrawBaseGUI();

		switch(GuiState) {
		case GUI_STATE.UI_STATE_ONLINE:
			DrawOnlineGUI();
			break;
		case GUI_STATE.UI_STATE_OFFLINE:
			DrawOfflineGUI();
			break;
		case GUI_STATE.UI_STATE_OPTIONS:
			DrawOptionsGUI();
			break;
		case GUI_STATE.UI_STATE_CREDITS:
			DrawCreditsGUI();
			break;
		case GUI_STATE.UI_STATE_QUIT:
			break;
		default:
			break;
		}
	}
}
