using UnityEngine;
using System.Collections;                       // Gebruikt om Arry's te kunnen gebruiken.
using UnityEngine.Networking;                   // Gebruikt voor het ophalen van data via UnityWebRequest.
using Newtonsoft.Json;                          // Newsoft.Json extentie (Zie: https://www.newtonsoft.com/json)
using Newtonsoft.Json.Linq;
using UnityEditor.Search;

public class FindLocationKerktijden : MonoBehaviour
{
    public string kerkNaam;                          // De naam van de kerk, in te vullen in de inspector
    public string kerkStad;                          // De stad/woonplaats waar de kerk in staat.
    public string kerkGenootschap;                   // Bij welk kerkgenootschap hoort deze kerk (CGK, NGK, PKN, ...)
    public string domineesNlNaam;                    // De naam zoals deze in dominees.nl wordt weergegeven

    public bool debugMode = false;                   // Als aan, worden extra logs gestuurd om meer informatie te verkrijgen.

    private float startTijd;
    private float eindTijd;

    // Zodra de app opstart, runt Unity automatisch deze functie.
    void Start()
    {
        StartCoroutine(ZoekKerk(kerkNaam)); // Roep direct de zoek-coroutine aan.
    }
    
    // Simpele publieke API om te zoeken op een kerk door de kerkNaam in te vullen.
    // Kan later worden gebruikt door een MapHandler om kerk-locaties te vinden.
    public void KrijgLatLon(string kerkNaam)
    {
        StartCoroutine(ZoekKerk(kerkNaam));
    }

    // Een Coroutine om data op te halen vanuit de kerktijden API.
    // Ik heb nog geen officiele toesteming om deze API te gebruiken,
    // We wachten nog op update (Laatste update: 19-09-2026)
    IEnumerator ZoekKerk(string kerkNaam)
    {
        startTijd = Time.deltaTime;                                                 // Sla de huidige tijd op. Dan kunnen we later de totale processing-time berekenen om een beeld van de snelheid van dit algoritme te krijgen.

        if(domineesNlNaam != null)
            FormateerDomineesNlNaam(domineesNlNaam);

        // Stel een url samen die zoek op de dataset 'gebedshuizen' + de kerkNaam.
        string url = 
        $"https://api.kerktijden.nl/api/search/findcommunity?query={FormateerKerkNaam(kerkStad)}&namequery=&genootschappen=&afstand=20&dagen=&datum=&index=";

        Debug.Log($"Ophalen vanaf: {url}...");

        UnityWebRequest www = UnityWebRequest.Get(url);                             // Haal data op uit de url: 'url'.

        yield return www.SendWebRequest();                                          // Return de data naar de UnityWebRequest.

        if (www.result != UnityWebRequest.Result.Success)                           // Check of de request succesvol is.
        {
            Debug.LogError(www.error);                                              // Log de specifieke error in de console.
            yield break;
        }
        else
        {
            if(debugMode)
            {
                string json = www.downloadHandler.text;
                
                // Formatter de Json in een wat mooier format vai JsonConvert.
                string prettyJson = JsonConvert.SerializeObject(
                    JsonConvert.DeserializeObject(json),
                    Formatting.Indented
                );

                Debug.Log(prettyJson);                                              // Log de raw response in json, ald debugMode aan staat.
            }                           

            FilterEnVerwerk(www.downloadHandler.text);                              // Stuur het resultaat door naar de FIlterEnVerwerk() functie, om het juiste resultaat te filteren en terug te geven.
        }
    }

