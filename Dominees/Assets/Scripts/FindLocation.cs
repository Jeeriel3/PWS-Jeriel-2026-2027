using UnityEngine;
using UnityEngine.Networking;               // Gebruikt voor UnityWebRequest
using System.Collections;                   // Gebruikt voor IEnumerator
using System.Collections.Generic;           // Gebruikt voor lijsten
using System.Text.RegularExpressions;       // Gebruikt voor Regex
using System.Net;                           // Gebruikt voor WebUtility.HtmlDecode
using System;                               // Gebruikt voor Uri.UnescapeDataString

public class FindLocation : MonoBehaviour
{
    private Resultaat resultaat = new Resultaat();      // Een instantie van de 'Resultaat' class om de resultaten in op te slaan.

    public string kerknaam;                             // De kerknaam van het zoekrtesultaat.
    public string kerkadres;                            // Het adres van het zoekresultaat.
    public string kerkwebsite;                          // De website van het zoekresultaat.
    public string kerkafbeelding;                       // Een url naar een afbeelding van de kerk.
    public string kerkLogo;                             // Het logo van de kerk.

    public string succes = null;                        // Een boolean die aangeeft of het zoeken succesvol was.

    // Een simpele api die het adres van een kerk kan vinden op basis van de naam van de kerk.
    public IEnumerator FindKerkInfo(string kerknaam, System.Action<bool> callback)
    {
        succes = null;                                                                  // Reset de 'succes' string
        resultaat = new Resultaat();                                                    // Maak een nieuw leeg 'Resultaat' aan
        StartCoroutine(ZoekKerk(kerknaam));                                             // Start de zoek-coroutine
        yield return new WaitUntil(() => succes != null);                               // Wacht totdat er een resultaat binnen is, true of false
        callback(succes == "true");                                                     // En stuur het resultaat terug
    }

    // Een coroutine om de kerk te zoeken op basis van de naam.
    // Dit doe ik door gebruik te maken van DuckDuckGo HTML.
    // Daarvoor moet je een formulier aanmaken en de naam van de kerk doorsturen naar DuckDuckGo.
    IEnumerator ZoekKerk(string kerknaam)
    {
        var form = new WWWForm();                                                       // Maak een nieuw formulier aan.
        form.AddField("q", kerknaam);                                                   // Voeg de naam van de kerk toe aan het formulier.
        form.AddField("kl", "nl-nl");                                                   // Geef aan dat we Nederlandse resultaten willen.

        using (var www = UnityWebRequest.Post("https://html.duckduckgo.com/html/", form))   // Stuur het formulier naar DuckDuckGo.
        {
            www.SetRequestHeader("User-Agent", "Modzilla/5.0 ...");                     // Stel de User-Agent in om te voorkomen dat DuckDuckGo de request blokkeert.
            yield return www.SendWebRequest();                                          // Stuur de request en wacht op de response.

            if (www.result != UnityWebRequest.Result.Success)                           // Als de request niet succesvol is...
            {
                Debug.LogWarning("Zoeken mislukt: " + www.error);                       // Loggen we de error,
                resultaat.succes = false;                                               // zetten we de succes-boolean
                succes = "false";                                                       // en string op 'false'.
                yield break;                                                            // en stoppen we met de coroutine.
            }

            string html = www.downloadHandler.text;                                     // Haal de HTML op van de response.

            VerwerkResultaten(html);                                                    // Stuur de HTML door naar een verwerk-functie die de resultaten eruit haalt.
        }
    }

