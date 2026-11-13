# PWS Jeriël 2026-2027

Deze repository bevat de volledige broncode van het Unity-project van de Dominees app gemaakt voor mijn profielwerkstuk. Ik heb veel tijd besteed aan het goed opmaken van de code en het voorzien van commentaal voor (bijna) elke regel. Neem vooral een kijkje in de belangrijke bestanden, of bekijk het [productontwerp](Productontwerp/productontwerp.md).

>[!NOTE]
> Aangezien ik de volledige broncode van de Unity app heb geupload, staan in deze repository heel bestanden van Unity zelf. Dit maakt het mogelijk om zelf de broncode (mapje `Dominees`) te downloaden en te openen in Unity. Alle bestanden die ik heb geschreven staan in [`Dominees/Assets/`](Dominees/Assets). Dit is dus ook de map waar je moet kijken om mijn app te vinden :)

## Belangrijke scripts:
**[`Assets/Scripts/MainScene/MenusManager.cs`](Dominees/Assets/Scripts/MainScene/MenusManager.cs) (Het 'hart'):**
Dit script bevat alle belangrije logica voor het 'hart' van de app. Zo vindt je hier de logica voor de `bottom navigation`, dataverwerking en controle van animaties en panelen (zoals het `filterpaneel`).

**[`Assets/Scripts/RequestData.cs`](Dominees/Assets/Scripts/RequestData.cs) (De logica voor data-requests en -verwerking):**
Dit is het originele script wat ik heb geschreven om deelvraag 4 te beantwoorden (zie hiervoor ook het eindverslag 4.3). Deze is niet aangepast sinds het inleveren van het onderzoeksdeel. Daarna heb ik de logica verplaatst naar `MenusManager.cs` (Zie het kopje hierboven).

**[`Assets/Scripts/Data.cs`](Dominees/Assets/Scripts/Data.cs) (De databron van de app):**
Dit script bevat alle custom classes zoals `Data.Beroep`, `Data.Dominee` en `Data.Gemeente`. Ook vind je in de script `static` functies. Deze zijn vanuit de hele app aan te roepen. Voorbeelden hiervan zijn `Data.FromatteerDatum(string input)`. Zo kan ik vanuit elk script snel een datum goed formatteren. Ook staat in dit script de hele opzet van de app uitgeled zoals verhoudingen van verschillende velden met elkaar, en een uitleg wat elk veld inhoudt in vergelijking met hoe die data op Dominees.nl te vinden is.
