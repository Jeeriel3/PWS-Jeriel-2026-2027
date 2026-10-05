using UnityEngine;
using System.Collections.Generic;   // Gebruikt voor Lists.

// Een simpele 'Gemeente' class die alle data uit een gemeente bevat.
[System.Serializable]
public class Gemeente
{
    public string gemeente;         // De naam van de gemeente.
    public string kerk;             // De naam van de kerk van de gemeente. (Bijvoorbeeld: "Westerkerk" of "Eben-Haëzerkerk")
    public string website;          // De link naar de website van de gemeente.
    public string afbeelding;       // De url van een eventuele afbeelding van de kerk. Dit kan een logo of foto van het gebouw zijn.
    public string logo;             // Het logo van de kerk. Dit is het logootje wat bovenin je Google browser staat en is wat kleine kwaliteit, maar goed te gebruiken.
    public string gemeenteid;       // De id van de gemeente.
    public string classis;          // De classis van de gemeente. (Regionaal samenwerkingsverband van protestantse kerken of gemeenten)
    public string provincie;        // De provincie waar het gebouw van de gemeente staat.
    public string adres;            // Het adres van de gemeente in het format: "{Straat} {Huisnummer}, {Postcode} {Plaatsnaam}"
    public string ring;             // Vaak leeg, maar dit is een kleinere, geografische onderverdeling binnen een classis.
    public List<Vacature> beroepen = new List<Vacature>();                  // Een lijst van alle vacatures die de gemeente heeft gehad. Deze bevat weer een lijst met alle beroepen die tijdens die vaccature zijn uitgebracht.
    public List<GemeenteDominee> dominees = new List<GemeenteDominee>();    // Een lijst van elle dominees die een gemeente heeft gehad. Deze bevat velden als naam, herkomst, vervolg, ect.
    public bool uitzending;         // In Dominees.nl tekst die 'ja' of 'nee' bevat, deze klopt lang niet altijd.
    public string bijgewerkt;       // Wanneer de data van deze gemeente het laatste is bijgewerkt.
    public string voortgekomen;     // Sommige gemeentes waren eerst onderdeel van een andere kerk, maar zijn opgesplitst. Vanuit welke gemeente is deze gemeente voortgekomen?
    public string opgegaan;         // Sommige gemeentes zijn samengegaan met een andere gemeente. In welke gemeente is deze gemeente opgegaan?
}

// Een simpele 'Vacature' class die de datum en alle beroepen van de vacature bevat.
[System.Serializable]
public class Vacature
{
    public string begindatum;      // De datum vanaf wanneer de gemeente vaccant is.
    public List<Beroep> beroepen = new List<Beroep>();  // Een lijst met alle beroepen die tijdens deze vaccature zijn uitgebracht.
}

// Een simpele 'GemeenteDominee' class die alle dominees die een gemeente heeft gehad bevat.
[System.Serializable]
public class GemeenteDominee
{
    public string ber;              // Aantal uitgebrachte beroepen in voorafgaande vacaturetijd.
    public string naam;             // Voorletter + Achternaam dominee.
    public string intrede;          // De datum van intrede.
    public string herkomst;         // De vorige gemeente waar deze dominee heeft gewerkt. Dit kan 'kandidaat' zijn.
    public string afscheid;         // De datum van afscheid/losmaking.
    public string vervolg;          // De gemeente waar de dominee naar toe is vertrokken. Dit kan 'overleden' zijn.
    public string ber2;             // Aantal uitgebrachte beroepen op predikant tijdens verblijf.
}

// Een simpele 'Beroep' class die alle data uit een beroep bevat.
[System.Serializable]
public class Beroep
{
    public string ber;              // Aantal uitgebrachte beroepen in huidige vacature door betreffende gemeente.
    public string gemeente;         // Gemeente die het beroep heeft uitgebracht.
    public string persoon;          // Voorletter + Achternaam dominee.
    public string herkomst;         // Originele gemeente van deze dominee.
    public string beslissing;       // Heeft de dominee het beroep aangenomen of bedankt? (Waar: aan = aangenomen en bed = bedankt)
    public string ber2;             // Aantal ontvangen beroepen door betreffende persoon in huidige herkomstsituatie.
    public string datum;            // De datum dat het beroep is geplaatst.
}

// Een simpele 'Overleden' class die alle data uit een overlijdensmelding bevat.
[System.Serializable]
public class Overleden
{
    public string naam;             // Voorletter + Achternaam dominee.
    public string geboortedatum;    // De geboortedatum van de overleden dominee.
    public string overlijdensdatum; // De overlijdensdatum van de overleden dominee.
    public string gemeentes;        // Een lijst met alle gemeentes waar een dominee heeft gewerkt (gescheiden door ',')
    public string bijzonderheden;   // Bevat soms een klein verhaaltje, of data over de begrafenisdienst (zoals datum, tijd en locatie).
}