    IEnumerator HaalGemeenteOp(string id)
    {
        // Stel een url samen die zoek op de dataset 'gebedshuizen' + de kerkNaam.
        string url = 
        $"https://api.kerktijden.nl//api/community/getcommunity?id={id}";

        UnityWebRequest www = UnityWebRequest.Get(url);                             // Haal data op uit de url: 'url'.

        yield return www.SendWebRequest();                                          // Return de data naar de UnityWebRequest.

        if (www.result != UnityWebRequest.Result.Success)                           // Check of de request succesvol is.
        {
            Debug.LogError(www.error);                                              // Log de specifieke error in de console.
            yield break;
        }
        else
        {
            string json = www.downloadHandler.text;
                
                // Formatter de Json in een wat mooier format vai JsonConvert.
                string prettyJson = JsonConvert.SerializeObject(
                    JsonConvert.DeserializeObject(json),
                    Formatting.Indented
                );
            
            eindTijd = Time.deltaTime;                                             // Sla de huidige tijd op als de 'eindTijd' zolang heeft het geduurd om alles op te halen en te verwerken.
            // Log het eindresultaat naar de Unity Console.
            Debug.Log($"Resultaat binnen! | Tijd: {startTijd-eindTijd}s \n{prettyJson}");
        }
    }

    private void FilterEnVerwerk(string data)
    {
        data = data.Trim();                                                         // Versimpel de string (Verwijderd overbodige spaties/witregels).

        JArray resultaten = JArray.Parse(data);                                     // De hele .json tekst wordt omgezet naar een JArray. Hier kan Newsoft.Json makkelijk mee navigeren.

        JToken besteResultaat = null;                                               // Maak een JToken aan voor het gewenste resultaat (Hier nog 'null').
        long hoogtsteNaamScore = 0;                                                 // Stel een lege 'hoogsteScore' variabele voor de naam in.
        long hoogtsteGenootschapScore = 0;                                          // Stel een lege 'hoogsteScore' variabele voor het genootschap in.

        kerkGenootschap = FormateerGenooschap(kerkGenootschap);                     // Zet -als nodig- de afkorting om in de naam die api.kerktijden.nl kan begrijpen.

        foreach (JToken gemeente in resultaten)
        {
            long naamScore = 0;
            long genootschapScore = 0;

            string naam = gemeente["name"].ToString();
            string genootschap = gemeente["denomination"]["name"].ToString();

            if(FuzzySearch.FuzzyMatch($"{kerkStad}", naam, ref naamScore))
            {
                if(naamScore > hoogtsteNaamScore)
                    hoogtsteNaamScore = naamScore;
            }

            if(FuzzySearch.FuzzyMatch(kerkGenootschap, genootschap, ref genootschapScore))
            {
                if(genootschapScore > hoogtsteGenootschapScore)
                    hoogtsteGenootschapScore = genootschapScore;
            }

            if((naamScore + genootschapScore) >= (hoogtsteNaamScore + hoogtsteGenootschapScore))
            {
                besteResultaat = gemeente;
            }
        }

        string id;

        if(besteResultaat != null)
        {
            id = besteResultaat["id"]?.ToString();                                      // Sla de ID van het beste resultaat op.
            StartCoroutine(HaalGemeenteOp(id));                                         // Haal alle data van de gemeente op vanaf de api.
        }
        else
        {
            Debug.LogWarning($"Geen goed resultaat gevonden voor: {kerkNaam} ({kerkGenootschap})");          
        }
    }

    // Simpele formatterfunctie die spaties in '+' veranderd, zodat
    // de API deze goed kan begrijpen.
    private string FormateerKerkNaam(string input)
    {
        string resutaat = input.Replace(" ", "+");
        return resutaat;
    }

    public void FormateerDomineesNlNaam(string input)
    {
        int laatsteStreep = input.LastIndexOf('-');                              // Zoek het laatste streepje in de naam, de laatste vanwege dit soort cassusen :'Kampen-Noord-Ngk'

        kerkStad = input.Substring(0, laatsteStreep).Trim();                     // Alles vóór het streepje
        kerkGenootschap = input.Substring(laatsteStreep + 1).Trim();             // Alles na het streepje
    }

