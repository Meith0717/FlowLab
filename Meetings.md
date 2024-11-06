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
      - Meine Gliederung für die Arbeit:
    ``` 
    1. Deckblatt: Titel der Arbeit, Name des Verfassers, Universität, Datum, etc.
    2. Abstract: Kurze Zusammenfassung der Arbeit (Thema, Ziel, Methodik, Ergebnisse).
    3. Inhaltsverzeichnis
    4. Abbildungs- und Tabellenverzeichnis
    5. Einleitung: Motivation, Zielsetzung, Forschungsfrage, Aufbau der Arbeit.
    6. Theoretischer Hintergrund: Relevante Grundlagen, Definitionen und Modelle.
        - Navier-Stokes-Gleichungen
        - Pressure Poisson Equation
        - Jacobi Iteration
        - IISPH
    7. Methodik: Vorgehensweise, Tools, und Implementierungsschritte.
    8. Ergebnisse: Darstellung und Erläuterung der durchgeführten Analysen und Ergebnisse.
    9. Diskussion: Interpretation der Ergebnisse, Vergleich mit anderen Arbeiten, Herausforderungen.
    10. Fazit: Zusammenfassung, Beantwortung der Forschungsfrage, Ausblick auf zukünftige Arbeiten.
    11. Literaturverzeichnis: Auflistung aller Quellen nach Zitierstil.
    12. Anhang: Zusatzmaterial wie Programmcode, Tabellen, etc.
    13. Eidesstattliche Erklärung
    ```
- **Notes:**
    - TODO

---
# Template
## Meeting x (date)
- **Preparation:**
    - TODO
- **Questions:**
    - TODO
- **Notes:**
    - TODO
---