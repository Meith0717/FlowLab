import matplotlib.pyplot as plt
import pandas as pd

lst = {"Quadratische Suche": "20240814_145637", "Räumliche Suche": "20240814_151036"}

plt.figure(figsize=(10, 5))
plt.rcParams['font.family'] = 'serif'
plt.rcParams['font.size'] = 17

for name, _dir in lst.items(): 
    # Read the data from the file
    performance_file = f"analytics\\{_dir}\\performance.csv"
    constants_file = f"analytics\\{_dir}\\constants.csv"
    performance = pd.read_csv(performance_file)
    constants = pd.read_csv(constants_file)

    smoothedPerformance = performance['frameDuration'].ewm(span=500).mean()
    # Plot
    plt.plot(performance['particleCount'], smoothedPerformance, linestyle='-', linewidth=2, label=name)

plt.xlabel('Anzahl an Partikel')
plt.ylabel('Simulationsschrittzeit (ms)')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig(r"report\\graphics\\Nachbarschafts-Suche.png")
