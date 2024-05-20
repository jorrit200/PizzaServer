# Pizza server
Deze solution is onderdeel van de pizza casus opdaracht HBO ICT jaar 2 periode 3 (herkansing periode 4). Ingeleverd door
Dimitri Westerveld en Jorrit de Haan.

Deze solution bevat alleen de server, de ingeleverde inhoud bevat ook een client. Beide maken gebruik van het pizza protocol. 
(Dit protocol word in meer detail bescreven in de bijgelverde documentatie, los van de solutions)

## De code
We beginnen bij de elgante, maar weinig zegende [PizzaServer.cs](./PizzaServer.cs).

### De server starten
Hier word een server object aangemaakt, afhankelijk van het protcol. Aan deze server worden een aantal Observers gehangen, 
het is dus al duidelijk dat we hier te maken hebben met een ***observer pattern***. Vervolgens word de server gestart, wat een eindelose listen and response loop begint.

Laten we eens kijken naar de `Attach` method. Hier wordt een observer attached aan een request-type. Een request-type is een concept van het pizza protocol, en kan worden gezien als een endpoint.
De server roept nu deze Observer's Update method aan. Het is nu deze observer's verantwoordelijkheid om alle request van dit type te behandelen.

### Een observer
Laten we naar één van de gekoppelde Observers kijken, [MenuObserver](./Observers/MenuObserver.cs).