    // Simpele omzetfunctie die afkortingen vanuit Dominees.nl
    // omzet naar volledige officiele namen zoals ze staan op kerktijden.nl
    // (Voor omzet zie: https://api.kerktijden.nl//api/search/GetDenominationsWithParents en https://dominees.nl/gemeentes.php)
    private string FormateerGenooschap(string input)
    {
        if(input == "bap")
            return "Vrije Baptisten Gemeenten";
        if(input == "bw")
            return "Overig";
        if(input == "cgk")
            return "Christelijke Gereformeerde Kerken";
        if(input == "dg")
            return "Doopsgezinde Broederschap";
        if(input == "gg")
            return "Gereformeerde Gemeenten";
        if(input == "ggin")
            return "Gereformeerde Gemeenten in Nederland";
        if(input == "gk")
            return "Gereformeerde Kerken";
        if(input == "gkv")
            return "Gereformeerde Kerken vrijgemaakt";
        if(input == "hg")
            return "Protestantse Kerk in Nederland (Hervormde Gemeenten)";
        if(input == "hhg")
            return "Hersteld Hervormde Kerk";
        if(input == "lg")
            return "Protestantse Kerk in Nederland (Evangelisch-Lutherse Gemeenten)";
        if(input == "ngk")
            return "Nederlandse Gereformeerde Kerken";
        if(input == "r")
            return "Remonstrantse Broederschap";
        if(input == "veg")
            return "Bond van Vrije Evangelische Gemeenten in Nederland";
        if(input == "vgkn")
            return "Voortgezette Gereformeerde Kerken in Nederland";
        if(input == "w" || input == "z" || input == "zd")
            return "Overige";
        else
            return input;
    }
}


/*
        foreach (JToken gemeente in resultaten)
        {
            string naam = gemeente["name"].ToString();                              // Zoek de naam op 'name' in de json en sla deze op.
            string genootschap = gemeente["denomination"]["name"].ToString();       // Zoek het genootschap op 'naam' binnen 'domination' in de json en sla deze op.

            // Als de kerkNaam niet is ingevuld, hoeven we die niet mee te nemen in de check op een match.
            // Dit kan namelijk voor een probleem zorgen omdat StringComparison alles afkeurt vanwege een extra ','
            // aan het einde van de kerkStad.
            if(string.IsNullOrEmpty(kerkNaam))
            {
                // Ljikt de naam en klopt het genootschap exact?
                if(naam.IndexOf($"{kerkStad}", System.StringComparison.OrdinalIgnoreCase) >= 0 && string.Equals(kerkGenootschap, genootschap))
                {

                    if(debugMode)
                    {
                        long score = 0;

                        if(FuzzySearch.FuzzyMatch($"{kerkStad}", $"{naam}", ref score))
                            Debug.Log($"Nieuw beste resultaat: {score}");
                    }

                    besteResultaat = gemeente;                                          // Sla het resultaat op
                    break;                                                              // Stop de loop, we hebben een winnar :)
                }        
            }
            else
            {
                // Ljikt de naam en klopt het genootschap exact?
                if(naam.IndexOf($"{kerkStad}, {kerkNaam}", System.StringComparison.OrdinalIgnoreCase) >= 0 && string.Equals(kerkGenootschap, genootschap))
                {
                    besteResultaat = gemeente;                                          // Sla het resultaat op

                    if(debugMode)
                    {
                        long score = 0;

                        if(FuzzySearch.FuzzyMatch($"{kerkStad}, {kerkNaam}", $"{naam}", ref score))
                            Debug.Log($"Nieuw beste resultaat: {score}");
                    }

                    break;                                                              // Stop de loop, we hebben een winnar :)
                }
            }

            // Resultaat matcht niet perfect, maar wel goed genoeg.
            if(besteResultaat == null && naam.IndexOf($"{kerkStad}, {kerkNaam}", System.StringComparison.OrdinalIgnoreCase) >= 0 && genootschap.IndexOf($"{kerkGenootschap}", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                besteResultaat = gemeente;                                          // Dus kan het bewaard worden, zonder te 'break' aan te roepen.
            }
        }

        if (besteResultaat == null)                                                 // Alle resultaten gehad, geen resultaat was goed genoeg?
        {
            // Log de error als een Debug.LogWarning().
            Debug.LogWarning($"Geen passende kerk gevonden voor: {kerkStad}-{kerkGenootschap}");
            return;                                                                 // En stuur een leeg resultaat terug.
        }

        // Gevonden gemeente:
        string id = besteResultaat["id"]?.ToString();                               // Sla de ID van het beste resultaat op.
        string naamGevonden = besteResultaat["name"]?.ToString();                   // Sla de naam van het beste resultaat op.
*/