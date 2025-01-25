import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from scipy.interpolate import make_interp_spline

# Load the CSV file
data = pd.read_csv("20250125_175859\\simulation.csv")

# Extract static values for title
solver = data["solver"].iloc[0]
boundary = data["boundary"].iloc[0]

# Calculate height based on particles
data["height"] = data["particles"] / 12

# Calculate the mean density error for each stiffness value
mean_density_error_per_stiffness = data.groupby("timeStep")["iterations"].mean()

# Create the plot
plt.figure(figsize=(10, 6))
plt.plot(mean_density_error_per_stiffness.index, mean_density_error_per_stiffness.values, label="Time Step", marker="o")
plt.xlabel("Time Step")
plt.ylabel("Solver Iterations")
plt.title(f"Time Step vs Solver Iterations\nSolver: {solver}, Boundary: {boundary}")
plt.legend()
plt.grid(True)

# Show the plot
plt.tight_layout()
plt.show()