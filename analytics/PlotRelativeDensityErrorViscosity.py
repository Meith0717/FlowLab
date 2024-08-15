import matplotlib.pyplot as plt
import pandas as pd

lst = ["20240813_161936", "20240813_162221", "20240813_162447", "20240813_173116"]

plt.figure(figsize=(10, 5))
plt.rcParams['font.family'] = 'serif'
plt.rcParams['font.size'] = 17 

plt.axhline(y=0, color='b', linewidth=3, linestyle='--')
for Dir in lst: 

    # Read the data from the file
    data_file = f"analytics\\{Dir}\\physics.csv"
    constants_file = f"analytics\\{Dir}\\constants.csv"
    data = pd.read_csv(data_file)
    constants = pd.read_csv(constants_file)

    # Plot
    plt.plot(data['sample'], data['relativeDensityError'], linestyle='-', linewidth=2, label=f"v = {constants["FluidViscosity"][0]}")

plt.ylim(0, .09)
plt.xlim(2000, 32000)

plt.xlabel('Simulationsschritte')
plt.ylabel('Relativer Dichtefehler')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig(r"report\\graphics\\Viskosität.png")
