using UnityEngine;
using UnityEditor;

namespace PostIt
{
	public class NoteWindow : EditorWindow
	{
		public static NoteWindow Open()
		{
			var win = GetWindow<NoteWindow>("Note");
			return win;
		}

		public void ShowText(string key, string text, System.Action<string, string> onChange)
		{
			this.key = key;
			this.text = text;
			this.onChange = onChange;
			canEdit = true;
		}

		public void Clear(string key)
		{
			if (this.key == key)
			{
				this.key = null;
				text = null;
				canEdit = false;
			}
			Repaint();
		}

		System.Action<string, string> onChange;
		bool canEdit;
		string key;
		string text;
		bool editMode;
		GUIStyle labelStyle;
		private void OnGUI()
		{
			if (canEdit)
			{
				// tool bar
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				editMode = GUILayout.Toggle(editMode, "edit", EditorStyles.toolbarButton, GUILayout.Width(40f));
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			else
				editMode = false;

			if (!string.IsNullOrEmpty(key))
			{
				var Object = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(key);
				EditorGUILayout.ObjectField(Object, typeof(UnityEngine.Object), true);
			}

			// text area
			if (editMode)
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					text = EditorGUILayout.TextArea(text, GUILayout.Height(position.height - 43f));
					if (check.changed)
					{
						if (onChange != null)
							onChange(key, text);
					}
				}
			}
			else if (text == null)
			{
				EditorGUILayout.HelpBox("Select a note.", MessageType.Warning);
			}
			else if (string.IsNullOrEmpty(text))
			{
				EditorGUILayout.HelpBox("Note is empty, click edit button to edit.", MessageType.Info);
			}
			else
			{
				if (labelStyle == null) labelStyle = new GUIStyle(GUI.skin.box);
				labelStyle.normal.textColor = Color.white * 0.8f;
				GUILayout.Box(text, labelStyle, GUILayout.Height(position.height - 43f), GUILayout.Width(position.width - 6f));
			}
		}
	}
}