    // Een resultaat is een <a ...class="result__a"...> ... </a>
    void VerwerkResultaten(string html)
    {
        List<Resultaat> resultaten = new List<Resultaat>();                             // Maak een lijst aan om de resultaten in op te slaan. 
        
        // Stap 1: vind alle resultaat-links in de HTML
        string patroon = "(<a[^>]*result__a[^>]*>)(.*?)</a>";
        MatchCollection links = Regex.Matches(html, patroon, RegexOptions.Singleline | RegexOptions.IgnoreCase);    // Zoek alle matches in de HTML.
        
        foreach(Match link in links)
        {
            string openendeTag = link.Groups[1].Value;                                  // De volledige <a ...> tag.
            string titleHtml = link.Groups[2].Value;                                    // De tekst tussen de <a> en </a> tags. Bijvoorbeeld: "Wij zijn <b>NGK</b> Eudokiakerk"

            // Stap 2: Haal de href uit de openende '<a>' tag. Dit is de URL van het resultaat.
            Match hrefMatch = Regex.Match(openendeTag, "href=\"([^\"]*)\"");
            if(!hrefMatch.Success) continue;                                            // Als er geen href gevonden is, sla dit resultaat over.
            string href = WebUtility.HtmlDecode(hrefMatch.Groups[1].Value);             // Decodeer de URL.

            // Stap 3: Haal de titel uit de tekst tussen de <a> en </a> tags. Dit is de titel van het resultaat.
            string url = href;
            Match uddgMatch = Regex.Match(href, "uddg=([^&]+)");                        // Zoek naar de 'uddg' parameter in de URL.
            if(uddgMatch.Success)
            {
                url = Uri.UnescapeDataString(uddgMatch.Groups[1].Value);                // Decodeer de 'uddg' parameter als deze aanwezig is.
            }
            else if(href.StartsWith("//"))
            {
                url = "https:" + href;                                                  // Voeg 'https:' toe aan URLs die beginnen met '//'.
            }

            // Stap 4: Maak de titel schoon
            string titel = Regex.Replace(titleHtml, "<[^>]+>", "");                     // Verwijder HTML-tags uit de titel.
            titel = WebUtility.HtmlDecode(titel);                                       // Decodeer HTML-entiteiten in de titel.
            titel = Regex.Replace(titel, "\\s+", " ");                                  // Haal de dubbele spaties weg.

            // Stap 5: Sla advertenties en interne links over.
            if(!url.StartsWith("http")) 
                continue;                                                               // Sla links over die niet beginnen met 'http'.
            if(url.Contains("duckduckgo.com/")) 
                continue;                                                               // Sla links over die naar DuckDuckGo zelf verwijzen.

            Resultaat resultaat = new Resultaat();                                      // Maak een nieuw 'Resultaat' aan.
            resultaat.naam = titel;                                                     // Zet de naam van het resultaat.
            resultaat.url = url;                                                        // Zet de URL van het resultaat.
            resultaat.succes = true;                                                    // Zet de succes-boolean op true.
            resultaten.Add(resultaat);                                                  // Voeg het resultaat toe aan de lijst van resultaten.
        }

        Resultaat besteResultaat = resultaten.Count > 0 ? resultaten[0] : null;         // Kies het eerste resultaat als het beste resultaat.
        StartCoroutine(VindKerkInformatie(besteResultaat.url, besteResultaat));         // Start een coroutine om het adres en een afbeelding van het beste resultaat te vinden.

        /*
        foreach(Resultaat resultaat in resultaten)                                      // Voor elk resultaat...
        {
            StartCoroutine(VindAdres(resultaat.url, resultaat));                        // Starten we een coroutine om het adres te vinden.
        }
        */
    }

