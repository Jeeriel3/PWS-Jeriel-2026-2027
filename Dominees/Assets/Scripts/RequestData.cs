using UnityEngine;
using System.Collections;
using UnityEngine.Networking;       // Gebruikt voor het ophalen van data via UnityWebRequest.
using System;                       // Gebruikt voor JsonUtility.
using Newtonsoft.Json.Linq;         // Newsoft.Json extentie (Zie: https://www.newtonsoft.com/json)

using TMPro;                        // Gebruikt om een simpele inputField te gebruiken om beroepen van verschillende data op te halen.

// Een simpele 'Beroep' class die alle data uit een beroep behoud:
[System.Serializable]
public class Beroep
{
    public string ber;          // Aantal uitgebrachte beroepen in huidige vacature door betreffende gemeente.
    public string gemeente;     // Gemeente die het beroep heeft uitgebracht.
    public string persoon;      // Voorletter + Achternaam dominee.
    public string herkomst;     // Originele gemeente van deze dominee.
    public string beslissing;   // Heeft de dominee het beroep aangenomen of bedankt? (Waar: aan = aangenomen en bed = bedankt)
    public string ber2;         // Aantal ontvangen beroepen door betreffende persoon in huidige herkomstsituatie.
    public string datum;        // De datum dat het beroep is geplaatst.
}

public class RequestData : MonoBehaviour
{
    // Veld om in de inspector in te vullen.
    // Dit is de url waar de data vanuit wordt opgehaald.
    // Voorbeeld: https://www.dominees.nl/GetBeroepen.php?q={Jaar}{Weeknummer}
    public string fetchUrl;

    // 2 simpele references om een UI werking te testen.
    public TMP_InputField jaarWeekInput;
    public TMP_Text beroepenOutputText;

    // Zodra de app opstart, runt Unity automatisch deze functie.
    void Start()
    {
        // Roep de Fetch coroutine aan.
        // StartCoroutine(FetchData(fetchUrl));
    }

    // Simpele functie om een URL samen te stellen vanuit een jaarWeek InputField in de Unity UI.
    public void GetDataFromInputField()
    {
        GetData($"https://www.dominees.nl/GetBeroepen.php?q={jaarWeekInput.text}");
    }

    // Publieke API om data op te halen vanuit een specifieke url.
    public void GetData(string url)
    {
        StartCoroutine(FetchData(url));
    }

    // Een simpele Coroutune om data op te halen.
    // Dominees.nl heeft een pagina die de data in een .json formaat teruggeeft.
    // De tekst uit deze webpagina kan je ophalen via UnityWebRequest. 
    // (Zie Eindverslag - Stap 4 - Deelvraag: Op welke technische manier kan de data van Dominees.nl efficiënt en stabiel worden ingeladen en verwerkt in mijn app?)
    IEnumerator FetchData(string url)
    {
        beroepenOutputText.text = null;                                             // Oude beroepen uit de UI text verwijderen.

        UnityWebRequest www = UnityWebRequest.Get(url);                             // Haal data op uit de url: '_url'
        yield return www.SendWebRequest();                                          // Return de data naar de UnityWebRequest.

        if(www.result != UnityWebRequest.Result.Success)                            // Check of de request succesvol is.
        {
            Debug.LogWarning($"Error: {www.error}");                                // Log de specifieke error in de console.
        }
        else
        {
            Debug.Log($"Data susccesvol verkregen: {www.downloadHandler.text}");    // Log het reusltaat als text. (Dit kunnen we later verwerken als string)


            VerwerkData(www.downloadHandler.text);                                  // Stuur de data naar VerwerkData() functie om te verwerken en om te zetten naar een class.
        }
    }

