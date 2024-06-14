import matplotlib.pyplot as plt
import pandas as pd

# Read the data from the file
data_file = "20240614_102723\phisics.csv"
data = pd.read_csv(data_file)

# Plot all on one graph
plt.figure(figsize=(10, 6))

# plt.plot(data['sample'], data['localDensity'], linestyle='-', label='localDensity')
# plt.plot(data['sample'], data['localPressure'], linestyle='-', label='localPressure')

plt.plot(data['sample'], data['pressureAcceleration'], linestyle='-', label='pressureAcceleration')
# plt.plot(data['sample'], data['viscosityAcceleration'], linestyle='-', label='viscosityAcceleration')
plt.plot(data['sample'], data['averageVelocity'], linestyle='-', label='averageVelocity')

# plt.plot(data['sample'], data['pressureAcceleration.X'], linestyle='-', label='pressureAcceleration.X')
# plt.plot(data['sample'], data['pressureAcceleration.Y'], linestyle='-', label='pressureAcceleration.Y')
 
# plt.plot(data['sample'], data['viscosityAcceleration.X'], linestyle='-', label='viscosityAcceleration.X')
# plt.plot(data['sample'], data['viscosityAcceleration.Y'], linestyle='-', label='viscosityAcceleration.Y')
 
# plt.plot(data['sample'], data['averageVelocity.X'], linestyle='-', label='averageVelocity.X')
# plt.plot(data['sample'], data['averageVelocity.Y'], linestyle='-', label='averageVelocity.Y')
 
# plt.plot(data['sample'], data['CFL'], linestyle='-', label='CFL')

plt.xlabel('Sample')
plt.ylabel('Values')
plt.title('All metrics over Samples')
plt.legend()
plt.grid(True)
plt.show()
