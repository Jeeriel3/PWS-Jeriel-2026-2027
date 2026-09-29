using UnityEngine;
using System.Collections;

using UnityEngine.Networking;       // Gebruikt voor het ophalen van data via UnityWebRequest.

using System;                       // Gebruikt voor JsonUtility.
using Newtonsoft.Json.Linq;         // Newsoft.Json extentie (Zie: https://www.newtonsoft.com/json)

using TMPro;                        // Gebruikt om een simpele inputField te gebruiken om beroepen van verschillende data op te halen.
using UnityEngine.UI;                        

public class RequestData : MonoBehaviour
{

    // 2 simpele references om een UI werking te testen.
    public TMP_InputField TestingInput;
    public TMP_Text beroepenOutputText;

    public ScrollRect rect;

    // Zodra de app opstart, runt Unity automatisch deze functie.
    void Start()
    {
        // Roep de Fetch coroutine aan.
        // StartCoroutine(FetchData(fetchUrl));
    }

    // 3 simpele functies om een URL samen te stellen vanuit een InputField in de Unity UI.
    public void GetBeroepenFromInputField()
    {
        GetBeroepen(TestingInput.text);
    }
    public void GetGemeenteFromInputField()
    {
        GetGemeente(TestingInput.text);
    }
    public void GetOverledenFromInputField()
    {
        GetOverleden(TestingInput.text);
    }

    // Publieke API om beroepen op te halen vanuit het jaarweeknummer.
    // deze functie verwacht dus een input als '202638' om alle beroepen uit week 38 van 2026 op te halen. 
    // Ook kan je 'vandaag' invullen om alle beroepen van de huidige week snel op te halen.
    public void GetBeroepen(string input)
    {
        if(input == "vandaag")                                                                                  // Als er 'vandaag' als input wordt gegeven
        {
            DateTime now = DateTime.Now;                                                                        // Vragen we de huidige systeemtijd op
            string jaarweenr = $"{now.Year}{System.Globalization.ISOWeek.GetWeekOfYear(now)}";                  // En stellen we een jaarweeknr samen van de huidige tijd.

            StartCoroutine(FetchData($"https://www.dominees.nl/GetBeroepen.php?q={jaarweenr}", "beroepen"));    // Dan kan de data worden opgehaald.
        }
        else
        {   
            StartCoroutine(FetchData($"https://www.dominees.nl/GetBeroepen.php?q={input}", "beroepen"));        // Als de input gewoon een jaarweeknummer is, kan de data direct worden opgehaald.
        }
    }

    // Publieke API om alle data van een gemeente op te halen vanuit een specifieke url.
    public void GetGemeente(string id)
    {
        StartCoroutine(FetchData($"https://dominees.nl/GetGemeentes.php?q={id}", "gemeente"));
    }

    // Publieke API om overleden dominees op te halen vanuit het huidige jaar.
    // deze functie verwacht dus een input als '2026' om alle beroepen uit 2026 op te halen. 
    // Ook kan je 'vandaag' invullen om alle beroepen van het huidige jaar snel op te halen.
    public void GetOverleden(string input)
    {
        if(input == "vandaag")                                                                                  // Als er 'vandaag' als input wordt gegeven
        {
            DateTime now = DateTime.Now;                                                                        // Vragen we de huidige systeemtijd op

            StartCoroutine(FetchData($"https://www.dominees.nl/GetOverleden.php?q={now.Year}", "overleden"));   // Dan kan de data worden opgehaald vanuit het huidige jaar.
        }
        else
        {   
            StartCoroutine(FetchData($"https://www.dominees.nl/GetOverleden.php?q={input}", "overleden"));      // Als de input gewoon een jaar is, kan de data direct worden opgehaald.
        }
    }


