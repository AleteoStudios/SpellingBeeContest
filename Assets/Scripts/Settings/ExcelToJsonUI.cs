using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Xml;
using SFB;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExcelToJsonUI : MonoBehaviour
{
    public Button cargarArchivoBtn;
    public Button convertirJsonBtn;
    public TextMeshProUGUI rutaArchivoTxt;
    public Slider barraProgreso;
    public TextMeshProUGUI mensajeEstado;
    public TMP_InputField inputNombreArchivo;

    private string rutaCSV;

    [System.Serializable]
    public class DataRow
    {
        public string word;
        public string speech;
        public string sentence;
        public string definition;
    }

    void Start()
    {
        cargarArchivoBtn.onClick.AddListener(SeleccionarArchivo);
        convertirJsonBtn.onClick.AddListener(() => StartCoroutine(ConvertirAJson()));
        barraProgreso.value = 0;
        mensajeEstado.text = "";
    }

    void SeleccionarArchivo()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Selecciona archivo CSV", "", "csv", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            rutaCSV = paths[0];
            rutaArchivoTxt.text = rutaCSV;
            mensajeEstado.text = "";
        }
        else
        {
            rutaArchivoTxt.text = "Ningún archivo seleccionado.";
        }
    }

    IEnumerator ConvertirAJson()
    {
        if (string.IsNullOrEmpty(rutaCSV))
        {
            mensajeEstado.text = "No se ha seleccionado un archivo.";
            yield break;
        }

        string nombreArchivo = inputNombreArchivo.text.Trim();
        if (string.IsNullOrEmpty(nombreArchivo))
        {
            mensajeEstado.text = "Por favor, ingresa un nombre para el archivo JSON.";
            yield break;
        }

        if (!nombreArchivo.EndsWith(".json"))
            nombreArchivo += ".json";

        mensajeEstado.text = "Cargando archivo...";
        barraProgreso.value = 0.1f;

        List<string> lines = new List<string>();
        try
        {
            using (var fs = new FileStream(rutaCSV, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }
        }
        catch (IOException e)
        {
            mensajeEstado.text = "Error al leer archivo: " + e.Message;
            yield break;
        }

        yield return null;

        var jsonList = new List<DataRow>();
        int total = lines.Count - 1;

        for (int i = 1; i < lines.Count; i++)
        {
            var values = ParseCSVLine(lines[i]);
            var row = new DataRow
            {
                word = values.Count > 0 ? values[0] : "",
                speech = values.Count > 1 ? values[1] : "",
                sentence = values.Count > 2 ? values[2] : "",
                definition = values.Count > 3 ? values[3] : ""
            };
            jsonList.Add(row);

            barraProgreso.value = 0.1f + 0.7f * (i / (float)total);
            yield return null;
        }

        barraProgreso.value = 0.9f;


        string jsonFinal = "[\n";

        for (int i = 0; i < jsonList.Count; i++)
        {
            string elemento = JsonUtility.ToJson(jsonList[i], true);
            jsonFinal += elemento;
            if (i < jsonList.Count - 1)
                jsonFinal += ",\n";
        }
        jsonFinal += "\n]";

        string carpetaDestino = Path.Combine(Application.persistentDataPath, "Datos");
        
        if (!Directory.Exists(carpetaDestino))
        {
            Directory.CreateDirectory(carpetaDestino);
            Debug.Log("Carpeta creada en: " + carpetaDestino);
        }


        try
        {
            File.WriteAllText(Path.Combine(carpetaDestino, nombreArchivo), jsonFinal);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (IOException e)
        {
            mensajeEstado.text = "Error al guardar JSON: " + e.Message;
            yield break;
        }

        barraProgreso.value = 1f;
        mensajeEstado.text = $"Archivo '{nombreArchivo}' convertido correctamente 🎉";
    }


    List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool insideQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Manejo de comillas dobles ""
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current += '"';
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }
}

