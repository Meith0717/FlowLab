import matplotlib.pyplot as plt
import pandas as pd

plt.figure(figsize=(10, 6))
lst = ["20240813_175321", "20240813_175708", "20240813_175940"]

plt.axhline(y=0, color='b', linestyle='--')
for Dir in lst: 

    # Read the data from the file
    data_file = f"{Dir}\\physics.csv"
    constants_file = f"{Dir}\\constants.csv"
    data = pd.read_csv(data_file)
    constants = pd.read_csv(constants_file)

    # Plot
    plt.plot(data['sample'], data['relativeDensityError'], linestyle='-', linewidth=2, label=f"Δt = {constants["TimeSteps"][0]}")

plt.ylim(-.01, .08)
plt.xlim(1000, 45000)

plt.xlabel('Simulationsschritte')
plt.ylabel('Relativer Dichtefehler')
plt.title(f"Kolonnentest mit Verschiedenen Zeitschritten Δt")
plt.legend()
plt.grid(True)
plt.show()
