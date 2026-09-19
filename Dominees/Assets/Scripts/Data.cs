// Een simpele 'Genootschap' class om snel bij belangrijke afkortingen te kunnen.
[System.Serializable]
public class Genootschap
{
    public string officieleNaam;    // Volledige officiele naam (zoals: Nederlandse Gereformeerde Kerken).
    public string afkorting;        // De triviale afkorting (ook gebruikt in Dominees.nl), bijvoorbeeld: NGK.
    public string kerktijdenKey;    // De key die kerktijden.nl geeft aan elk genootschap (te vinden op https://api.kerktijden.nl//api/search/GetDenominationsWithParents).
}

// Een simpele 'Beroep' class die alle data uit een beroep behoud:
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