import matplotlib.pyplot as plt
import pandas as pd

plt.figure(figsize=(10, 6))
lst = {"Sequentielle Verarbeitung": "20240814_153827", "Parallele Verarbeitung": "20240814_154140"}

for name, _dir in lst.items(): 
    # Read the data from the file
    performance_file = f"{_dir}\\performance.csv"
    constants_file = f"{_dir}\\constants.csv"
    performance = pd.read_csv(performance_file)
    constants = pd.read_csv(constants_file)

    smoothedPerformance = performance['frameDuration'].ewm(span=500).mean()
    # Plot
    plt.plot(performance['particleCount'], smoothedPerformance, linestyle='-', linewidth=2, label=name)

plt.xlabel('Anzahl an Partikel')
plt.ylabel('Simulationsschrittzeit (ms)')
plt.title(f"Prozessverarbeitung")
plt.legend()
plt.grid(True)
plt.show()
