﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class OneLinksToAllScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable moduleSelectable;
    public KMSelectable[] buttons;
    public Text[] texts;

    private List<string> explicitTerms = new List<string>();
    private List<string> exceptions = new List<string>();

    private List<string> queryLinks = new List<string>();
    private List<string> exampleSolution = new List<string>();
    private int repeats = 0;
    private int curCount = 0;
    private string queryCheckBackURL = "http://en.wikipedia.org/w/api.php?action=query&format=json&prop=linkshere&lhprop=title&lhlimit=max&lhnamespace=0";
    private string queryGetRandomURL = "https://en.wikipedia.org/w/api.php?action=query&format=json&list=random&rnlimit=1&rnnamespace=0";
    private string queryLeadsToURL = "http://en.wikipedia.org/w/api.php?action=query&format=json&prop=links&pllimit=max&plnamespace=0";
    private string queryRedirectCheck = "https://en.wikipedia.org/w/api.php?action=query&prop=revisions&rvslots=*&rvprop=content&formatversion=2";
    private string title1 = "";
    private string title2 = "";
    private string temp = "";
    private string enteredNums = "";
    private string contvar;
    private bool error = false;
    private bool activated = false;
    private bool getTerms = false;
    private bool focused = false;
    private bool caps = false;
    private bool nums = false;
    private bool alt = false;

    private List<string> addedArticles = new List<string>();
    private int curIndex = 0;

    private char[] keySet1 = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private char[] keySet2 = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
    private char[] keySet3 = new char[] { 'á', 'é', 'í', 'ó', 'ú', 'à', 'è', 'ì', 'ò', 'ù', 'ä', 'ë', 'ï', 'ö', 'ü', 'ā', 'ē', 'ī', 'ō', 'ū', 'ã', 'ñ', 'õ', 'â', 'ê', 'ô' };
    private char[] keySet4 = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '␣', '(', ')', '\'', '.', ',', '–', '-', ':', '&', '/', ' ', ' ', ' ', ' ', ' ' };
    private char[] keySetSolve = new char[] { 'C', 'O', 'N', 'G', 'R', 'A', 'T', 'U', 'L', 'A', 'T', 'I', 'O', 'N', 'S', 'Y', 'O', 'U', 'R', 'E', 'D', 'O', 'N', 'E', '!', ' ' };
    private int keyIndex = 0;
    private int submit = -1;

    private Coroutine load;
    private Coroutine loadlinks;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private OneLinksToAllSettings Settings = new OneLinksToAllSettings();

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        ModConfig<OneLinksToAllSettings> modConfig = new ModConfig<OneLinksToAllSettings>("OneLinksToAllSettings");
        //Read from the settings file, or create one if one doesn't exist
        Settings = modConfig.Settings;
        //Update the settings file incase there was an error during read
        modConfig.Settings = Settings;
        Debug.LogFormat("[One Links To All #{0}] Explicit Filter: {1}", moduleId, Settings.disableExplicitContent ? "On" : "Off");
        if (Settings.disableExplicitContent)
        {
            StartCoroutine(FillCensoringLists());
        }
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        if (Application.isEditor)
        {
            focused = true;
        }
        moduleSelectable.OnFocus += delegate () { focused = true; };
        moduleSelectable.OnDefocus += delegate () { focused = false; };
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        activated = true;
        if (!error)
        {
            load = StartCoroutine(Loading(0));
            StartCoroutine(QueryProcess());
        }
        else
        {
            texts[0].text = "Error: Failed to grab explicit terms and exceptions!";
            texts[2].text = "Error: Failed to grab explicit terms and exceptions!";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            if (!caps) caps = true;
            else caps = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            caps = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            caps = false;
        }
        if (Input.GetKeyDown(KeyCode.Numlock))
        {
            if (!nums) nums = true;
            else nums = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            alt = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            alt = false;
        }
        if (moduleSolved != true && load == null && activated && focused)
        {
            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                if (enteredNums == "0225" || enteredNums == "160")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[5].OnInteract();
                }
                else if (enteredNums == "0233" || enteredNums == "130")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[6].OnInteract();
                }
                else if (enteredNums == "0237" || enteredNums == "161")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[7].OnInteract();
                }
                else if (enteredNums == "0243" || enteredNums == "162")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[8].OnInteract();
                }
                else if (enteredNums == "0250" || enteredNums == "163")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[9].OnInteract();
                }
                else if (enteredNums == "0224" || enteredNums == "133")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[10].OnInteract();
                }
                else if (enteredNums == "0232" || enteredNums == "138")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[11].OnInteract();
                }
                else if (enteredNums == "0236" || enteredNums == "141")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[12].OnInteract();
                }
                else if (enteredNums == "0242" || enteredNums == "149")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[13].OnInteract();
                }
                else if (enteredNums == "0249" || enteredNums == "151")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[14].OnInteract();
                }
                else if (enteredNums == "0228" || enteredNums == "132")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[15].OnInteract();
                }
                else if (enteredNums == "0235" || enteredNums == "137")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[16].OnInteract();
                }
                else if (enteredNums == "0239" || enteredNums == "139")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[17].OnInteract();
                }
                else if (enteredNums == "0246" || enteredNums == "148")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[18].OnInteract();
                }
                else if (enteredNums == "0252" || enteredNums == "129")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[19].OnInteract();
                }
                else if (enteredNums == "0257")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[20].OnInteract();
                }
                else if (enteredNums == "0275")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[21].OnInteract();
                }
                else if (enteredNums == "0299")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[22].OnInteract();
                }
                else if (enteredNums == "0333")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[23].OnInteract();
                }
                else if (enteredNums == "0263")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[24].OnInteract();
                }
                else if (enteredNums == "0227")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[25].OnInteract();
                }
                else if (enteredNums == "0241" || enteredNums == "164")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[26].OnInteract();
                }
                else if (enteredNums == "0245")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[27].OnInteract();
                }
                else if (enteredNums == "0226" || enteredNums == "131")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[28].OnInteract();
                }
                else if (enteredNums == "0234" || enteredNums == "136")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[29].OnInteract();
                }
                else if (enteredNums == "0244" || enteredNums == "147")
                {
                    if (keyIndex != 2) buttons[33].OnInteract();
                    buttons[30].OnInteract();
                }
                else if (enteredNums == "0151")
                {
                    if (keyIndex != 3) buttons[34].OnInteract();
                    buttons[21].OnInteract();
                }
                enteredNums = "";
                return;
            }
            if (caps && Input.GetKeyDown(KeyCode.A))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[5].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.B))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[6].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.C))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[7].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.D))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[8].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.E))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[9].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.F))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[10].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.G))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[11].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.H))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[12].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.I))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[13].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.J))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[14].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.K))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[15].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.L))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[16].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.M))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[17].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.N))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[18].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.O))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[19].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.P))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[20].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Q))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[21].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.R))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[22].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.S))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[23].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.T))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[24].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.U))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[25].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.V))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[26].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.W))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[27].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.X))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[28].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Y))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[29].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Z))
            {
                if (keyIndex != 0) buttons[31].OnInteract();
                buttons[30].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.A))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[5].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.B))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[6].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.C))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[7].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.D))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[8].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.E))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[9].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.F))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[10].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.G))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[11].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.H))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[12].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.I))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[13].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.J))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[14].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.K))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[15].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.L))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[16].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.M))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[17].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.N))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[18].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.O))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[19].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.P))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[20].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Q))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[21].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.R))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[22].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.S))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[23].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.T))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[24].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.U))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[25].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.V))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[26].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.W))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[27].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.X))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[28].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Y))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[29].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Z))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[30].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Z))
            {
                if (keyIndex != 1) buttons[32].OnInteract();
                buttons[30].OnInteract();
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad0))
            {
                enteredNums += "0";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad1))
            {
                enteredNums += "1";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad2))
            {
                enteredNums += "2";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad3))
            {
                enteredNums += "3";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad4))
            {
                enteredNums += "4";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad5))
            {
                enteredNums += "5";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad6))
            {
                enteredNums += "6";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad7))
            {
                enteredNums += "7";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad8))
            {
                enteredNums += "8";
            }
            else if (alt && Input.GetKeyDown(KeyCode.Keypad9))
            {
                enteredNums += "9";
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad0)) || (!caps && Input.GetKeyDown(KeyCode.Alpha0)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[5].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad1)) || (!caps && Input.GetKeyDown(KeyCode.Alpha1)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[6].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad2)) || (!caps && Input.GetKeyDown(KeyCode.Alpha2)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[7].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad3)) || (!caps && Input.GetKeyDown(KeyCode.Alpha3)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[8].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad4)) || (!caps && Input.GetKeyDown(KeyCode.Alpha4)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[9].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad5)) || (!caps && Input.GetKeyDown(KeyCode.Alpha5)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[10].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad6)) || (!caps && Input.GetKeyDown(KeyCode.Alpha6)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[11].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad7)) || (!caps && Input.GetKeyDown(KeyCode.Alpha7)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[12].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad8)) || (!caps && Input.GetKeyDown(KeyCode.Alpha8)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[13].OnInteract();
            }
            else if ((nums && Input.GetKeyDown(KeyCode.Keypad9)) || (!caps && Input.GetKeyDown(KeyCode.Alpha9)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[14].OnInteract();
            }
            else if (Input.GetKeyDown(KeyCode.Space) && texts[1].text.Trim() != "")
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[15].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Alpha9))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[16].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[17].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Quote))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[18].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Period))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[19].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Comma))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[20].OnInteract();
            }
            else if (!caps && (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[22].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Semicolon))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[23].OnInteract();
            }
            else if (caps && Input.GetKeyDown(KeyCode.Alpha7))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[24].OnInteract();
            }
            else if (!caps && Input.GetKeyDown(KeyCode.Slash))
            {
                if (keyIndex != 3) buttons[34].OnInteract();
                buttons[25].OnInteract();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) && !texts[1].text.Equals(""))
            {
                buttons[2].OnInteract();
            }
            else if (((!nums && Input.GetKeyDown(KeyCode.KeypadPeriod)) || Input.GetKeyDown(KeyCode.Delete)) && !texts[1].text.Equals(""))
            {
                buttons[3].OnInteract();
            }
            else if ((!nums && Input.GetKeyDown(KeyCode.Keypad8)) || Input.GetKeyDown(KeyCode.UpArrow) && !texts[1].text.Equals(""))
            {
                buttons[0].OnInteract();
            }
            else if (((!nums && Input.GetKeyDown(KeyCode.Keypad2)) || Input.GetKeyDown(KeyCode.DownArrow)) && curIndex != 0)
            {
                buttons[1].OnInteract();
            }
            else if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && Valid())
            {
                buttons[4].OnInteract();
            }
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if ((moduleSolved != true && load == null && activated && !error) || (moduleSolved != true && pressed == buttons[4] && error))
        {
            if (pressed == buttons[0] && !texts[1].text.Equals("") && submit == -1)
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                addedArticles.Add(texts[1].text);
                curIndex++;
                texts[1].text = "";
                texts[3].text = (curIndex + 1).ToString();
            }
            else if (pressed == buttons[1] && curIndex != 0 && submit == -1)
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = addedArticles[curIndex - 1];
                addedArticles.RemoveAt(curIndex - 1);
                curIndex--;
                texts[3].text = (curIndex + 1).ToString();
            }
            else if (pressed == buttons[2] && !texts[1].text.Equals("") && submit == -1)
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = texts[1].text.Substring(0, texts[1].text.Length-1);
            }
            else if (pressed == buttons[3] && !texts[1].text.Equals("") && submit == -1)
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = "";
            }
            else if (pressed == buttons[4] && Valid())
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                if (error)
                {
                    Debug.LogFormat("[One Links To All #{0}] Submit has been pressed, module disarmed!", moduleId);
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                    return;
                }
                if (submit == -1)
                {
                    Debug.LogFormat("[One Links To All #{0}] ==Submitted Path==", moduleId);
                    temp = texts[1].text;
                    texts[3].text = "";
                    if (texts[1].text == "" && addedArticles.Count == 0)
                    {
                        StartCoroutine(noSub());
                    }
                    else
                    {
                        StartCoroutine(finalCheck());
                    }
                }
                else if (submit == 0)
                {
                    Debug.LogFormat("[One Links To All #{0}] Submitted path is valid, module disarmed!", moduleId);
                    moduleSolved = true;
                    audio.PlaySoundAtTransform("solve", transform);
                    for (int i = 5; i < 31; i++)
                    {
                        if (i == 11)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 14)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                        }
                        else if (i == 15)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 16)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                        }
                        else if (i == 17)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                        }
                        else if (i == 20)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 21)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 22)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 23)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 27)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                        }
                        else if (i == 29)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        }
                        else if (i == 30)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                            buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                        }
                        if (i == 30)
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().text = ":)";
                        }
                        else
                        {
                            buttons[i].GetComponentInChildren<TextMesh>().text = keySetSolve[i - 5].ToString();
                        }
                    }
                    GetComponent<KMBombModule>().HandlePass();
                    texts[0].text = "";
                    texts[1].text = "GG";
                    texts[2].text = "";
                    texts[3].text = "";
                }
                else
                {
                    Debug.LogFormat("[One Links To All #{0}] Submitted path is invalid, Strike!", moduleId);
                    GetComponent<KMBombModule>().HandleStrike();
                    if (submit == 1)
                    {
                        texts[1].text = "";
                        texts[3].text = 1.ToString();
                    }
                    else
                    {
                        texts[1].text = temp;
                        texts[3].text = (curIndex + 1).ToString();
                    }
                    submit = -1;
                }
            }
            else if (pressed == buttons[31] && keyIndex != 0 && submit == -1)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 0;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 22)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 23)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0011f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet1[i - 5].ToString();
                }
            }
            else if (pressed == buttons[32] && keyIndex != 1 && submit == -1)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 1;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 22)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 23)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet2[i - 5].ToString();
                }
            }
            else if (pressed == buttons[33] && keyIndex != 2 && submit == -1)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 2;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 22)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 23)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if(i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet3[i - 5].ToString();
                }
            }
            else if (pressed == buttons[34] && keyIndex != 3 && submit == -1)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 3;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if (i == 22)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if (i == 23)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if (i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet4[i - 5].ToString();
                }
            }
            else if (Array.IndexOf(buttons, pressed) > 4 && pressed != buttons[31] && pressed != buttons[32] && pressed != buttons[33] && pressed != buttons[34] && pressed.GetComponentInChildren<TextMesh>().text != " " && submit == -1)
            {
                if (keyIndex == 3 && pressed == buttons[15])
                {
                    if (texts[1].text.Trim() != "")
                    {
                        pressed.AddInteractionPunch(0.25f);
                        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                        texts[1].text += " ";
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    texts[1].text += pressed.GetComponentInChildren<TextMesh>().text;
                }
            }
        }
    }

    private bool Valid()
    {
        if (error)
            return true;
        if (addedArticles.Count != 0 && texts[1].text != "")
        {
            return true;
        }
        else if (addedArticles.Count == 0 && texts[1].text != "")
        {
            return true;
        }
        else if (addedArticles.Count == 0 && texts[1].text == "")
        {
            return true;
        }
        else if (addedArticles.Count != 0 && texts[1].text == "")
        {
            return false;
        }
        return false;
    }

    private void DealWithError(int type)
    {
        StopAllCoroutines();
        if (type == 0)
        {
            texts[0].text = "Error: Failed to get a random article!";
            texts[2].text = "Error: Failed to get a random article!";
            Debug.LogFormat("[One Links To All #{0}] Error: Starting/finishing article query failed! Press submit to solve the module.", moduleId);
        }
        else if (type == 1)
        {
            texts[0].text = "Error: Failed to check if the path is valid!";
            texts[1].text = "";
            texts[2].text = "Error: Failed to check if the path is valid!";
            Debug.LogFormat("[One Links To All #{0}] Error: Link query failed! Press submit to solve the module.", moduleId);
        }
        else if (type == 2)
        {
            if (activated)
            {
                texts[0].text = "Error: Failed to grab explicit terms and exceptions!";
                texts[2].text = "Error: Failed to grab explicit terms and exceptions!";
            }
            Debug.LogFormat("[One Links To All #{0}] Error: Explicit terms and exceptions query failed! Press submit to solve the module.", moduleId);
        }
        error = true;
    }

    private bool Censored(string article)
    {
        if (Settings.disableExplicitContent)
        {
            article = article.ToLower();
            if (exceptions.Contains(article))
            {
                return true;
            }
            else
            {
                for (int i = 0; i < explicitTerms.Count; i++)
                {
                    if (article.Contains(explicitTerms[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        else
        {
            return true;
        }
    }

    private char HasValidChars(string title)
    {
        for (int i = 0; i < title.Length; i++)
        {
            if (!keySet1.Contains(title[i]) && !keySet2.Contains(title[i]) && !keySet3.Contains(title[i]) && !keySet4.Contains(title[i]))
            {
                return title[i];
            }
        }
        return ' ';
    }

    private IEnumerator FillCensoringLists()
    {
        getTerms = true;
        Debug.LogFormat("<One Links To All #{0}> Starting query of explicit terms and exceptions due to enabled filter...", moduleId);
        UnityWebRequest www = UnityWebRequest.Get("https://docs.google.com/spreadsheets/d/1J-AanIiu6-mcIa9sw18fdBomRluQiCnhdWMaL-OcGSc/gviz/tq?tqx=out:json");
        yield return www.SendWebRequest();
        if (www.error == null)
        {
            var match = Regex.Match(www.downloadHandler.text, @"google.visualization.Query.setResponse\((.+)\)").Groups[1].Value;
            var sheetResponse = JsonConvert.DeserializeObject<SheetResponse>(match);
            if (sheetResponse == null)
            {
                DealWithError(2);
                yield break;
            }
            var table = sheetResponse.table;
            var columns = table.cols.Select(column => Regex.Replace(column.label.ToLowerInvariant(), "[^a-z]", "")).ToArray();
            foreach (var row in table.rows)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (row.c[i] != null)
                    {
                        if (i == 0)
                            explicitTerms.Add(row.c[i].v.ToLower());
                        else if (i == 1)
                            exceptions.Add(row.c[i].v.ToLower());
                    }
                }
            }
        }
        else
        {
            DealWithError(2);
            yield break;
        }
        Debug.LogFormat("<One Links To All #{0}> Query of explicit terms and exceptions successful!", moduleId);
        getTerms = false;
    }

    private IEnumerator QueryProcess()
    {
        while (getTerms) { yield return null; };
        Debug.LogFormat("<One Links To All #{0}> Starting query of starting article...", moduleId);
        while (title1.Equals(title2) || !Censored(title1))
        {
            WWW www = new WWW(queryGetRandomURL);
            while (!www.isDone) { yield return null; if (www.error != null) break; };
            if (www.error == null)
            {
                try
                {
                    var result = JObject.Parse(www.text);
                    title1 = result["query"]["random"][0]["title"].ToObject<string>();
                } catch (JsonReaderException)
                {
                    DealWithError(0);
                    yield break;
                }
                loadlinks = StartCoroutine(getLeadsToLink(title1));
                while (loadlinks != null) { yield return null; }
                if (queryLinks.Count == 0)
                    title1 = title2;
            }
            else
            {
                DealWithError(0);
                yield break;
            }
        }
        exampleSolution.Add(title1);
        Debug.LogFormat("<One Links To All #{0}> Query of starting article successful! Found starting article: {1}", moduleId, title1);
        exampleSolution.Add(queryLinks.PickRandom());
        title2 = title1;
        Debug.LogFormat("<One Links To All #{0}> Starting query of finishing article...", moduleId);
        repeats = UnityEngine.Random.Range(20, 30);
        redo:
        while (title1.Equals(title2))
        {
            if (curCount < repeats)
            {
                loadlinks = StartCoroutine(getLeadsToLink(exampleSolution.Last()));
                while (loadlinks != null) { yield return null; }
                if (queryLinks.Count == 0 && curCount != 0)
                    title2 = exampleSolution[exampleSolution.Count - 1];
                else if (queryLinks.Count == 0 && curCount == 0)
                {
                    exampleSolution.Remove(exampleSolution.Last());
                    loadlinks = StartCoroutine(getLeadsToLink(title1));
                    while (loadlinks != null) { yield return null; }
                    exampleSolution.Add(queryLinks.PickRandom());
                    curCount--;
                }
                else
                {
                    exampleSolution.Add(queryLinks.PickRandom());
                }
            }
            else
                title2 = exampleSolution.Last();
            curCount++;
        }
        loadlinks = StartCoroutine(getLeadsToLink(title2));
        while (loadlinks != null) { yield return null; }
        if (queryLinks.Count == 0)
        {
            bool done = false;
            while (!done)
            {
                exampleSolution.Remove(exampleSolution.Last());
                loadlinks = StartCoroutine(getLeadsToLink(exampleSolution.Last()));
                while (loadlinks != null) { yield return null; }
                exampleSolution.Add(queryLinks.PickRandom());
                loadlinks = StartCoroutine(getLeadsToLink(exampleSolution.Last()));
                while (loadlinks != null) { yield return null; }
                if (queryLinks.Count != 0)
                    done = true;
            }
            title2 = exampleSolution.Last();
        }
        WWW www2 = new WWW(queryRedirectCheck + "&titles=" + title2.Replace("&", "%26"));
        while (!www2.isDone) { yield return null; if (www2.error != null) break; };
        if (www2.error == null)
        {
            if (www2.text.ToUpper().Contains("#REDIRECT"))
            {
                exampleSolution.Remove(exampleSolution.Last());
                curCount--;
                title2 = title1;
                goto redo;
            }
        }
        else
        {
            DealWithError(0);
            yield break;
        }
        Debug.LogFormat("<One Links To All #{0}> Query of finishing article successful! Found finishing article: {1}", moduleId, title2);
        StopCoroutine(load);
        load = null;
        texts[0].text = title1;
        texts[2].text = title2;
        texts[3].text = 1.ToString();
        Debug.LogFormat("[One Links To All #{0}] The starting article is titled {1} and the ending article is titled {2}", moduleId, title1, title2);
        Debug.LogFormat("[One Links To All #{0}] ==One Possible Path==", moduleId);
        Debug.LogFormat("[One Links To All #{0}] {1}", moduleId, exampleSolution.Join(" => "));
    }

    private IEnumerator noSub()
    {
        load = StartCoroutine(Loading(1));
        bool valid = true;
        Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, title2);
        loadlinks = StartCoroutine(getQueryLinks(title2));
        while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
        Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, title2);
        if (loadlinks != null)
        {
            StopCoroutine(loadlinks);
            loadlinks = null;
        }
        if (!queryLinks.Contains(title1))
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, title2);
            valid = false;
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, title2);
        }
        if (valid)
        {
            submit = 0;
        }
        else
        {
            submit = 1;
        }
        StopCoroutine(load);
        texts[1].text = "✓";
        load = null;
    }

    private IEnumerator finalCheck()
    {
        load = StartCoroutine(Loading(1));
        bool valid = true;
        if (addedArticles.Count == 0)
        {
            Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, temp);
            loadlinks = StartCoroutine(getQueryLinks(temp));
            while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
            Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, temp);
            if (loadlinks != null)
            {
                StopCoroutine(loadlinks);
                loadlinks = null;
            }
            if (!queryLinks.Contains(title1))
            {
                Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, temp);
                valid = false;
            }
            else
            {
                Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, temp);
            }
        }
        else
        {
            for (int i = 0; i <= addedArticles.Count; i++)
            {
                if (i == 0)
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, addedArticles[0]);
                    loadlinks = StartCoroutine(getQueryLinks(addedArticles[0]));
                    while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, addedArticles[0]);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(title1))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, addedArticles[0]);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, addedArticles[0]);
                    }
                }
                else if (i == addedArticles.Count)
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, addedArticles[i - 1], temp);
                    loadlinks = StartCoroutine(getQueryLinks(temp));
                    while (loadlinks != null && !queryLinks.Contains(addedArticles[i - 1])) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, addedArticles[i - 1], temp);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(addedArticles[i - 1]))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, addedArticles[i - 1], temp);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, addedArticles[i - 1], temp);
                    }
                }
                else
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, addedArticles[i - 1], addedArticles[i]);
                    loadlinks = StartCoroutine(getQueryLinks(addedArticles[i]));
                    while (loadlinks != null && !queryLinks.Contains(addedArticles[i - 1])) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, addedArticles[i - 1], addedArticles[i]);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(addedArticles[i - 1]))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, addedArticles[i - 1], addedArticles[i]);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, addedArticles[i - 1], addedArticles[i]);
                    }
                }
            }
        }
        Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, temp, title2);
        loadlinks = StartCoroutine(getQueryLinks(title2));
        while (loadlinks != null && !queryLinks.Contains(temp)) { yield return null; }
        Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, temp, title2);
        if (loadlinks != null)
        {
            StopCoroutine(loadlinks);
            loadlinks = null;
        }
        if (!queryLinks.Contains(temp))
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, temp, title2);
            valid = false;
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, temp, title2);
        }
        if (valid)
        {
            submit = 0;
        }
        else
        {
            submit = 2;
        }
        StopCoroutine(load);
        texts[1].text = "✓";
        load = null;
    }

    private IEnumerator getLeadsToLink(string title)
    {
        queryLinks.Clear();
        contvar = "temp";
        while (contvar != "")
        {
            string urledit = queryLeadsToURL;
            if (contvar != "temp")
                urledit += "&plcontinue=" + contvar;
            string temp = urledit + "&titles=" + title.Replace("&", "%26");
            WWW www = new WWW(temp);
            while (!www.isDone) { yield return null; if (www.error != null) break; };
            if (www.error == null)
            {
                int index = www.text.IndexOf("pages") + 5;
                int ct = 0;
                string id = "";
                string newurl = "";
                while (ct < 2)
                {
                    index++;
                    if (www.text[index].Equals('\"'))
                    {
                        ct++;
                    }
                    else if (ct == 1)
                    {
                        id += www.text[index];
                    }
                }
                id = "\"" + id + "\"";
                newurl = www.text.Replace(id, "\"id\"");
                newurl = newurl.Replace("\"continue\"", "\"cont\"");
                var result = JObject.Parse(newurl);
                int count = 0;
                while (true)
                {
                    try
                    {
                        string check = result["query"]["pages"]["id"]["links"][count]["title"].ToObject<string>();
                        if ((curCount == (repeats - 1) && Censored(check) && !exampleSolution.Contains(check)) || (curCount != (repeats - 1) && Censored(check) && HasValidChars(check) == ' ' && !exampleSolution.Contains(check)))
                            queryLinks.Add(check);
                        count++;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Here in case it runs out of articles on a page to add
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        // Here in case it is not a valid article
                        break;
                    }
                }
                if (newurl.Contains("\"cont\""))
                {
                    contvar = result["cont"]["plcontinue"].ToObject<string>();
                }
                else
                {
                    contvar = "";
                }
            }
            else
            {
                DealWithError(0);
                yield break;
            }
        }
        loadlinks = null;
    }

    private IEnumerator getQueryLinks(string title)
    {
        queryLinks.Clear();
        bool docheck = false;
        if (exampleSolution.Contains(title) && title != title1)
        {
            docheck = true;
            queryLinks.Add(exampleSolution[exampleSolution.IndexOf(title) - 1]);
        }
        contvar = "temp";
        while (contvar != "")
        {
            string urledit = queryCheckBackURL;
            if (contvar != "temp")
                urledit += "&lhcontinue=" + contvar;
            string temp = urledit + "&titles=" + title.Replace("&", "%26");
            WWW www = new WWW(temp);
            while (!www.isDone) { yield return null; if (www.error != null) break; };
            if (www.error == null)
            {
                int index = www.text.IndexOf("pages") + 5;
                int ct = 0;
                string id = "";
                string newurl = "";
                while (ct < 2)
                {
                    index++;
                    if (www.text[index].Equals('\"'))
                    {
                        ct++;
                    }
                    else if (ct == 1)
                    {
                        id += www.text[index];
                    }
                }
                id = "\"" + id + "\"";
                newurl = www.text.Replace(id, "\"id\"");
                newurl = newurl.Replace("\"continue\"", "\"cont\"");
                var result = JObject.Parse(newurl);
                int count = 0;
                while (true)
                {
                    try
                    {
                        string check = result["query"]["pages"]["id"]["linkshere"][count]["title"].ToObject<string>();
                        if (docheck)
                        {
                            if (check != exampleSolution[exampleSolution.IndexOf(title) - 1])
                                queryLinks.Add(check);
                        }
                        else
                            queryLinks.Add(check);
                        count++;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Here in case it runs out of articles on a page to add
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        // Here in case it is not a valid article
                        break;
                    }
                }
                if (newurl.Contains("\"cont\""))
                {
                    contvar = result["cont"]["lhcontinue"].ToObject<string>();
                }
                else
                {
                    contvar = "";
                }
            }
            else
            {
                DealWithError(1);
                yield break;
            }
        }
        loadlinks = null;
    }

    private IEnumerator Loading(int type)
    {
        int ct = 0;
        while (true)
        {
            if (ct > 3)
            {
                ct = 0;
            }
            if (ct == 0)
            {
                if (type == 0)
                {
                    texts[0].text = ".";
                    texts[2].text = ".";
                }
                else
                {
                    texts[1].text = ".";
                }
            }
            else if (ct == 1)
            {
                if (type == 0)
                {
                    texts[0].text = "..";
                    texts[2].text = "..";
                }
                else
                {
                    texts[1].text = "..";
                }
            }
            else if (ct == 2)
            {
                if (type == 0)
                {
                    texts[0].text = "...";
                    texts[2].text = "...";
                }
                else
                {
                    texts[1].text = "...";
                }
            }
            else if (ct == 3)
            {
                if (type == 0)
                {
                    texts[0].text = "";
                    texts[2].text = "";
                }
                else
                {
                    texts[1].text = "";
                }
            }
            ct++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} type <title> [Types 'title' using the keypad (Case sensative)] | !{0} add [Presses the add (+) button] | !{0} minus (#) [Presses the minus (-) button (optionally '#' times)] | !{0} clear [Presses the clear button] | !{0} delete (#) [Presses the delete button (optionally '#' times)] | !{0} submit [Presses the submit button]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if ((load != null || !activated) && !error)
        {
            yield return "sendtochaterror Buttons cannot be pressed right now!";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*add\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot add an empty article!";
                yield break;
            }
            buttons[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*minus\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (curIndex == 0)
            {
                yield return "sendtochaterror Cannot remove anymore articles!";
                yield break;
            }
            buttons[1].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot clear text on an empty screen!";
                yield break;
            }
            buttons[3].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*delete\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot delete text on an empty screen!";
                yield break;
            }
            buttons[2].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!Valid())
            {
                yield return "sendtochaterror Cannot press the submit button with at least 1 article added and having an empty screen!";
                yield break;
            }
            buttons[4].OnInteract();
            if (error && moduleSolved) yield return "awardpoints -15";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (parameters.Length >= 2)
            {
                parameters[1] = command.Substring(5, command.Length - 5);
                char check = HasValidChars(parameters[1]);
                if (check != ' ')
                {
                    yield return "sendtochaterror The specified character to type '" + check + "' is invalid!";
                    yield break;
                }
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    if (parameters[1][i].Equals(' '))
                    {
                        if (keyIndex != 3)
                        {
                            buttons[34].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                        buttons[15].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                    else if (keySet1.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 0)
                        {
                            buttons[31].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                        buttons[Array.IndexOf(keySet1, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                    else if (keySet2.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 1)
                        {
                            buttons[32].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                        buttons[Array.IndexOf(keySet2, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                    else if (keySet3.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 2)
                        {
                            buttons[33].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                        buttons[Array.IndexOf(keySet3, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                    else if (keySet4.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 3)
                        {
                            buttons[34].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                        buttons[Array.IndexOf(keySet4, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify what to type!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*minus\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp > 0 && temp <= addedArticles.Count)
                    {
                        if (curIndex == 0)
                        {
                            yield return "sendtochaterror Cannot remove anymore articles!";
                            yield break;
                        }
                        for (int i = 0; i < temp; i++)
                        {
                            buttons[1].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror The specified number of times to press the minus button is out of range 1-[# of articles - 1]!";
                    }
                }
                else
                {
                    yield return "sendtochaterror The specified number of times to press the minus button is invalid!";
                }
            }
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the number of times to press minus button!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*delete\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (error)
            {
                yield return "sendtochaterror An error is being displayed, you may only press submit!";
                yield break;
            }
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp > 0 && temp <= texts[1].text.Length)
                    {
                        if (texts[1].text.Equals(""))
                        {
                            yield return "sendtochaterror Cannot delete text on an empty screen!";
                            yield break;
                        }
                        for (int i = 0; i < temp; i++)
                        {
                            buttons[2].OnInteract();
                            yield return new WaitForSeconds(0.025f);
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror The specified number of times to press the delete button is out of range 1-[length of text on screen]!";
                    }
                }
                else
                {
                    yield return "sendtochaterror The specified number of times to press the delete button is invalid!";
                }
            }
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the number of times to press delete button!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while ((load != null || !activated) && !error) { yield return true; }
        if (error || submit == 0)
        {
            buttons[4].OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        else if (submit == 1 || submit == 2)
        {
            moduleSolved = true;
            audio.PlaySoundAtTransform("solve", transform);
            for (int i = 5; i < 31; i++)
            {
                if (i == 11)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 14)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 15)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 16)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 17)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 20)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 21)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 22)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 23)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 27)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 29)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                }
                if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = ":)";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySetSolve[i - 5].ToString();
                }
            }
            GetComponent<KMBombModule>().HandlePass();
            texts[0].text = "";
            texts[1].text = "GG";
            texts[2].text = "";
            texts[3].text = "";
        }
        else
        {
            for (int i = 0; i < curCount - 1; i++)
            {
                if (addedArticles.Count == i)
                    break;
                else if (exampleSolution[i + 1] != addedArticles[i])
                {
                    while (addedArticles.Count != i)
                    {
                        buttons[1].OnInteract();
                        yield return new WaitForSeconds(0.025f);
                    }
                    break;
                }
            }
            if (texts[1].text != exampleSolution[curIndex + 1])
            {
                buttons[3].OnInteract();
                yield return new WaitForSeconds(0.025f);
            }
            int start = curIndex + 1;
            for (int i = start; i < exampleSolution.Count - 1; i++)
            {
                if (texts[1].text == "")
                    yield return ProcessTwitchCommand("type " + exampleSolution[i]);
                if (i != (exampleSolution.Count - 2))
                {
                    buttons[0].OnInteract();
                    yield return new WaitForSeconds(0.025f);
                }
            }
            buttons[4].OnInteract();
            yield return new WaitForSeconds(0.025f);
            buttons[4].OnInteract();
        }
    }

    private class SheetResponse
    {
        public Table table;

        public class Table
        {
            public Column[] cols;
            public Row[] rows;
            public class Column
            {
                public string label;
            }

            public class Row
            {
                public Value[] c;

                public class Value
                {
                    public string v;
                }
            }
        }
    }

    class OneLinksToAllSettings
    {
        public bool disableExplicitContent = true;
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "OneLinksToAllSettings.json" },
            { "Name", "One Links To All Settings" },
            { "Listing", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "disableExplicitContent" },
                    { "Text", "If enabled, One Links To All will try not generate starting and ending articles that contain explicit terms." }
                },
            } }
        }
    };
}
