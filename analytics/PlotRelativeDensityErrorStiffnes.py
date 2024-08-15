import matplotlib.pyplot as plt
import pandas as pd

lst = [ "20240813_152226", "20240813_134049", "20240813_133754", "20240813_134418", "20240813_153052"]

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

    # Calculate mean
    mean_value = data['relativeDensityError'].mean()

    # Plot
    plt.plot(data['sample'], data['relativeDensityError'], linestyle='-', linewidth=2, label=f"k = {constants["FluidStiffness"][0]}")

    # Annotate the mean value on the y-axis
    plt.text(30000, mean_value, f'{mean_value:.3f}', va='center', ha='left')

plt.ylim(0, .11)
plt.xlim(5000, 30000)

plt.xlabel('Simulationsschritte')
plt.ylabel('Relativer Dichtefehler')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig(r"report\\graphics\\Steifigkeit.png")
