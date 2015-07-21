using UnityEngine;
using Assets.Code.Units;
using System;
using System.Collections.Generic;
using Assets.Code.Enums;
using Assets.Code.Behaviors;
using Assets.Code.Controllers;
using Assets.Code.Interfaces;
using System.Collections;
using Assets.Code.Navigation;
using Assets.Code.Scores;
using Assets.Code.GameTypes;
using Assets.Code.GUICode;

public class MainController : MonoBehaviour
{
    private List<IServiceable> needServiced = new List<IServiceable>();
    private bool newWave;

    GameScore gameScore = GameScore.GetInstance();
    GameClock time = GameClock.GetInstance();
    WaveController wave = WaveController.GetInstance();

    public GameTextBoxes txtLives;
    public GameTextBoxes txtResources;
    public GameTextBoxes txtScore;
    public GameTextBoxes txtWaves;
    public GameTextBoxes txtTime;
    public GameTextBoxes txtNext;

    // Use this for initialization
    void Start()
    {
        //This should already be created with the loading of the map, but for testing purposes it is here
        AbstractGameStrategy strat = new EndlessGameStrategy();
        gameScore.Initialize(20, 500, strat);

        GameClock.GetInstance().StartClock();

        newWave = true;

        // create our controllers
        CombatController combat = CombatController.Instance;
        NavigationController nav = NavigationController.Instance;
        wave.Initialize(2158569);
        wave.GenerateWave();

        needServiced.Add(combat);
        needServiced.Add(wave);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (IServiceable controller in needServiced)
            controller.Service();

        if (txtLives == null)
            txtLives = GuiAPI.GetTextBox("Lives");
        if (txtNext == null)
            txtNext = GuiAPI.GetTextBox("Next");
        if (txtResources == null)
            txtResources = GuiAPI.GetTextBox("Money");
        if (txtScore == null)
            txtScore = GuiAPI.GetTextBox("Score");
        if (txtTime == null)
            txtTime = GuiAPI.GetTextBox("Time");
        if (txtWaves == null)
            txtWaves = GuiAPI.GetTextBox("Waves");

        txtLives.UpdateText(GameScore.GetInstance().LivesRemaining.ToString());
        txtResources.UpdateText(GameScore.GetInstance().Resources.ToString());
        txtScore.UpdateText(GameScore.GetInstance().TotalScore.ToString());
        txtWaves.UpdateText(GameScore.GetInstance().WavesCompleted.ToString());
        txtTime.UpdateText(GameClock.GetInstance().GetCurrentTimePlayed().ToString());

        GameScore.GetInstance().PassiveMoneyGain();

        if (wave.EnemiesInGame == 0)
        {
            wave.GenerateWave();
            GameClock.GetInstance().PauseClock(PauseTypes.WavePause);
            wave.SetTimeTilNextWave();
        }

        if (CombatController.Instance.CurrentCombatState == CombatState.OutOfCombat)
        {
            if (WaveController.GetInstance().TimeTilNextWave <= 0.0)
            {
                gameScore.IncrementWavesCompleted();
                GameClock.GetInstance().PauseClock(PauseTypes.WavePause);
            }
            else
            {
                newWave = false;
                txtNext.UpdateText("Next wave in: " + (Math.Round(wave.TimeTilNextWave)).ToString());
            }
        }
        else if (!newWave)
        {
            txtNext.UpdateText("");
            newWave = true;
        }
    }
}