    // Een simpele Coroutune om data op te halen.
    // Dominees.nl heeft een pagina die de data in een .json formaat teruggeeft.
    // De tekst uit deze webpagina kan je ophalen via UnityWebRequest. 
    // (Zie Eindverslag - Stap 4 - Deelvraag: Op welke technische manier kan de data van Dominees.nl efficiënt en stabiel worden ingeladen en verwerkt in mijn app?)
    IEnumerator FetchData(string url, string type)
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
            string resultaat = www.downloadHandler.text;
            Debug.Log($"Data susccesvol verkregen: {resultaat}");                   // Log het reusltaat als text. (Dit kunnen we later verwerken als string)

            // Er zijn 3 type data die mijn app (tot nu toe) van Dominees.nl kan ophalen.
            // Dit zijn beroepen, gemeentes en overleden dominees.
            // Voor elk type is een andere manier van dataverwekring nodig, dus heb ik
            // een specifieke functie gemaakt voor elk type data. De juiste functie wordt hier gekozen.
            if(type == "beroepen")
                VerwerkBeroepen(resultaat);
            if(type == "gemeente")
                VerwerkGemeente(resultaat);
            if(type == "overleden")
                VerwerkOverleden(resultaat);
        }
    }

    // Omdat de data die we verkrijgen niet overteenkomt met een normaal .json bestand moeten we eerst de data verwerken.
    // Deze functie splitst alle verkregen data op in aparte regels, om ze vervolgens te verwerken.
    // Daarvoor gebruik ik een package genaamd Newsoft Json. (Zie: https://www.newtonsoft.com/json)
    private void VerwerkBeroepen(string data)
    {
        data = data.Trim();                                         // Versimpel de string (Verwijderd overbodige spaties/witregels).

        JObject json = JObject.Parse(data);                         // De hele .json tekst wordt omgezet naar een JObject. Hier kan Newsoft.Json makkelijk mee werken.

        JObject eersteEntry = (JObject)json.First.First;            // Pak de "0" weg uit de Json, zodat hier geen problemen mee komen.

        string beroepenText = eersteEntry["beroepen"].ToString();   // Pak de "beroepen" data uit de JObject en zet deze om naar een string.
            
        string[] regelsInData = beroepenText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);        //  Splits de data per regel ('\r\n' = nieuwe regel).

        foreach (string regel in regelsInData)                      //  Voor elke regel die we hebben gesplitst.
        {
            string regelSchoon = regel.Trim();                      // Schoon de inhoud op (Verwijderd overbodige spaties/witregels).

            Beroep beroep = VerplaatsBeroepNaarClass(regelSchoon);  // Stuur de regel door naar de functie die het omzet naar een class.

            if(beroep == null)                                      // Als er een leeg beroep wordt gevonden, slaan we deze over.
                continue;    

            LogBeroepInhoud(beroep);                                // Log voor nu de inhoud van het beroep.
        }
    }

    // De beroepen van een gemeente zijn op een vreselijke manier vormgegeven in de database.
    // Bovenaan staat een datum. Dit is de vorming van de kerk.
    // Daarna volgt een scheiding in de vorm van '==='.
    // Vervolgens staat op elke regel een dominee. Deze bevat verschillende informatie. Deze is te vinden in Data.GemeenteDominee
    // De vaccature eindigd weer met een scheiding van '==='.
    //
    // Daaronder staan de echte vaccatures. Deze starten met '==='.
    // Elk beroep is daaronder dan weer gescheiden met '---'.
    // Voor elk beroep staat ook hier weer een begindatum. (Vaccature.begindatum)
    // En elk beroep heeft weer een lijst met beroep-regels, zoals als verwerkt worden in VerwerkBeroepen().
    //
    // Voor een voorbeeld van deze opzet, bekijk onderstaande link, dit is namelijk aardig moeilijk uit te leggen!
    // https://github.com/Jeeriel3/PWS-Jeriel-2026-2027/blob/main/voorbeeld_beroepingsdata_gemeente.txt
    private void VerwerkGemeenteBeroepen(string data, Gemeente gemeente)
    {
        int eersteScheiding = data.IndexOf("===");                                      // Er zijn dus 2 datasets. De tweede dataset begint na de tweede '==='. Zoek eerst de eerste '===' op en slaan deze op in een int.
        int tweedeScheiding = -1;                                                       // Maak een int die direct op -1 staat voor de 2e scheiding.

        string bovensteData = data.Substring(eersteScheiding + 3);                      // Maak een substring voor de bovenste data, om te verwerken naar beroepen.  

        // Voordat de vaccatures worden bekeken moeten eerst alle GemeenteDominees worden ingesteld.
        // Daarom worden eerst alle regels gesplitst in een aparte array met de naam 'regels'.
        string[] regels = bovensteData.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );

        // De volgende logica doen we voor elke regel in de array.
        // Dit wordt dus voor elke vaccature en elk beroep herhaald.
        foreach (string regel in regels)
        {
            string regelSchoon = regel.Trim();                                          // Versimpel de string (Verwijderd overbodige spaties/witregels).

            string[] velden = regelSchoon.Split(';');                                   // Maak een array voor alle velden. Deze zijn gesplitst door ';'

            // Als er 7 of meer velden zijn en het eerste veld niet leeg is, is dit een geldige vaccature.
            if (velden.Length >= 7 && !string.IsNullOrWhiteSpace(velden[0]))
            {
                GemeenteDominee huidigeDominee = new GemeenteDominee();                 // Maak een nieuwe 'GemeenteDominee' aan.

                huidigeDominee.ber = velden[0].Trim();
                huidigeDominee.naam = velden[1].Trim();
                huidigeDominee.intrede = velden[2].Trim();
                huidigeDominee.herkomst = velden[3].Trim();
                huidigeDominee.afscheid = velden[4].Trim();
                huidigeDominee.vervolg = velden[5].Trim();
                huidigeDominee.ber2 = velden[6].Trim();

                gemeente.dominees.Add(huidigeDominee);                                  // En voeg de dominee toe aan de 'dominees' lijst in de 'Gemeente' class.

                continue;                                                               // Dan kunnen we door.
            }
        }

        tweedeScheiding = data.IndexOf("===", eersteScheiding + 3);                     // Zoek daarna de tweede scheiding door de eerste scheiding + 3 te doen.

        data = data.Substring(tweedeScheiding + 3);                                     // Alles na de tweede === is de dataset die we willen hebben.

        // Maak daarna voor elke regel een eigen entery aan in de 'regel' array.
        string[] regels1 = data.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );

        Vacature huidigeVacature = null;                                                // Maak een nieuwe Vaccature aan en noem deze 'huidigeVaccature'. Deze kunnen we later invullen.

        // De volgende logica doen we voor elke regel in de array.
        // Dit wordt dus voor elke vaccature en elk beroep herhaald.
        foreach (string regel in regels1)
        {
            string regelSchoon = regel.Trim();                                           // Versimpel de string (Verwijderd overbodige spaties/witregels).

            if (string.IsNullOrWhiteSpace(regelSchoon))                                  // Als er een lege regel wordt gevonden, slaan we deze over.
                continue;

            // Eerst komt een nieuwe vaccature. Deze heeft veel ';' en 1 jaartal.
            // Bijvoorbeeld: ';1893;;;;' of ';1903;;;;'
            string[] velden = regelSchoon.Split(';');                                    // Maak een array voor alle velden. Deze zijn gesplitst door ';'

            // Als er 2 of meer velden zijn, het eerste veld niet leeg is en het tweede veld een datum is, is dit een geldige vaccature.
            if (velden.Length >= 2 && string.IsNullOrWhiteSpace(velden[0]) && int.TryParse(velden[1].Trim(), out int jaar))
            {
                huidigeVacature = new Vacature();                                       // Maak een nieuwe vaccature aan.
                huidigeVacature.begindatum = jaar.ToString();                           // Stel de begindatum in

                gemeente.beroepen.Add(huidigeVacature);                                 // En voeg de vaccature toe aan de lijst in de 'Beroepen' class.

                continue;                                                               // Dan kunnen we door.
            }

            if (regelSchoon == "---")                                                   // Als we een nieuwe scheider tegenkomen:
            {
                huidigeVacature = null;                                                 // Zet de huidige vaccature op 'null'
                continue;                                                               // En ga door.
            }

            if (huidigeVacature == null)
                continue;

            if (velden.Length < 5)                                                      // Zolang er minder dan 5 velden zijn,
                continue;                                                               // kunnen we door

            // Nu kan het beroep gescheiden worden.
            // Alle velden die eerder zijn losgemaakt,
            // kunnen hieronder worden ingevuld.
            Beroep beroep = new Beroep();                                               // Dan maken we een nieuw 'Beroep' aan.

            // En vullen we alle velden die nodig zijn in.
            beroep.ber = velden[0].Trim();                                           
            beroep.datum = velden[1].Trim();
            beroep.persoon = velden[2].Trim();
            beroep.herkomst = velden[3].Trim();
            beroep.ber2 = velden[4].Trim();

            huidigeVacature.beroepen.Add(beroep);                                       // En voeg het beroep toe aan de lijst met beroepen in de huidige vaccature.
        }
    }

    private bool IsDatumRegel(string regel)
    {
        string datum = regel.Trim().TrimEnd(';').Trim();

        DateTime resultaat;

        return DateTime.TryParseExact(
            datum,
            "dd-MM-yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out resultaat
        );
    }

    private void VerwerkGemeente(string data)
    {
        data = data.Trim();                                         // Versimpel de string (Verwijderd overbodige spaties/witregels).

        JObject json = JObject.Parse(data);                         // De hele .json tekst wordt omgezet naar een JObject. Hier kan Newsoft.Json makkelijk mee werken.

        Gemeente gemeente = new Gemeente();                         // Maak een nieuwe gemeente-object aan.

        // Voor alle enteries in de array zoek ik apart alles op
        gemeente.gemeente = json["0"]["gemeente"].ToString();       // De naam van de gemeente.
        gemeente.gemeenteid = json["0"]["gemeenteid"].ToString();   // De id van de gemeente.
        gemeente.classis = json["0"]["classis"].ToString();
        gemeente.provincie = json["0"]["provincie"].ToString();
        gemeente.ring = json["0"]["ring"].ToString();

        // De beroepen staan in dit tabel hetzelfde als in de beroepen tabel.
        // Daarom wordt elk beroep apart behandeld en worden ze daarna aan de list 
        // in de 'Gemeente' class toegevoegd.
        VerwerkGemeenteBeroepen(json["0"]["beroepen"].ToString(), gemeente);

        // De gemeente staat in Dominees.nl als tekst met 'ja' of 'nee' opgeslagen.
        // Deze zet ik om in een bool, zodat er makkelijker mee te werken is.
        bool uitzending;
        if (json["0"]["uitzending"].ToString() == "ja")
            uitzending = true;
        else
            uitzending = false;
        gemeente.uitzending = uitzending;

        gemeente.bijgewerkt = json["0"]["bijgewerkt"].ToString();
        gemeente.voortgekomen = json["0"]["voortgekomen"].ToString();
        gemeente.opgegaan = json["0"]["opgegaan"].ToString();

        LogGemeenteInhoud(gemeente);                            // Log (voor nu) de informatie uit de 'Gemeente' class.
    }

    // Alle overleden dominees staan op dezelfde manier in de database als alle beroepen.
    // Deze functie splitst alle verkregen data op in aparte regels, om ze vervolgens te verwerken.
    // Daarvoor gebruik ik een package genaamd Newsoft Json. (Zie: https://www.newtonsoft.com/json)
    private void VerwerkOverleden(string data)
    {
        data = data.Trim();                                                     // Versimpel de string (Verwijderd overbodige spaties/witregels).

        JObject json = JObject.Parse(data);                                     // De hele .json tekst wordt omgezet naar een JObject. Hier kan Newsoft.Json makkelijk mee werken.

        JObject eersteEntry = (JObject)json.First.First;                        // Pak de "0" weg uit de Json, zodat hier geen problemen mee komen.

        string overledenText = eersteEntry["overleden"].ToString();             // Pak de "overleden" data uit de JObject en zet deze om naar een string.
            
        string[] regelsInData = overledenText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);        //  Splits de data per regel ('\r\n' = nieuwe regel).

        foreach (string regel in regelsInData)                                  //  Voor elke regel die we hebben gesplitst.
        {
            string regelSchoon = regel.Trim();                                  // Schoon de inhoud op (Verwijderd overbodige spaties/witregels).

            Overleden overleden = VerplaatsOverledenNaarClass(regelSchoon);     // Stuur de regel door naar de functie die het omzet naar een class.

            if(overleden == null)                                               // Als er een lege entery wordt gevonden, slaan we deze over.
                continue;    

            LogOverledenInhoud(overleden);                                      // Log (voor nu) alle informatie uit de 'Overleden' class.
        }
    }
    
    // Omdat de data in Dominees.nl door Frans Verkade zelf word ingevoerd,
    // is het voor hem makkelijk gemaakt om de data te splitsen met ';'
    // Deze functie haalt alle data weer los van elkaar en stopt het
    // in een nieuwe class. Dit kan later in een UI makkelijk worden weergegeven.
    private Beroep VerplaatsBeroepNaarClass(string beroepRegel)
    {
        string[] velden = beroepRegel.Split(";");                   // Splitst alle velden in aparte strings. De velden zijn in de database namelijk gescheiden door een ':'.

        if (velden.Length < 7)                                      // Sla ongeldige/kop-regels over. Er zijn blijvoorbeeld regels als: ";;;;;-;;" en "; voornemen beroep ;;;;;;".                                         
            return null;
        
        if(string.IsNullOrWhiteSpace(velden[1]))                    // Leeg beroep terugkeren als er geen data in zit.
            return null;
        
        Beroep beroep = new Beroep();                               // Maak een nieuwe instantie van een 'Beroep' class aan.

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

    // Omdat de data in Dominees.nl door Frans Verkade zelf word ingevoerd,
    // is het voor hem makkelijk gemaakt om de data te splitsen met ';'
    // Deze functie haalt alle data weer los van elkaar en stopt het
    // in een nieuwe class. Dit kan later in een UI makkelijk worden weergegeven.
    private Overleden VerplaatsOverledenNaarClass(string beroepRegel)
    {
        string[] velden = beroepRegel.Split(";");                   // Splitst alle velden in aparte strings. De velden zijn in de database namelijk gescheiden door een ':'.

        if (velden.Length < 7)                                      // Sla ongeldige/kop-regels over. Er zijn blijvoorbeeld regels als: ";;;;;-;;" en "; voornemen beroep ;;;;;;".                                         
            return null;
        
        if(string.IsNullOrWhiteSpace(velden[1]))                    // Leeg beroep terugkeren als er geen data in zit.
            return null;
        
        Overleden overleden = new Overleden();                      // Maak een nieuwe instantie van een 'Beroep' class aan.

        // Vul elk veld 1 voor 1 in de juiste volgorde in:
        overleden.naam = velden[0].Trim();
        overleden.geboortedatum = velden[1].Trim();
        overleden.overlijdensdatum = velden[2].Trim();
        overleden.gemeentes = velden[3].Trim();
        overleden.bijzonderheden = velden[4].Trim();

        return overleden;                                            // Stuur de ingevulde 'Overlden' class terug.
    }

    // Een simpele test-functie om de inhoud van een beroep
    // in de console te loggen, zodat ik kan zien of
    // de data goed gesplitst wordt.
    private void LogBeroepInhoud(Beroep beroep)
    {
        rect.verticalNormalizedPosition = 1;                        // Zet de ScrollRect helemaal naar boven (makkelijkere UI bediening - puur voor testen).

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
            $"\n ====================================== \n \n"; // Simpele scheider + een witregel voor overzichtelijkheid.
    }

    // Een simpele test-functie om de inhoud van een Gemeente
    // in de console te loggen, zodat ik kan zien of
    // de data goed gesplitst wordt.
    private void LogGemeenteInhoud(Gemeente gemeente)
    {
        rect.verticalNormalizedPosition = 1;                        // Zet de ScrollRect helemaal naar boven (makkelijkere UI bediening - puur voor testen).

        string output =
            $"Gemeente gevonden:\n" +
            $"gemeente: {gemeente.gemeente}\n" +
            $"gemeenteid: {gemeente.gemeenteid}\n" +
            $"classis: {gemeente.classis}\n" +
            $"provincie: {gemeente.provincie}\n" +
            $"uitzending: {gemeente.uitzending}\n" +
            $"bijgewerkt: {gemeente.bijgewerkt}\n" +
            $"voortgekomen: {gemeente.voortgekomen}\n" +
            $"opgegaan: {gemeente.opgegaan}\n" +
            $"\n========== DOMINEES ==========\n";

        foreach (GemeenteDominee dominee in gemeente.dominees)
        {
            output +=
                $"\n--- DOMINEE ---\n" +
                $"  ber: {dominee.ber}\n" +
                $"  naam: {dominee.naam}\n" +
                $"  intreden: {dominee.intrede}\n" +
                $"  herkomst: {dominee.herkomst}\n" +
                $"  afscheid: {dominee.afscheid}\n" +
                $"  vervolg: {dominee.vervolg}\n" +
                $"  ber2: {dominee.ber2}";
        }

        output += $"\n========== VACCATURES: ==========\n";

        foreach (Vacature vacature in gemeente.beroepen)
        {
            output +=
                $"\n--- VACATURE ---\n" +
                $"Begindatum: {vacature.begindatum}\n" +
                $"Aantal beroepen: {vacature.beroepen.Count}\n";

            foreach (Beroep beroep in vacature.beroepen)
            {
                output +=
                    $"  Beroep:\n" +
                    $"    ber:          {beroep.ber}\n" +
                    $"    persoon:      {beroep.persoon}\n" +
                    $"    datum:        {beroep.datum}\n" +
                    $"    herkomst:     {beroep.herkomst}\n" +
                    $"    beslissing:   {beroep.beslissing}\n" +
                    $"    gemeente:     {beroep.gemeente}\n" +
                    $"    ber2:         {beroep.ber2}\n";
            }
        }

        Debug.Log(output);

        // Ook in de UI zetten
        beroepenOutputText.text = output;
    }

    // Een simpele test-functie om de inhoud van een beroep
    // in de console te loggen, zodat ik kan zien of
    // de data goed gesplitst wordt.
    private void LogOverledenInhoud(Overleden overleden)
    {
        rect.verticalNormalizedPosition = 1;                        // Zet de ScrollRect helemaal naar boven (makkelijkere UI bediening - puur voor testen).
        
        Debug.Log(
            $"Overleden ontvangen:\n" +
            $"naam: {overleden.naam}\n" +
            $"geboortedatum: {overleden.geboortedatum}\n" +
            $"overlijdensdatum: {overleden.overlijdensdatum}\n" +
            $"gemeentes: {overleden.gemeentes}\n" +
            $"bijzonderheden: {overleden.bijzonderheden}"
        );

        // Vul een tekstelement in om in de UI ook de beroepen te kunnen zien.
        beroepenOutputText.text +=                              // Nieuwe beroepen plaatsen.
            $"Overleden ontvangen:\n" +
            $"naam: {overleden.naam}\n" +
            $"geboortedatum: {overleden.geboortedatum}\n" +
            $"overlijdensdatum: {overleden.overlijdensdatum}\n" +
            $"gemeentes: {overleden.gemeentes}\n" +
            $"bijzonderheden: {overleden.bijzonderheden}" +
            $"\n ====================================== \n \n"; //Simpele scheider + een witregel voor overzichtelijkheid.
    }
}