    // Omdat de data die we verkrijgen niet overteenkomt met een normaal .json bestand moeten we eerst de data verwerken.
    // Deze functie splitst alle verkregen data op in aparte regels, om ze vervolgens te verwerken.
    // Daarvoor gebruik ik een package genaamd Newsoft Json. (Zie: https://www.newtonsoft.com/json)
    public void VerwerkData(string data)
    {
        data = data.Trim();                                         // Versimpel de string (Verwijderd overbodige spaties/witregels).

        JObject json = JObject.Parse(data);                         // De hele .json tekst wordt omgezet naar een JObject. Hier kan Newsoft.Json makkelijk mee navigeren.

        JObject eersteEntry = (JObject)json.First.First;            // Pak de "0" weg uit de Json, zodat hier geen problemen mee komen.

        string beroepenText = eersteEntry["beroepen"].ToString();   // Pak de "beroepen" data uit de JObject en zet deze om naar een string.
        
        string[] regelsInData = beroepenText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);        //  Splits de data per regel ('\n' = nieuwe regel)

        foreach (string regel in regelsInData)                      //  Voor elke regel die we hebben gesplitst
        {
            string regelSchoon = regel.Trim();                      // Schoon de inhoud op (Verwijderd overbodige spaties/witregels).

            Beroep beroep = VerplaatsBeroepNaarClass(regelSchoon);  // Stuur de regel door naar de functie die het omzet naar een class.

            if(beroep == null)                                      // Als er een leeg beroep wordt gevonden, slaan we deze over.
                continue;

            LogBeroepInhoud(beroep);                                // Log de inhoud van een beroep (Gebruikt als test voor nu)
        }
    }
    
    // Omdat de data in Dominees.nl door Frans Verkade zelf word ingevoerd,
    // is het voor hem makkelijk gemaakt om de data te splitsen met ';'
    // Deze functie haalt alle data weer los van elkaar en stopt het
    // in een nieuwe class. Dit kan later in een UI makkelijk worden weergegeven.
    private Beroep VerplaatsBeroepNaarClass(string beroepRegel)
    {
        string[] velden = beroepRegel.Split(";");                          // Splitst alle velden in aparte strings. De velden zijn in de database namelijk gescheiden door een ':'

        // Sla ongeldige/kop-regels over.
        // Er zijn blijvoorbeeld regels als:
        // ";;;;;-;;" en "; voornemen beroep ;;;;;;"
        if (velden.Length < 7)                                             
            return null;
        
        if(string.IsNullOrWhiteSpace(velden[1]))
            return null;
        
        Beroep beroep = new Beroep();                                     // Maak een nieuwe instantie van een 'Beroep' class aan.

        // Vul elk veld 1 voor 1 in de juiste volgorde in:
        beroep.ber = velden[0].Trim();
        beroep.gemeente = velden[1].Trim();
        beroep.persoon = velden[2].Trim();
        beroep.herkomst = velden[3].Trim();
        beroep.beslissing = velden[4].Trim();
        beroep.ber2 = velden[5].Trim();
        beroep.datum = velden[6].Trim();

        return beroep;
    }

    // Een simpele test-functie om de inhoud van een beroep
    // in de console te loggen, zodat ik kan zien of
    // de data goed gesplitst wordt.
    public void LogBeroepInhoud(Beroep beroep)
    {
        Debug.Log(
            $"Beroep ontvangen:\n" +
            $"ber: {beroep.ber}\n" +
            $"gemeente: {beroep.gemeente}\n" +
            $"persoon: {beroep.persoon}\n" +
            $"herkomst: {beroep.herkomst}\n" +
            $"beslissing: {beroep.beslissing}\n" +
            $"ber2: {beroep.ber2}\n" +
            $"datum: {beroep.datum}"
        );

        // Vul een tekstelement in om in de UI ook de beroepen te kunnen zien.
        beroepenOutputText.text +=                              // Nieuwe beroepen plaatsen.
            $"Beroep ontvangen:\n" +
            $"ber: {beroep.ber}\n" +
            $"gemeente: {beroep.gemeente}\n" +
            $"persoon: {beroep.persoon}\n" +
            $"herkomst: {beroep.herkomst}\n" +
            $"beslissing: {beroep.beslissing}\n" +
            $"ber2: {beroep.ber2}\n" +
            $"datum: {beroep.datum} \n" +
            $"\n ====================================== \n \n"; //Simpele scheider + een witregel voor overzichtelijkheid.
    }
}
