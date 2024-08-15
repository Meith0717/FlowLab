import matplotlib.pyplot as plt
import pandas as pd

plt.figure(figsize=(10, 6))
lst = {"Quadratische Suche": "20240814_145637", "Räumliche Suche": "20240814_151036"}

for name, _dir in lst.items(): 
    # Read the data from the file
    performance_file = f"{_dir}\\performance.csv"
    constants_file = f"{_dir}\\constants.csv"
    performance = pd.read_csv(performance_file)
    constants = pd.read_csv(constants_file)

    smoothedPerformance = performance['frameDuration'].ewm(span=100).mean()
    # Plot
    plt.plot(performance['particleCount'], smoothedPerformance, linestyle='-', linewidth=2, label=name)

plt.xlabel('Anzahl an Partikel')
plt.ylabel('Simulationsschrittzeit (ms)')
plt.title(f"Nachbarschafts-Suche")
plt.legend()
plt.grid(True)
plt.show()
