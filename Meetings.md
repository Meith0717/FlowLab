# Bachelor Meetings
---
## Meeting 00 (24.10.24)
- **Preparation:**
- **Questions:**
- **Notes:**
    - Relative schnelle Kondensation zum eigentlichem Thema
    - Analysen: Iterationen Konstant halten -> Dichte fehler beoachten
    - dichte fehler ändern -> Oszilation?
    - Selber zitieren
    - Alle inklusieve Bachelor projekt in Arbeit beschreiben
---
## Meeting 01 (31.10.24)
- **Preparation:**
  - Fehler an der Boundary behoben -> wurde Geschwindigkeit berechnet (unnötig)
  - Diagonal element: Gewicht wie stark ein Partikel von seinem eigenem Druck beeinflusst wird
  - Scource Therm: abweichung zur ruhedichte
- **Questions:**
  - Fehler Komvergiert nicht 
    -  Druck bleibt Null 
    - Druck Update ist kleiner 0
    - Diagonal Element ist kleiner 0

  - Diagonal Element Formel 
- **Notes:**
    - ISPH -> Lösung der Poisson Gleichung
    - IISPH -> Methode für ISHP
    - Feher Betrag berechnen
    - Omega kleiner -> Diagonal Element
    - Diagonal element 0
    - Zeitschritt fehlerquelle
---
## Meeting 02 (06.11.24)
- **Preparation:**
    - Scource Term 
        - Compression: < 0
        - Neutral: = 0
        - Expansion: > 0
    - Laplace (Pressure acceleraion Divergence) ?? 
        - To a point: ?> 0? but !< 0!
        - Same direction: = 0
        - From a point: ?< 0? but !> 0! 
- **Questions:**
    - Diagonal Element (Erklären wie funktioniert)
        - Fluid Compressed
            - Source Term < 0
            - Laplace < 0
                - sf - Ap < 0 => aii < 0
        - Fluid Ideal
            - Scource Term = 0
            - Laplace = 0
                sf - Ap = 0
        - Fluid Expansion
            - Scource Term > 0
            Wie ist es hier? 
  ---
## Meeting 03 (14.11.24)
- **Preparation:**
    - TODO
- **Questions:**
    - TODO
- **Notes:**
    - Unterschied zwichen 
      - Geschätzter Fehler Ap . si
      - Realer/Warer Fehler Am anfang des Simuationsschritt: Dichte berechnen!!!! Sollte schlechter sein
    - Geschwindigkeit visualisieren (Farbe)
    - Druk ist ein Werkzeug zum anpassen der Geschwindigkeit

    - Gamas1 und Gamma2 einführen und damit spielen -> Iterationan beobachten (hoch einstelen) kleine veränderung
        - paerikel in der Boundary besser -> mehr nachbarn
    - ???pressure boundaries over reflection????
    - Wasserseule tests -> Je höher umseo mehr i -> zeitschritt größer -> mehr I -> 
    - Motivation -> Gesamtlaufzeiten vergleichen: SE kleiner fehler kleinerer zeitschritt -> IISPH größee zeitschritte
    - Iterationen zu hoch->problem
    - Omega .7 -> sollte crashen : Omega groß -> de zu groß; Je höher umso weiniger Iterationen
# Template
## Meeting x (date)
- **Preparation:**
    - TODO
- **Questions:**
    - TODO
- **Notes:**
    - TODO
---