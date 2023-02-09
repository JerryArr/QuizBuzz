using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class quizBuzz : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMModSettings ModSettings;
    class Settings
    {
        public int stageTime;
        public string note;
    }
    Settings settings;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public KMSelectable[] buttons;
    public KMSelectable clearButton;
    public KMSelectable deleteButton;
    public KMSelectable enterButton;

    public MeshRenderer inputPlace;
    public MeshRenderer fizzDisplay;
    public MeshRenderer buzzDisplay;
    public MeshRenderer timerDisplay;


    float timeLeft = -100.0f;
    int stageTiming = 20;

    string[] theModules = new string[19]
    {
        "Bases","Cheap Checkout","Connection Check","Cryptography","Fast Math",
        "FizzBuzz","Laundry","LED Encryption","Lightspeed","Marble Tumble",
        "Monsplode, Fight!","Morse Code","Question Mark","Spinning Buttons","Splitting the Loot",
        "Street Fighter","Tax Returns","Web Design","Wire Sequence",
    };
    string[] theAnswers = new string[19]
    {
        "2, 3, 4, 5, 6, 7, 8, 10",
        "250, 394, 397, 498, 797, 946",
        "1, 2, 3, 4, 5, 6, 7",
        "7, 8, 9, 10, 11, 16",
        "13, 15, 31, 36, 40, 41, 46, 47, 72, 73, 76, 93, 99",
        "1, 2, 3, 4, 5, 8",
        "80, 105, 120, 140, 160, 200, 230, 300, 390",
        "2, 3, 4, 5, 6, 7",
        "1, 2, 3, 4, 5, 6, 8, 9",
        "1, 2, 3, 4, 5, 6, 7, 8",
        "1, 2, 3, 4, 5, 6",
        "505, 515, 522, 532, 535, 542, 545, 552, 555, 565, 572, 575, 582, 592, 595, 600",
        "2, 4, 5, 7, 8, 9",
        "5, 6, 7, 8, 9, 10",
        "12, 16, 22, 25, 26, 30",
        "3, 4, 5, 6, 7, 8",
        "81, 478, 599, 736, 932, 1241, 1647",
        "1, 2, 3, 4, 5, 7, 9",
        "1, 2, 4, 6, 8, 9",
    };

    int[] fizzPositions = new int[6] { 20, 20, 20, 20, 20, 20 };
    int[] buzzPositions = new int[4] { 20, 20, 20, 20 };

    int fizzNumber = 0;
    int buzzNumber = 0;
    int fizzRound = 0;
    int buzzRound = 0;
    int curStage = 0;
    int startNumber = -1;

    bool pressedAllowed = false;
    bool isSolved = false;

    string currentInput = "";
    string reasonWhy = "";

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        Init();
        stageTiming = settings.stageTime;
        if (stageTiming == 0)
        {
            stageTiming = 20;
        }
        pressedAllowed = true;

        //curStage = 19;
    }

    void Init()
    {
        determineStartNumber();
        
        doStageChange();
        delegationZone();
        settings = JsonConvert.DeserializeObject<Settings>(ModSettings.Settings);
    }

    void determineStartNumber()
    {
        startNumber = 15 * UnityEngine.Random.Range(1, 6); //generates a random number from 1-5 then multiplies it by 15
        startNumber = startNumber - UnityEngine.Random.Range(1, 10); //subtracts a number from 1-9 from the start number, guaranteeing a fizzbuzz number
        curStage = startNumber - 1; //curStage is incremented by 1 in the stage change function
        timerDisplay.GetComponentInChildren<TextMesh>().text = "-" + startNumber + "-";
        Debug.LogFormat("[Quiz Buzz #{0}] The starting stage for this run is {1}.", _moduleId, startNumber);
    }

    void doSubmit()
    {
        if (pressedAllowed)
        {
            if (curStage % 15 == 0) //fizzbuzz number
            {
                var pieces = theAnswers[fizzNumber].ToLowerInvariant().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var curPiece = 0;
                var itsGood = false;
                var secondPart = "";
                var fizzPiece = 0;
                var goodFizz = -1;
                var goodBuzz = -1;
                reasonWhy = "Strike on stage " + curStage + ": ";
                while (curPiece < pieces.Length && !itsGood)
                {
					if (pieces[curPiece].Length > currentInput.Length) 
						//since potential answers are checked in increasing order, if an answer has more digits than the total string that was input, we can safely skip the rest
					{
						curPiece = pieces.Length;
					}
					else if (pieces[curPiece] == currentInput.Substring(0, pieces[curPiece].Length))
					{
						itsGood = true;
						goodFizz = curPiece;
						secondPart = currentInput.Substring(pieces[curPiece].Length, currentInput.Length - pieces[curPiece].Length);
					}
					else
					{
						curPiece++;
					}

                }
                if (!itsGood)
                {
                    reasonWhy = reasonWhy + "Fizzbuzz input (" + currentInput + ") did not start with a valid answer for the category " + theModules[fizzNumber] + ". ";

                }

                if (itsGood)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (curPiece == fizzPositions[i])
                        {
							//Debug.Log("Whoops");
                            itsGood = false;
                            reasonWhy = reasonWhy + "Fizzbuzz input (" + currentInput + ") started with an answer in a previously used list position for Fizz. You used positions ";
                            for (int j = 0; j < 3; j++)
                            {
								if (fizzPositions[j] == 20)
								{
									j = 4;
								}
								else
								{
									reasonWhy = reasonWhy + (1 + fizzPositions[j]) + ", ";
								}
                                
                            }
								if (fizzPositions[3] == 20)
								{
									reasonWhy = reasonWhy + "and you tried position " + (1 + curPiece) + " again. ";
								}
								else
								{
									reasonWhy = reasonWhy + "and " + (1 + fizzPositions[3]) + ", and you tried position " + (1 + curPiece) + " again. ";
								}
                            i = 4;
                        }
                    }
                    // Now on to the buzz part of the fizzbuzz
                    pieces = theAnswers[buzzNumber].ToLowerInvariant().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    fizzPiece = curPiece;
                    curPiece = 0;
                    itsGood = false;
                    while (curPiece < pieces.Length && !itsGood)
                    {
                        if (pieces[curPiece] == secondPart)
                        {
                            itsGood = true;
                            goodBuzz = curPiece;
                        }
                        else
                        {
                            curPiece++;
                        }
                        if (!itsGood && secondPart == "")
                        {
                            reasonWhy = reasonWhy + "Fizzbuzz input (" + currentInput + ") did not end with a valid answer for the category " + theModules[buzzNumber] + ". ";
                            curPiece = 999;
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (curPiece == buzzPositions[i])
                        {
                            itsGood = false;
                            reasonWhy = reasonWhy + "Fizzbuzz input (" + currentInput + ") ended with an answer in a previously used list position for Buzz. You used position";
							if (buzzPositions[1] == 20)
							{
								reasonWhy = reasonWhy + " " + (buzzPositions[0] + 1) + " and you tried it again. ";
							}
							else
							{
								reasonWhy = reasonWhy + "s " + (buzzPositions[0] + 1) + " and " + (1 + buzzPositions[1]) + ", and you tried position " + (1 + curPiece) + " again. ";
							}
							
                            i = 2;
                        }
                        
                    }
                }


                if (itsGood && reasonWhy == "Strike on stage " + curStage + ": ")
                {
                    if (curStage == startNumber + 9)
                    {
                        timeLeft = -100f;
                        timerDisplay.GetComponentInChildren<TextMesh>().text = "WIN";
                        reasonWhy = "";
                        Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a FizzBuzz stage and you entered {2}, which was in position {3} of the list for {4} and position {5} for the list for {6}.",
                            _moduleId, curStage, currentInput, (fizzPiece + 1), theModules[fizzNumber], (curPiece + 1), theModules[buzzNumber]);
                        doSolve();
                    }
                    else
                    {
                        fizzPositions[fizzRound] = goodFizz;
						//Debug.Log("fizzRound is" + fizzRound + " and goodFizz is " + goodFizz + ".");
                        fizzRound++;

                        buzzPositions[buzzRound] = goodBuzz;
						//Debug.Log("buzzRound is" + buzzRound + " and goodBuzz is " + goodBuzz + ".");
                        buzzRound++;
                        GetComponent<KMAudio>().PlaySoundAtTransform("ding", transform);
                        Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a FizzBuzz stage and you entered {2}, which was in position {3} of the list for {4} and position {5} for the list for {6}.",
                            _moduleId, curStage, currentInput, (fizzPiece + 1), theModules[fizzNumber], (curPiece + 1), theModules[buzzNumber]);
                        _usedIxs[fizzPiece] = true;
                        _usedIxs[curPiece] = true;
                        doStageChange();
                    }
                }
                else
                {
                    doStrike();
					timerDisplay.GetComponentInChildren<TextMesh>().text = "-" + startNumber + "-";
                }
            }
            else if (curStage % 5 == 0)
            {
                var pieces = theAnswers[buzzNumber].ToLowerInvariant().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var curPiece = 0;
                var itsGood = false;
                var goodPiece = -1;
                reasonWhy = "Strike on stage " + curStage + ": ";
                while (curPiece < pieces.Length && !itsGood)
                {
					
					//Debug.LogFormat("Trying to match {0} against {1} as piece number {2}.", currentInput, pieces[curPiece], curPiece + 1);
                    if (pieces[curPiece] == currentInput)
                    {
                        itsGood = true;
                        goodPiece = curPiece;
                    }
                    else
                    {
						
                        curPiece++;
                    }
                }
                if (!itsGood)
                {
                    reasonWhy = reasonWhy + "Buzz input (" + currentInput + ") was not a valid answer for the category " + theModules[buzzNumber] + ". ";
                    curPiece = 999;
                }
                for (int i = 0; i < ((curStage - startNumber) / 5) - 1; i++)
                {
                    if (curPiece == buzzPositions[i])
                    {
                        itsGood = false;
                        reasonWhy = reasonWhy + "Buzz input (" + currentInput + ") was an answer in a previously used list position for Buzz. You used position(s) ";
                        for (int j = 0; j < ((curStage - startNumber) / 5) - 1; j++)
                        {
                            reasonWhy = reasonWhy + (buzzPositions[j] + 1) + " and ";
                        }
                        reasonWhy = reasonWhy + "you tried position " + (1 + curPiece) + " again. ";
                        i = (curStage / 5);
                    }
                }



                if (!itsGood)
                {
                    doStrike();
					timerDisplay.GetComponentInChildren<TextMesh>().text = "-" + startNumber + "-";
                }
                else if (curStage == startNumber + 9)
                {
                    timeLeft = -100f;
                    timerDisplay.GetComponentInChildren<TextMesh>().text = "WIN";
                    reasonWhy = "";
                    Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a Buzz stage and you entered {2}, which was in position {3} of the list for {4}.", _moduleId, curStage,
                     currentInput, (curPiece + 1), theModules[buzzNumber]);
                    doSolve();
                }
                else
                {
                    buzzPositions[buzzRound] = goodPiece;
                    buzzRound++;
                    GetComponent<KMAudio>().PlaySoundAtTransform("ding", transform);
                    Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a Buzz stage and you entered {2}, which was in position {3} of the list for {4}.", _moduleId, curStage,
                        currentInput, (curPiece + 1), theModules[buzzNumber]);
                    _usedIxs[curPiece] = true;
                    doStageChange();
                    
                }



            }
            else if (curStage % 3 == 0)
            {
                var pieces = theAnswers[fizzNumber].ToLowerInvariant().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var curPiece = 0;
                var itsGood = false;
                var goodPiece = -1;
                reasonWhy = "Strike on stage " + curStage + ": ";
                while (curPiece < pieces.Length && !itsGood)
                {
                    if (pieces[curPiece] == currentInput)
                    {
                        itsGood = true;
                        goodPiece = curPiece;
                    }
                    else
                    {
                        curPiece++;
                    }
                }
                if (!itsGood)
                {
                    reasonWhy = reasonWhy + "Fizz input (" + currentInput + ") was not a valid answer for the category " + theModules[fizzNumber] + ". ";
                    curPiece = 999;
                }
                for (int i = 0; i < ((curStage - startNumber) / 3) - 1; i++)
                {
                    if (curPiece == fizzPositions[i])
                    {
                        itsGood = false;
                        reasonWhy = reasonWhy + "Fizz input (" + currentInput + ") was an answer in a previously used list position for Fizz. You used position(s) ";
                        for (int j = 0; j < ((curStage - startNumber) / 3) - 1; j++)
                        {
                            reasonWhy = reasonWhy + (fizzPositions[j] + 1) + " and ";
                        }
                        reasonWhy = reasonWhy + "you tried position " + (1 + curPiece) + " again. ";
                        i = (curStage / 3);
                    }
                }
                if (!itsGood)
                {
                    doStrike();
					timerDisplay.GetComponentInChildren<TextMesh>().text = "-" + startNumber + "-";
                }
                else if (curStage == startNumber + 9)
                {
                    timeLeft = -100f;
                    timerDisplay.GetComponentInChildren<TextMesh>().text = "WIN";
                    reasonWhy = "";
                    Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a Fizz stage and you entered {2}, which was in position {3} of the list for {4}.", _moduleId, curStage,
                        currentInput, (curPiece + 1), theModules[fizzNumber]);
                    doSolve();
                }
                else
                {
                    fizzPositions[fizzRound] = goodPiece;
                    fizzRound++;
                    GetComponent<KMAudio>().PlaySoundAtTransform("ding", transform);
                    Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a Fizz stage and you entered {2}, which was in position {3} of the list for {4}.", _moduleId, curStage,
                        currentInput, (curPiece + 1), theModules[fizzNumber]);
                    _usedIxs[curPiece] = true;
                    doStageChange();
                }
            }

            else
            {

                reasonWhy = "Strike on stage " + curStage + ": ";
                if (currentInput == "" + curStage)
                {
                    //good

                    Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is correct, it was a Number stage and you entered {1}.", _moduleId, curStage);
                    //Debug.LogFormat("[Quiz Buzz #{0}] Stage {1} is a correct Fizz, entered {2}, which was in position {3} if the list for {4}.", _moduleId, curStage);
                    if (curStage == startNumber + 9)
                    {
                        timeLeft = -100f;
                        timerDisplay.GetComponentInChildren<TextMesh>().text = "WIN";
                        reasonWhy = "";
                        doSolve();
                    }
                    else
                    {
                        GetComponent<KMAudio>().PlaySoundAtTransform("ding", transform);
                        doStageChange();
                    }
                    //beep sound


                }
                else
                {
                    reasonWhy = reasonWhy + "Number input for stage " + curStage + " was expected, but you tried " + currentInput + " instead. ";
                    doStrike();
					timerDisplay.GetComponentInChildren<TextMesh>().text = "-" + startNumber + "-";
                    reasonWhy = "";
                }
            }
        }
        

    }
    
    void doSolve()
    {
        isSolved = true;
        pressedAllowed = false;
        timeLeft = -100f;
        Debug.LogFormat("[Quiz Buzz #{0}] That's a solve! Module disarmed!", _moduleId, reasonWhy);
        GetComponent<KMAudio>().PlaySoundAtTransform("win", transform);
        GetComponent<KMBombModule>().HandlePass();
		
                if (Bomb.GetSolvableModuleNames().Where(x => "Souvenir".Contains(x)).Count() > 0)
                {
					fizzDisplay.GetComponentInChildren<TextMesh>().text = "- - -";
					buzzDisplay.GetComponentInChildren<TextMesh>().text = "- - -";					
					inputPlace.GetComponentInChildren<TextMesh>().text = "- - -";					
				}
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Type a number with !{0} (type/t) 12345678. Delete a digit with !{0} (delete/del/d). Clear with !{0} (clear/c). Enter with (enter/e). Type then enter with (typeenter/te) 12345678";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (stageTiming < 45)
        {
            stageTiming = 45;
        }
        var pieces = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string theError = "";
        yield return null;
        if (pieces.Length == 1)
        {
            if (pieces[0] == "type" || pieces[0] == "t")                
            {
                theError = "sendtochaterror Missing argument! You must type a 1 to 8 digit number after 'type'.";
                yield return theError;
            }
            else if (pieces[0] == "typeenter" || pieces[0] == "te")
            {
                theError = "sendtochaterror Missing argument! You must type a 1 to 8 digit number after 'typeenter'.";
                yield return theError;
            }

            else if (pieces[0] == "delete" || pieces[0] == "d" || pieces[0] == "del")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                deleteButton.OnInteract();
            }
            else if (pieces[0] == "clear" || pieces[0] == "c" || pieces[0] == "clr")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                clearButton.OnInteract();
            }
            else if (pieces[0] == "enter" || pieces[0] == "e" || pieces[0] == "s" || pieces[0] == "submit")
            {
                if (currentInput == "" || currentInput == null)
                {
                    theError = "sendtochaterror Nothing to enter, so enter not pressed!";
                    yield return theError;
                }
                else
                {
                    yield return new WaitForSeconds(.1f);
                    yield return null;
                    enterButton.OnInteract();
                }

            }
            else
            {
                theError = "sendtochaterror Invalid command! " + pieces[0] + " is not a valid command, please use type, delete, clear, enter or typeenter.";
                yield return theError;
            }
        }

        else if (pieces.Length > 1)
        {
            if (pieces[0] == "type" || pieces[0] == "t")
            {
                if (pieces[1].Length > 8)
                {

                    theError = "sendtochaterror Invalid length! You don't need any more than eight digits.";
                    yield return theError;
                }
                for (int k = 0; k < pieces[1].Length; k++)
                {
                    Debug.Log(pieces[1].Substring(k, 1));
                    if (pieces[1].Substring(k, 1) != "0" && pieces[1].Substring(k, 1) != "1" && pieces[1].Substring(k, 1) != "2" && pieces[1].Substring(k, 1) != "3" &&
                        pieces[1].Substring(k, 1) != "4" && pieces[1].Substring(k, 1) != "5" && pieces[1].Substring(k, 1) != "6" && pieces[1].Substring(k, 1) != "7" &&
                        pieces[1].Substring(k, 1) != "8" && pieces[1].Substring(k, 1) != "9")
                    {

                        theError = "sendtochaterror Invalid character! " + pieces[1].Substring(k, 1) + " is not a digit.";
                        yield return theError;
                    }
                }
                if (theError == "")
                {
                    for (int l = 0; l < pieces[1].Length; l++)
                    {
                        var curDigit = Int16.Parse(pieces[1].Substring(l, 1));
                        yield return new WaitForSeconds(.1f);
                        yield return null;
                        buttons[curDigit].OnInteract();
                    }

                }
            }
            else if (pieces[0] == "typeenter" || pieces[0] == "te")
            {
                if (pieces[1].Length > 8)
                {

                    theError = "sendtochaterror Invalid length! You don't need any more than eight digits.";
                    yield return theError;
                }
                for (int k = 0; k < pieces[1].Length; k++)
                {
                    Debug.Log(pieces[1].Substring(k, 1));
                    if (pieces[1].Substring(k, 1) != "0" && pieces[1].Substring(k, 1) != "1" && pieces[1].Substring(k, 1) != "2" && pieces[1].Substring(k, 1) != "3" &&
                        pieces[1].Substring(k, 1) != "4" && pieces[1].Substring(k, 1) != "5" && pieces[1].Substring(k, 1) != "6" && pieces[1].Substring(k, 1) != "7" &&
                        pieces[1].Substring(k, 1) != "8" && pieces[1].Substring(k, 1) != "9")
                    {

                        theError = "sendtochaterror Invalid character! " + pieces[1].Substring(k, 1) + " is not a digit.";
                        yield return theError;
                    }
                }
                if (theError == "")
                {
                    for (int l = 0; l < pieces[1].Length; l++)
                    {
                        var curDigit = Int16.Parse(pieces[1].Substring(l, 1));
                        yield return new WaitForSeconds(.1f);
                        yield return null;
                        buttons[curDigit].OnInteract();
                    }

                }
                yield return new WaitForSeconds(.1f);
                yield return null;
                enterButton.OnInteract();
                enterButton.OnInteractEnded();
            }
                        else if (pieces[0] == "delete" || pieces[0] == "d" || pieces[0] == "del")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                deleteButton.OnInteract();
            }
            else if (pieces[0] == "clear" || pieces[0] == "c" || pieces[0] == "clr")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                clearButton.OnInteract();
            }
            else if (pieces[0] == "enter" || pieces[0] == "e" || pieces[0] == "s" || pieces[0] == "submit")
            {
                yield return new WaitForSeconds(.1f);
                yield return null;
                enterButton.OnInteract();
                enterButton.OnInteractEnded();
            }
            else
            {
                theError = "sendtochaterror Invalid command! " + pieces[0] + " is not a valid command, please use type, delete, clear, or enter.";
                yield return theError;
            }

        }
        else
        {
            theError = "sendtochaterror No command entered! Please use type, delete, clear, or enter.";
            yield return theError;
        }


       
    }

    void FixedUpdate()
    {

        if (timeLeft >= 0)
        {
            timeLeft -= Time.fixedDeltaTime;
            var timeString = "";
            if (timeLeft <= 0)
            {
                timeString = "---";
                reasonWhy = "Time ran out. ";
                doStrike();
				timeString = "-" + startNumber + "-";
            }
            else if (timeLeft < 10)
            {
                timeString = Math.Floor(timeLeft) + "." + ((int)(timeLeft * 10) % 10);
            }
            else
            {
                timeString = Math.Floor(timeLeft) + "";
            }


            timerDisplay.GetComponentInChildren<TextMesh>().text = timeString;
        }
        else if (curStage == startNumber)
        {
            //keep timer at "---"
        }
        else //time is up!!!!! Or we solved the module which is fine because the strike only fires in the associated function if the module is not solved yet
        {
            doStrike();
        }

    }

    void doStrike()
    {
        if (!isSolved)
        {
            _usedIxs = new bool[16];
            fizzRound = 0;
            buzzRound = 0;
            for (int i = 0; i < 4; i++)
            {
                fizzPositions[i] = 20;
                buzzPositions[i] = 20;
            }
            fizzPositions[4] = 20;
            fizzPositions[5] = 20;

            Debug.LogFormat("[Quiz Buzz #{0}] {1}Strike given!", _moduleId, reasonWhy);
            determineStartNumber();
            doStageChange();
            timeLeft = -100f;
            GetComponent<KMAudio>().PlaySoundAtTransform("buzz", transform);
            GetComponent<KMBombModule>().HandleStrike();
        }

    }

    void doStageChange()
    {
        var stringX = "Used are F ";
        for (int i = 0; i < 6; i++)
        {
            stringX = stringX + fizzPositions[i] + " ";
        }
        stringX = stringX + "B ";
        for (int i = 0; i < 4; i++)
        {
            stringX = stringX + buzzPositions[i] + " ";
        }
        //Debug.Log(stringX);
        curStage++;
        if (curStage > startNumber)
        {
            timeLeft = stageTiming;
        }
        //Debug.Log("CurStage is " + curStage + " and StartStage is " + startNumber + ".");
        doClear();
        fizzNumber = UnityEngine.Random.Range(0, 19);
        buzzNumber = (UnityEngine.Random.Range(1, 19) + fizzNumber) % 19;
        fizzDisplay.GetComponentInChildren<TextMesh>().text = theModules[fizzNumber];
        buzzDisplay.GetComponentInChildren<TextMesh>().text = theModules[buzzNumber];
        
    }


    void doClear()
    {
        currentInput = "";
        inputPlace.GetComponentInChildren<TextMesh>().text = "";
    }

    void doNumber(int n)
    {
        currentInput = currentInput + "" + n;
        if (currentInput.Length > 8)
        {
            currentInput = currentInput.Substring(1, 8);
        }
        inputPlace.GetComponentInChildren<TextMesh>().text = currentInput;
    }

    void doDelete()
    {
        if (currentInput.Length > 0)
        { 
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            inputPlace.GetComponentInChildren<TextMesh>().text = currentInput;
        }
    }

    void OnPress()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
    }

    void OnRelease()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if (pressedAllowed)
        {

            return;
        }

    }

    void delegationZone()
    {

        buttons[0].OnInteract += delegate () { OnPress(); doNumber(0); buttons[0].AddInteractionPunch(0.2f); return false; };
        buttons[1].OnInteract += delegate () { OnPress(); doNumber(1); buttons[1].AddInteractionPunch(0.2f); return false; };
        buttons[2].OnInteract += delegate () { OnPress(); doNumber(2); buttons[2].AddInteractionPunch(0.2f); return false; };
        buttons[3].OnInteract += delegate () { OnPress(); doNumber(3); buttons[3].AddInteractionPunch(0.2f); return false; };
        buttons[4].OnInteract += delegate () { OnPress(); doNumber(4); buttons[4].AddInteractionPunch(0.2f); return false; };
        buttons[5].OnInteract += delegate () { OnPress(); doNumber(5); buttons[5].AddInteractionPunch(0.2f); return false; };
        buttons[6].OnInteract += delegate () { OnPress(); doNumber(6); buttons[6].AddInteractionPunch(0.2f); return false; };
        buttons[7].OnInteract += delegate () { OnPress(); doNumber(7); buttons[7].AddInteractionPunch(0.2f); return false; };
        buttons[8].OnInteract += delegate () { OnPress(); doNumber(8); buttons[8].AddInteractionPunch(0.2f); return false; };
        buttons[9].OnInteract += delegate () { OnPress(); doNumber(9); buttons[9].AddInteractionPunch(0.2f); return false; };

        clearButton.OnInteract += delegate () {
            OnPress(); doClear();
            clearButton.AddInteractionPunch(0.2f); return false;
        };
        enterButton.OnInteract += delegate () { OnPress(); doSubmit(); enterButton.AddInteractionPunch(0.4f); return false; };
        deleteButton.OnInteract += delegate () { OnPress(); doDelete(); deleteButton.AddInteractionPunch(0.2f); return false; };

        buttons[0].OnInteractEnded += delegate () { OnRelease(); };
        buttons[1].OnInteractEnded += delegate () { OnRelease(); };
        buttons[2].OnInteractEnded += delegate () { OnRelease(); };
        buttons[3].OnInteractEnded += delegate () { OnRelease(); };
        buttons[4].OnInteractEnded += delegate () { OnRelease(); };
        buttons[5].OnInteractEnded += delegate () { OnRelease(); };
        buttons[6].OnInteractEnded += delegate () { OnRelease(); };
        buttons[7].OnInteractEnded += delegate () { OnRelease(); };
        buttons[8].OnInteractEnded += delegate () { OnRelease(); };
        buttons[9].OnInteractEnded += delegate () { OnRelease(); };

        clearButton.OnInteractEnded += delegate () { OnRelease(); };
        enterButton.OnInteractEnded += delegate () { OnRelease(); };
        deleteButton.OnInteractEnded += delegate () { OnRelease(); };


    }

    // Implemented by Quinn Wuest
    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!isSolved)
        {
            string sol = "";
            int index = Array.IndexOf(_usedIxs, false);
            var a = _nums[fizzNumber];
            var b = _nums[buzzNumber];
            if (curStage % 3 == 0)
            {
                sol += _nums[fizzNumber][index].ToString();
                _usedIxs[index] = true;
                while (_usedIxs[index])
                    index = (index + 1) % _usedIxs.Length;
            }
            if (curStage % 5 == 0)
                sol += _nums[buzzNumber][index].ToString();
            if (sol == "")
                sol = curStage.ToString();
            if (!sol.StartsWith(currentInput))
            {
                clearButton.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            int start = 0;
            for (int i = 0; i < currentInput.Length; i++)
                if (currentInput[i] == sol[i])
                    start++;
            for (int i = start; i < sol.Length; i++)
            {
                buttons[sol[i] - '0'].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            enterButton.OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
        yield break;
    }

    private bool[] _usedIxs = new bool[16];
    private static readonly int[][] _nums = new int[][]
    {
        new int[] {2, 3, 4, 5, 6, 7, 8, 10},
        new int[] {250, 394, 397, 498, 797, 946},
        new int[] {1, 2, 3, 4, 5, 6, 7},
        new int[] {7, 8, 9, 10, 11, 16},
        new int[] {13, 15, 31, 36, 40, 41, 46, 47, 72, 73, 76, 93, 99},
        new int[] {1, 2, 3, 4, 5, 8},
        new int[] {80, 105, 120, 140, 160, 200, 230, 300, 390},
        new int[] {2, 3, 4, 5, 6, 7},
        new int[] {1, 2, 3, 4, 5, 6, 8, 9},
        new int[] {1, 2, 3, 4, 5, 6, 7, 8},
        new int[] {1, 2, 3, 4, 5, 6},
        new int[] {505, 515, 522, 532, 535, 542, 545, 552, 555, 565, 572, 575, 582, 592, 595, 600},
        new int[] {2, 4, 5, 7, 8, 9},
        new int[] {5, 6, 7, 8, 9, 10},
        new int[] {12, 16, 22, 25, 26, 30},
        new int[] {3, 4, 5, 6, 7, 8},
        new int[] {81, 478, 599, 736, 932, 1241, 1647},
        new int[] {1, 2, 3, 4, 5, 7, 9},
        new int[] {1, 2, 4, 6, 8, 9},
    };
}