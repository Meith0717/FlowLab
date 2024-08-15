import matplotlib.pyplot as plt
import pandas as pd

plt.figure(figsize=(10, 6))
lst = ["20240813_161936", "20240813_162221", "20240813_162447", "20240813_173116"]

plt.axhline(y=0, color='b', linestyle='--')
for Dir in lst: 

    # Read the data from the file
    data_file = f"{Dir}\\physics.csv"
    constants_file = f"{Dir}\\constants.csv"
    data = pd.read_csv(data_file)
    constants = pd.read_csv(constants_file)

    # Plot
    plt.plot(data['sample'], data['relativeDensityError'], linestyle='-', linewidth=2, label=f"v = {constants["FluidViscosity"][0]}")

plt.ylim(0, .09)
plt.xlim(2000, 32000)

plt.xlabel('Simulationsschritte')
plt.ylabel('Relativer Dichtefehler')
plt.title(f"Kolonnentest mit Verschiedener Viskosität v")
plt.legend()
plt.grid(True)
plt.show()
