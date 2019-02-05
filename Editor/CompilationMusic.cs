#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

using UnityEditor;
using UnityEditorInternal;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;

public static class CompilationMusic
{
	public static T GetResource<T>(string name)
		where T : Object
	{
		T resource = Resources.Load<T>(name);

		if (Object.Equals(resource, null))
			throw new FileNotFoundException("An " + typeof(T).Name + " file named \"" + name + "\" needs to be located in a Resources folder");

		return resource;
	}

	public static AudioSource FindAudioSource(Scene scene)
	{
		foreach (var gameobject in scene.GetRootGameObjects())
			if (gameobject.name == AudioSourceName)
				return gameobject.GetComponent<AudioSource>();

		return null;
	}

	public const string ScriptName = "Compilation Music";
	public const string AudioSourceName = ScriptName + " Audio";

	[InitializeOnLoadMethod]
	public static void Loaded()
	{
		CompilationPipeline.assemblyCompilationStarted += OnCompilationStart;
	}

	static void OnCompilationStart(string obj)
	{
		var scene = EditorSceneManager.GetSceneByName(string.Empty);
		AudioSource audioSource = null;

		if (scene.isLoaded)
			audioSource = FindAudioSource(scene);
		else
			scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

		if (audioSource == null)
		{
			audioSource = new GameObject(AudioSourceName).AddComponent<AudioSource>();

			EditorSceneManager.MoveGameObjectToScene(audioSource.gameObject, scene);
		}

		if (audioSource.isPlaying)
		{

		}
		else
		{
			var clip = GetResource<AudioClip>(ScriptName + " Loop");

			audioSource.clip = clip;
			audioSource.loop = true;
			audioSource.Play();
		}
	}

	[DidReloadScripts]
	static void OnScriptsReload()
	{
		var scene = EditorSceneManager.GetSceneByName(string.Empty);

		if (scene.isLoaded)
		{
			var audioSource = FindAudioSource(scene);

			if (audioSource != null)
			{
				var clip = GetResource<AudioClip>(ScriptName + " End");

				audioSource.Stop();
				audioSource.PlayOneShot(clip);

				EditorApplication.update -= Update;
				targetTime = (float)EditorApplication.timeSinceStartup + clip.length;
				EditorApplication.update += Update;
			}
		}
	}

	static float targetTime;
	static void Update()
	{
		if (EditorApplication.timeSinceStartup >= targetTime)
			End();
	}

	static void End()
	{
		var scene = EditorSceneManager.GetSceneByName(string.Empty);

		if (scene.isLoaded)
		{
			var audioSource = FindAudioSource(scene);

			if (audioSource != null)
			{
				if (scene.rootCount == 1)
					EditorSceneManager.UnloadSceneAsync(scene);
				else
					GameObject.DestroyImmediate(audioSource.gameObject);
			}

			targetTime = 0f;

			EditorApplication.update -= Update;

		}
	}
}
#endif