// Een simpele 'Genootschap' class om snel bij belangrijke afkortingen te kunnen.
[System.Serializable]
public class Genootschap
{
    public string officieleNaam;    // Volledige officiele naam (zoals: Nederlandse Gereformeerde Kerken).
    public string afkorting;        // De triviale afkorting (ook gebruikt in Dominees.nl), bijvoorbeeld: NGK.
    public string kerktijdenKey;    // De key die kerktijden.nl geeft aan elk genootschap (te vinden op https://api.kerktijden.nl//api/search/GetDenominationsWithParents).

    public static Genootschap Instance;

    // Omzet naar volledige officiele namen zoals ze staan in de RCE dataset.
    // (Voor omzet zie:  https://data.cultureelerfgoed.nl/term/id/rn/2/364c9485-22d8-4962-8713-7fdf1910c553.html en https://dominees.nl/gemeentes.php)
    public string FormateerEnkelGenooschap(string input)
    {
        if(input == "bap")
            return "Overig";														// Staat helaas niet in de dataset... Probleem voor later :)
        if(input == "bw")
            return "Overig";														// Buitengewone wijkgemeente, geen aparte denominatie
        if(input == "cgk")
            return "Christelijke Gereformeerde Kerken in Nederland";				// Komt exact voor in de RCE-dataset
        if(input == "dg")
            return "Algemene Doopsgezinde Sociëteit";								// Enige doopsgezinde optie in de RCE-dataset
        if(input == "gg")
            return "Gereformeerde Gemeenten (in Nederland en Noord-Amerika)";		// Enige Gereformeerde Gemeenten-optie in de dataset
        if(input == "ggin")
            return "Gereformeerde Gemeenten (in Nederland en Noord-Amerika)";		// Valt hieronder
        if(input == "gk")
            return "Gereformeerde Kerken in Nederland";								// Komt exact voor in de RCE-dataset
        if(input == "gkv")
            return "Overig";														// Gereformeerde Kerken vrijgemaakt staat niet in deze RCE-lijst
        if(input == "hg")
            return "Nederlandse Hervormde Kerk";									// Hervormde Gemeente → Nederlandse Hervormde Kerk
        if(input == "hhg")
            return "Overig";														// Hersteld Hervormde Kerk staat niet in deze RCE-lijst
        if(input == "lg" || input == "elk")
            return "Evangelisch-Lutherse kerk";									    // Komt voor in de RCE-dataset
        if(input == "ngk")
            return "Nederlands Gereformeerde Kerken";					            // Nederlandse Gereformeerde Kerken staat niet in deze lijst, maar is wel te vinden op de erfgoedatlas.
        if(input == "pkn")
            return "Protestantse Kerk in Nederland";                                // Deze staat niet op Dominees.nl. Dit vindt ik onzin. Ik heb het als optie iig hier nu staan. Misschien dat ik er later nog iets mee doe.
        if(input == "r")
            return "Remonstrantse Broederschap";									// Komt exact voor in de RCE-dataset
        if(input == "veg")
            return "Overig";														// Vrije Evangelische Gemeente staat niet in deze RCE-lijst
        if(input == "vgkn")
            return "Overig";														// Voortgezette Gereformeerde Kerken staan niet in deze RCE-lijst
        if(input == "w")
            return "Waalse Kerk";													// Komt exact voor in de RCE-dataset
        if(input == "z")
            return "Overig";														// Zelfstandige gemeente is geen specifieke denominatie
        if(input == "zd")
            return "Overig";														// Zevendedags Adventisten staan niet in deze RCE-lijst
        else
            return input;
    }

    // Simpele helper-bool om te checken of een input een gemeente-afkroting is. 
    // Dit helpt om dubbele gemeentes te scheiden van stad- en wijknamen.
    public bool isGemeente(string input)
    {
        if(input == "bap")
            return true;								
        if(input == "bw")
            return true;												
        if(input == "cgk")
            return true;		
        if(input == "dg")
            return true;					
        if(input == "gg")
            return true;		
        if(input == "ggin")
            return true;
        if(input == "gk")
            return true;					
        if(input == "gkv")
            return  true;											
        if(input == "hg")
            return true;							
        if(input == "hhg")
            return true;											
        if(input == "lg" || input == "elk")
            return  true;							
        if(input == "ngk")
            return true;					         
        if(input == "pkn")
            return true;                         
        if(input == "r")
            return true;;								
        if(input == "veg")
            return true;											
        if(input == "vgkn")
            return true;		
        if(input == "w")
            return true;
        if(input == "z")
            return true;
        if(input == "zd")
            return true;														
        else
            return false;
    }
}