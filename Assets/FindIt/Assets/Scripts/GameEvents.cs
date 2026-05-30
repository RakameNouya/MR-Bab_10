using UnityEngine;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Quiz/new GameEvents")]
public class GameEvents : ScriptableObject {

    public delegate void    UpdateQuestionUICallback            (Question question);
    public                  UpdateQuestionUICallback            UpdateQuestionUI                = null;

    public delegate void    UpdateQuestionAnswerCallback        (AnswerData pickedAnswer);
    public                  UpdateQuestionAnswerCallback        UpdateQuestionAnswer            = null;

    public delegate void    DisplayResolutionScreenCallback     (UIManager.ResolutionScreenType type, int score);
    public                  DisplayResolutionScreenCallback     DisplayResolutionScreen         = null;

    public delegate void    ScoreUpdatedCallback();
    public                  ScoreUpdatedCallback                ScoreUpdated                    = null;

    public delegate void    CheckAnswerCallback();
    public                  CheckAnswerCallback                 CheckAnswer                     = null;

    public delegate void    RestartUICallback();
    public                  RestartUICallback                   RestartUI                       = null;

    public delegate void    RequestRestartCallback();
    public                  RequestRestartCallback              RequestRestart                  = null;

    [HideInInspector]
    public                  int                                 CurrentFinalScore               = 0;
    [HideInInspector]
    public                  int                                 StartupHighscore                = 0;
}