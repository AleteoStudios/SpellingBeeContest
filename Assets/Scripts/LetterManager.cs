using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using Unity.Netcode;
using Unity.Collections;



public class LetterManager : NetworkBehaviour
{
    [Serializable]
    public class WordData
    {

        public string word;
        public string speech;
        public string sentence;
        public string definition;


    }

    [Header("JSON")]
    public String fileName;
    public WordData[] words;
    private string fileFormat = ".json";

    [Header("Referencias de UI")]

    public TextMeshProUGUI showingWord;
    public TextMeshProUGUI showingSpeach;
    public TextMeshProUGUI showingSentence;
    public TextMeshProUGUI showingDefinition;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI wordsStockLabel;
    public TextMeshProUGUI levelLabelCnt;

    [Header("Variables de Red")]
    private NetworkVariable<FixedString128Bytes> randomWord = new();
    private NetworkVariable<FixedString128Bytes> randomSpeach = new();
    private NetworkVariable<FixedString128Bytes> randomSentence = new();
    private NetworkVariable<FixedString128Bytes> randomDefinition = new();
    private NetworkVariable<FixedString128Bytes> level = new();

    private NetworkVariable<int> wordsSize = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone);

    [Header("Panels")]

    public GameObject canvasHost;
    public GameObject canvasHome;

    public GameObject panelRight;
    public GameObject panelIncorrect;

    public Image imageStatus;
    public Sprite spriteRight, spriteIncorrect, spriteLogo;



    public Timer timerManager;

    public List<string> spellingWord = new List<string>();
    public List<string> speachList = new List<string>();
    public List<string> sentencesList = new List<string>();
    public List<string> definitionList = new List<string>();

    public int index;




    private void Awake()
    {
       
    }



    private void Start()
    {

        levelLabelCnt.text = "Ready?";
        SaveData();

    }



    private void LoadJsonData()
    {
        string filePath = Path.Combine(Application.persistentDataPath,"Datos",fileName + fileFormat);
        Debug.Log("Intentando cargar JSON desde: " + filePath);

        if (File.Exists(filePath))
        {
            string fileJson = File.ReadAllText(filePath);
            Debug.Log("Contenido JSON: " + fileJson);

            words = JsonHelper.FromJsonArray<WordData>(fileJson);

            if (words == null || words.Length == 0)
            {
                Debug.LogError("No se pudo deserializar el JSON o está vacío.");
                return;
            }
        }
        else
        {
            Debug.LogError("No se encontró el archivo JSON: " + filePath);
            return;
        }

        spellingWord.Clear();
        speachList.Clear();
        sentencesList.Clear();
        definitionList.Clear();

        for (int i = 0; i < words.Length; i++)
        {
            spellingWord.Add(words[i].word);
            speachList.Add(words[i].speech);
            sentencesList.Add(words[i].sentence);
            definitionList.Add(words[i].definition);
        }

        Debug.Log($"Se cargaron correctamente {words.Length} palabras desde: {filePath}");
    }



    public void RandomWord()
    {
        if (!IsServer) return; // Solo el host modifica variables de red

        RunEvaluatorForAll();
        TimerManager();

        index = UnityEngine.Random.Range(0, spellingWord.Count);

        // Asigna valores a las NetworkVariables
        randomWord.Value = new FixedString128Bytes(spellingWord[index]);
        randomSpeach.Value = new FixedString128Bytes(speachList[index]);
        randomSentence.Value = new FixedString128Bytes(sentencesList[index]);
        randomDefinition.Value = new FixedString128Bytes(definitionList[index]);
        level.Value = levelLabel.text;
        wordsSize.Value = spellingWord.Count;




        // Borra los elementos ya usados
        spellingWord.RemoveAt(index);
        speachList.RemoveAt(index);
        sentencesList.RemoveAt(index);
        definitionList.RemoveAt(index);

    }


    public void RightBtn()
    {
        panelRight.SetActive(true);
        imageStatus.sprite = spriteRight;
        timerManager.StopTimer();
        timerManager.toggle.isOn = false;
    }

    public void IncorrectBtn()
    {
        panelIncorrect.SetActive(true);
        imageStatus.sprite = spriteIncorrect;
        timerManager.StopTimer();
        timerManager.toggle.isOn = false;
    }

    public void Home()
    {
        RunEvaluatorForAll();
        TimerManager();
        spellingWord.Clear();
        speachList.Clear();
        sentencesList.Clear();
        definitionList.Clear();
        canvasHost.SetActive(false);
        canvasHome.SetActive(true);
        levelLabelCnt.text = "Ready?";
    }
    public void Level1()
    {
        fileName = "Round1";
        LoadJsonData();
        levelLabel.text = "1st Round";
        RandomWord();
        levelLabelCnt.text = "1st Round";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);
    }

    public void Level2()
    {
        fileName = "Round2";
        LoadJsonData();
        RandomWord();
        levelLabel.text = "2nd Round";
        levelLabelCnt.text = "2nd Round";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);
    }

    public void Level3()
    {
        fileName = "Round3";
        LoadJsonData();
        RandomWord();
        levelLabel.text = "3rd Round";
        levelLabelCnt.text = "3rd Round";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);
    }

    public void Level4()
    {
        fileName = "Round4";
        LoadJsonData();
        RandomWord();
        levelLabel.text = "4th Round";
        levelLabelCnt.text = "4th Round";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);

    }

    public void Level5()
    {
        fileName = "Round5";
        LoadJsonData();
        RandomWord();
        levelLabel.text = "5th Round";
        levelLabelCnt.text = "5th Round";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);
    }

    public void Level6()
    {
        fileName = "3grade3row";
        LoadJsonData();
        RandomWord();
        levelLabel.text = "3rd Grade 2nd Row";
        levelLabelCnt.text = "3rd Grade 2nd Row";
        canvasHome.SetActive(false);
        canvasHost.SetActive(true);
    }


    void TimerManager()
    {
        timerManager.StopTimer();
        timerManager.ResetTimer();
        timerManager.toggle.isOn = false;
    }

    void EvaluatorManager()
    {
        panelRight.SetActive(false);
        panelIncorrect.SetActive(false);
        imageStatus.sprite = spriteLogo;
    }

    public void ExitApp()
    {
        Application.Quit();
    }



    public void Update()
    {

        wordsStockLabel.text = "Words in stock: " + wordsSize.Value;


        if (Input.GetKeyDown("space"))
        {
            RandomWord();
        }



    }

    public override void OnNetworkSpawn()
    {
        randomWord.OnValueChanged += (_, newVal) => showingWord.text = newVal.ToString();
        randomSpeach.OnValueChanged += (_, newVal) => showingSpeach.text = newVal.ToString();
        randomSentence.OnValueChanged += (_, newVal) => showingSentence.text = "<b>Sentence: </b>" + newVal.ToString();
        randomDefinition.OnValueChanged += (_, newVal) => showingDefinition.text = "<b>Definition: </b>" + newVal.ToString();
        level.OnValueChanged += (_, newVal) => levelLabel.text = newVal.ToString();

    }

    [ServerRpc(RequireOwnership = false)]
    public void RightBtnServerRpc()
    {
        RightBtnClientRpc();
    }



    [ServerRpc(RequireOwnership = false)]
    public void IncorrectBtnServerRpc()
    {
        IncorrectBtnClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EvaluatorServerRpc()
    {
        EvaluatorClientRpc();
    }


    [ClientRpc]
    void RightBtnClientRpc()
    {
        RightBtn(); // Ejecuta el método en todos
    }

    [ClientRpc]
    void IncorrectBtnClientRpc()
    {
        IncorrectBtn(); // Ejecuta el método en todos
    }

    [ClientRpc]
    void EvaluatorClientRpc()
    {
        EvaluatorManager();
    }

    public void RunEvaluatorForAll()
    {
        // Ejecuta localmente
        EvaluatorManager();

        // Si es servidor, notifícalo a todos los clientes
        if (IsServer)
        {
            EvaluatorClientRpc(); // llama al método en clientes
        }
        else
        {
            // si es cliente, pide al servidor que lo propague
            EvaluatorServerRpc();
        }
    }

    public void SaveData()
    {
        LoadJsonData();
        RandomWord();
    }


}