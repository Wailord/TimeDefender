/***********************************************************
* Primary author:           Luke Thompson
* Filename:               	SoundController.cs
*
* Overview:
* 	This sound controller class can be used to play audio 
*   files. To play a sound effect, simply call the Play()
*   function and pass as a parameter a sound category and
*   an optional sound ID (0-2). If a sound ID is not passed,
*   a random sound will be played. Sound categories are as 
*   follows:
*       public enum sounds
*           bg                      Background music/ambience
*           menu                    Menu sounds (clicking)
*           spawn                   Enemy spawn sounds
*           death                   Enemy death sounds
*           tower                   Tower construction/upgrade sounds
*           light_projectile        rocks, arrows, and other launched projectiles
*           light_gun               Pistols, rifles, and machine guns
*           heavy_gun               Cannons and artillery
*          
************************************************************/

using UnityEngine;

namespace Assets.Code.Sound
{
	public enum sounds { bg = 0, menu, spawn, death, tower, light_projectile, gun, laser };

	public class SoundPlayer
	{
		private static SoundController instance = null;

		private SoundPlayer(){}

		public static SoundController getInstance()
		{
			if (instance == null) {
				instance = new SoundController ();
			}
			return instance;
		}
	}

	public class SoundController
	{
		const int category_count = 9;
		//public bool[] Playing = new bool[category_count]; //indicates whether a given category has a sound playing
		private AudioClip[,] Sounds = new AudioClip[category_count, 3];
		private AudioSource[] Player = new AudioSource[category_count];
		private System.Random r = new System.Random();

		bool music_en = true;
		int current_track;

		public GameObject SoundPlayer = new GameObject();

		#region ctor

		public SoundController()
		{
			for (int i = 0; i < category_count; i++) {
				Player [i] = SoundPlayer.AddComponent<AudioSource> ();
			}
			
			Sounds [(int)sounds.menu, 0] = (AudioClip)Resources.Load ("Audio/Click1");
			Sounds [(int)sounds.menu, 1] = (AudioClip)Resources.Load ("Audio/Click2");
			Sounds [(int)sounds.menu, 2] = (AudioClip)Resources.Load ("Audio/Click3");
					
			Sounds [(int)sounds.bg, 0] = (AudioClip)Resources.Load ("Audio/InfernalGalop");
			Sounds [(int)sounds.bg, 1] = (AudioClip)Resources.Load ("Audio/1812Overture");
			Sounds [(int)sounds.bg, 2] = (AudioClip)Resources.Load ("Audio/WilliamTellOverture");
					
			Sounds [(int)sounds.death, 0] = (AudioClip)Resources.Load ("Audio/Death1");
			Sounds [(int)sounds.death, 1] = (AudioClip)Resources.Load ("Audio/Death2");
			Sounds [(int)sounds.death, 2] = (AudioClip)Resources.Load ("Audio/Death3");
					
			Sounds [(int)sounds.spawn, 0] = (AudioClip)Resources.Load ("Audio/Teleport1");
			Sounds [(int)sounds.spawn, 1] = (AudioClip)Resources.Load ("Audio/Teleport2");
			Sounds [(int)sounds.spawn, 2] = (AudioClip)Resources.Load ("Audio/Teleport3");
					
			Sounds [(int)sounds.tower, 0] = (AudioClip)Resources.Load ("Audio/Construction1");
			Sounds [(int)sounds.tower, 1] = (AudioClip)Resources.Load ("Audio/Construction2");
			Sounds [(int)sounds.tower, 2] = (AudioClip)Resources.Load ("Audio/Construction3");
					
			Sounds [(int)sounds.light_projectile, 0] = (AudioClip)Resources.Load ("Audio/LightProjectile1");
			Sounds [(int)sounds.light_projectile, 1] = (AudioClip)Resources.Load ("Audio/LightProjectile2");
			Sounds [(int)sounds.light_projectile, 2] = (AudioClip)Resources.Load ("Audio/LightProjectile3");

			Sounds [(int)sounds.gun, 0] = (AudioClip)Resources.Load ("Audio/Gun1");
			Sounds [(int)sounds.gun, 1] = (AudioClip)Resources.Load ("Audio/Gun2");
			Sounds [(int)sounds.gun, 2] = (AudioClip)Resources.Load ("Audio/Gun3");

			Sounds [(int)sounds.laser, 0] = (AudioClip)Resources.Load ("Audio/Laser1");
			Sounds [(int)sounds.laser, 1] = (AudioClip)Resources.Load ("Audio/Laser2");
			Sounds [(int)sounds.laser, 2] = (AudioClip)Resources.Load ("Audio/Laser3");

			current_track = r.Next (0, 3);
		}

		#endregion
		
		//Plays the chosen sound. If a soundID is not specified, a random
		//sound will play from the chosen category.
		public void Play(sounds soundtype)
		{
			if (!Player [(int)soundtype].isPlaying)
			{
				Player [(int)soundtype].clip = Sounds [(int)soundtype, r.Next (0, 3)];
				Player [(int)soundtype].Play ();
			}

			if (music_en)
				MusicCheck ();
			else
				StopPlaying (sounds.bg);
		}

		public void MusicCheck()
		{
			if (!Player [(int)sounds.bg].isPlaying) 
			{
				++current_track;
				if (current_track > 2) 
					current_track = 0;
				Player [(int)sounds.bg].clip = Sounds [(int)sounds.bg, current_track];
				Player [(int)sounds.bg].Play ();
			}
		}
		
		//Stops a sound from playing, particularly useful for turning music off
		public void StopPlaying(sounds soundtype)
		{
			Player [(int)soundtype].Stop ();
		}
	}
}
