using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PostIt
{
	public class PostItWindow : EditorWindow
	{
		[MenuItem("Window/Post-It-Assets/Open Window")]
		static void Open()
		{
			GetWindow<PostItWindow>("Post-It-Assets");
		}

		private void OnEnable()
		{
			wantsMouseMove = wantsMouseEnterLeaveWindow = true;
			var data = JsonUtility.FromJson<JsonFormatter>(EditorPrefs.GetString("PostItNotes.PostItWindow.List", "{}"));
			for (int i = 0, max = data.filePathes.Count; i < max; ++i)
			{
				var postData = new PostItData()
				{
					name = string.Empty,
					note = string.Empty
				};
				if (data.datas.Count > i)
				{
					postData = data.datas[i];
				}
				pathHashes.Add(data.filePathes[i]);
				postItDatas.Add(data.filePathes[i], postData);
			}
		}

		private void OnDisable()
		{
			List<string> ordered = pathHashes.ToList();
			List<PostItData> sortted = new List<PostItData>();
			foreach (var key in ordered)
			{
				sortted.Add(postItDatas[key]);
			}
			EditorPrefs.SetString("PostItNotes.PostItWindow.List", JsonUtility.ToJson(new JsonFormatter()
			{
				filePathes = ordered,
				datas = sortted
			}));
		}

		[System.Serializable]
		struct JsonFormatter
		{
			public List<string> filePathes;
			public List<PostItData> datas;
		}

		[System.Serializable]
		struct PostItData
		{
			public string name;
			public string note;
		}

		NoteWindow noteWindow;
		Dictionary<string, PostItData> postItDatas = new Dictionary<string, PostItData>();
		HashSet<string> pathHashes = new HashSet<string>();
		int checkRemoveID = 0;
		Vector2 scrollView;
		private void OnGUI()
		{
			var pos = position;
			pos.x = pos.y = 0;

			scrollView = EditorGUILayout.BeginScrollView(scrollView);
			string removePath = null;
			foreach (var path in pathHashes)
			{
				EditorGUILayout.BeginHorizontal();
				int id = path.GetHashCode();
				if (checkRemoveID == id)
				{
					EditorGUILayout.LabelField("Are you sure you want to remove this object?");
					if (GUILayout.Button("Yes", GUILayout.Width(60f)))
					{
						removePath = path;
						checkRemoveID = 0;
					}
					if (GUILayout.Button("No", GUILayout.Width(60f)))
					{
						checkRemoveID = 0;
					}
				}
				else
				{
					var data = postItDatas[path];
					data.name = EditorGUILayout.TextField(data.name, GUILayout.MaxWidth(120f));

					var Object = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
					EditorGUILayout.ObjectField(Object, typeof(UnityEngine.Object), true);

					if (Object == null)
					{
						removePath = path;
					}
					else
					{
						if (GUILayout.Button("Note", GUILayout.Width(40f)))
						{
							noteWindow = NoteWindow.Open();
							noteWindow.ShowText(path, data.note, UpdateText);
						}
						if (GUILayout.Button("X", GUILayout.Width(20f)))
						{
							checkRemoveID = id;
						}
					}

					postItDatas[path] = data;
				}
				EditorGUILayout.EndHorizontal();
			}

			if (!string.IsNullOrEmpty(removePath))
			{
				pathHashes.Remove(removePath);
				postItDatas.Remove(removePath);
				if (noteWindow != null)
				{
					noteWindow.Clear(removePath);
				}
			}
			EditorGUILayout.EndScrollView();

			if (pos.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.DragUpdated)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					Event.current.Use();
				}
				else if (Event.current.type == EventType.DragPerform)
				{
					for (int i = 0, max = DragAndDrop.objectReferences.Length; i < max; ++i)
					{
						var obj = DragAndDrop.objectReferences[i];
						if (obj)
						{
							var id = obj.GetInstanceID();
							try
							{
								var path = AssetDatabase.GetAssetPath(obj);
								if (!string.IsNullOrEmpty(path) && !pathHashes.Contains(path))
								{
									pathHashes.Add(path);
									postItDatas.Add(path, new PostItData()
									{
										name = string.Empty,
										note = string.Empty
									});
								}
							}
							catch (System.Exception) { }
						}
					}
					Event.current.Use();
				}
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.HelpBox("Drag and drop asset into this window.", MessageType.None);
		}

		void UpdateText(string key, string note)
		{
			if (postItDatas.ContainsKey(key))
			{
				var data = postItDatas[key];
				data.note = note;
				postItDatas[key] = data;
			}
		}
	}
}