    // Een coroutine om het adres en een afbeelding van de website te vinden.
    IEnumerator VindKerkInformatie(string url, Resultaat resultaat)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);                                 // Haal data op uit de url.
        yield return www.SendWebRequest();                                              // Return de data naar de UnityWebRequest.

        if (www.result != UnityWebRequest.Result.Success)                               // Als de request niet succesvol is...
        {
            Debug.LogError($"Error for {url}: {www.error}");                            // Loggen we de specifieke error in de console
            yield break;                                                                // En stoppen we met de coroutine.
        }
        else
        {
            string html = www.downloadHandler.text;                                     // Haal de HTML op van de response.
            string adres = ExtractAdres(html);                                          // Stuur de HTML door naar een functie die het adres eruit haalt.
            string afbeelding = ExtractAfbeelding(html, url);                           // Stuur de HTML door naar een functie die een afbleeding eruit haalt.
            string logo = 
            $"https://www.google.com/s2/favicons?domain={new Uri(url).Host}&sz=128";    // Stel het logo van de kerk in door gebruik te maken van de favicon-dienst van Google.
            resultaat.adres = adres;                                                    // Zet het adres in het resultaat.
            resultaat.afbeeldingUrl = afbeelding;                                       // Zet de afbeeldingUrl in het resultaat.

            // Aangezien dit de laatste stap is kunnen we hieronder voor nu alle data van
            // het resultaat loggen in de console.
            Debug.Log($"Resultaat: \n Naam:    {resultaat.naam}\n Adres:    {resultaat.adres} \n Bron:    {resultaat.url} \n Afbeeding: {resultaat.afbeeldingUrl}");

            // Ook kunnen we het resultaat opslaan in de klasse-variabelen zodat we deze later kunnen gebruiken.
            kerknaam = resultaat.naam;                                                  // Zet de kerknaam in de klasse-variabele.
            kerkadres = resultaat.adres;                                                // Zet het kerkadres in de klasse-variabele.
            kerkwebsite = resultaat.url;                                                // Zet de kerkwebsite in de klasse-variabele.
            kerkafbeelding = resultaat.afbeeldingUrl;                                   // Zet de kerkafbeelding in de klasse-variabele.
            // Stel de 'succes' variabele in:
            if(resultaat.succes == true)
                succes = "true";
            else
                succes = "false";
        }
    }

    string ExtractAdres(string html)
    {
        string tekst = HtmlNaarTekst(html);                                             // Converteer de HTML naar platte tekst in een aparte functie.
        string[] regels = tekst.Split('\n');                                            // Splits de tekst in regels op basis van nieuwe regels.

        // Stel een patroon in om de postcode en straat te vinden. Dit is een regex die zoekt naar een postcode gevolgd door een straatnaam.
        string postcodePatroon = "(\\d{4}\\s?[A-Z]{2})\\s+([A-Z][\\w'’\\-]+(?:\\s[A-Z][\\w'’\\-]+){0,2})";
        string straatPatroon = "^[A-Z][\\w'’.\\- ]{2,40}?\\s\\d{1,4}\\s?[a-zA-Z]?(?:\\s?[-/]\\s?\\d{1,4}\\s?[a-zA-Z]?)?$";

        for(int i = 0; i < regels.Length; i++)                                          // Loop door alle regels.
        {
            Match postcode = Regex.Match(regels[i], postcodePatroon);                   // Zoek naar een postcode in de regel. 
            if(!postcode.Success) continue;                                             // Als er geen postcode gevonden is, ga naar de volgende regel.

            string straat = null;                                                       // Stel de straat in op null.
            string voorPostcode = regels[i].Substring(0, postcode.Index).Trim(' ', ',', '-', '|'); // Haal de tekst voor de postcode op en trim deze van spaties en speciale tekens.
            if(Regex.IsMatch(voorPostcode, straatPatroon))                              // Als de tekst voor de postcode overeenkomt met het straatpatroon...
            {
                straat = voorPostcode;                                                  // Stel de straat in op de tekst voor de postcode.
            }
            else if (i > 0 && Regex.IsMatch(regels[i - 1], straatPatroon))              // Als de tekst voor de postcode niet overeenkomt met het straatpatroon, maar de vorige regel wel...
            {
                straat = regels[i - 1];                                                 // Stel de straat in op de vorige regel.
            }

            if(straat == null) continue;                                                // Als er geen straat gevonden is, ga naar de volgende regel.

            return straat + ", " + postcode.Groups[1].Value + postcode.Groups[2].Value; // Return de straat en postcode als een string.
        }

        return null;                                                                    // Als er geen adres gevonden is, return null.
    }

    // Een functie die altijd een string van een afbeeldingURL returnt.
    // Er wordt naar relevante tags gezocht, daarna wordt de url absoluut gemaakt
    // via de 'MaakAbsoluut()' functie.
    string ExtractAfbeelding(string html, string paginaUrl)
    {
        string[] metaNamen = { "og:image:secure_url", "og:image", "twitter:image" };                            // Stel 3 Meta-tags in die betrouwbaar zijn.
        MatchCollection metaTags = Regex.Matches(html, "<meta[^>]+>", RegexOptions.IgnoreCase);                 // Zoek naam een matchende tag.

        foreach(string naam in metaNamen)
        {
            foreach(Match meta in metaTags)
            {
                string tag = meta.Value;
                if(!Regex.IsMatch(tag, "(property|name)\\s*=\\s*[\"']" + Regex.Escape(naam) + "[\"']", RegexOptions.IgnoreCase))
                    continue;                                                                                   // Als we geen match vinden, skippen we deze metaTag.
                
                Match content = Regex.Match(tag, "content\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase); // Sla de Match op.
                if(!content.Success)
                    continue;                                                                                   // Als we een error tegekomen, skippen we deze regel alsnog
                
                string url = MaakAbsoluut(WebUtility.HtmlDecode(content.Groups[1].Value), paginaUrl);           // Stuur de contentGroup naar de 'MaakAbsoluut()' functie, om er een volle url van te maken.
                
                return url;                                                                                     // Stuur deze url terug.
            }

            foreach (Match img in Regex.Matches(html, "<img[^>]+>", RegexOptions.IgnoreCase))
            {
                Match src = Regex.Match(img.Value, "(?:data-src|src)\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase);
                if(!src.Success)
                    continue;                                                                                   // Als we een error tegekomen, skippen we deze regel alsnog

                string url = MaakAbsoluut(WebUtility.HtmlDecode(src.Groups[1].Value), paginaUrl);               // Stuur de contentGroup naar de 'MaakAbsoluut()' functie, om er een volle url van te maken.
                
                return url;                                                                                     // Stuur deze url terug.
            }
        }

        return null;                                                                                            // Als we niks vinden, returen we 'null'.
    }

    // Een simpele functie om van bijvoorbeeld 'img/kerk.jpg' een volle url te maken.
    string MaakAbsoluut(string waarde, string paginaUrl)
    {
        if(Uri.TryCreate(new Uri(paginaUrl), waarde, out Uri absoluut) && (absoluut.Scheme == "http" || absoluut.Scheme == "https"))
        {
            return absoluut.AbsoluteUri;                                                                    // Stuur de absolute Uri terug.
        }
        return null;                                                                                        // URI's en onzin vallen af, en sturen we dus 'null' terug.
    }

    // Een HTML naar tekst converter die dingen als scripts, styles en tags verwijdert.
    // Dit is handig om bijvoorbeeld te gebruiken bij het zoeken naar een adres in de HTML van een website.
    public string HtmlNaarTekst(string html)
    {
        // Scripts, styles en commentaar bevatten geen zichtbare tekst, dus: Weg ermee!
        html = Regex.Replace(html, "<(script|style|noscript|svg)[^>]*>.*?</\\1>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);      // Scripts, styles, noscripts en svg
        html = Regex.Replace(html, "<!--.*?-->", " ", RegexOptions.Singleline);                                                                 // Commentaar

        // Tags die een nieuwe regel starten, bijvoorbeeld <p>, <br>, <div> en <h1> worden een enter.
        html = Regex.Replace(html, "</?(p|div|br|li|ul|tr|td|th|h[1-6]|footer|header|address|section|article|table)\\b[^>]*>", "\n", RegexOptions.IgnoreCase);

        // Alle andere tags verwijdren en speciale tekens decoderen.
        html = Regex.Replace(html, "<[^>]+>", "");
        html = WebUtility.HtmlDecode(html).Replace('\u00a0', ' ');                                                                              // Decodeer HTML-entiteiten en vervang non-breaking spaces door gewone spaties.
        
        // Spates opruimen en lege regels weggooien.
        List<string> regels = new List<string>();                                                                                               // Maak een lijst met alle regels in de overgebleven HTML.
        foreach(string regel in html.Split("\n"))                                                                                               // Splits alle regels op '\n' en voor elke regel...
        {
            string schoon = Regex.Replace(regel, "[ \\t]+", " ").Trim();                                                                        // Worden dubbele spaties en tabs vervangen door een enkele spatie en trim de regel.
            if (schoon.Length > 0)
            {
                regels.Add(schoon);                                                                                                             // En wordt elke niet-lege regel toegevoegd aan de lijst van regels.
            }
        }
        return string.Join("\n", regels.ToArray());                                                                                             // Voeg alle regels weer samen en retun deze.
    }
}

// Een simpele 'Resultaat' class om de titel en URL van een zoekresultaat in op te slaan.
public class Resultaat
{
    public string naam;             // De titel van het resultaat.
    public string url;              // De URL van het resultaat.
    public string adres;            // Het adres van het resultaat.
    public string afbeeldingUrl;    // De url van een eventuele afbeelding van de kerk of het logo.
    public string logo;             // De url van het logo van de kerk. Dit maakt gebruik van de favicon-dienst van Google.

    public bool succes;             // Een boolean die aangeeft of het zoeken succesvol was.
}