import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import seaborn as sns

def Plot_Boundary_Handling(data):
    # Extract the required columns
    boundaries = data["boundary"].unique()
    iterations = data["iterations"]

    # Create a boxplot to show the range and mean of iterations for each boundary
    plt.figure(figsize=(10, 6))
    sns.boxplot(x="iterations", y="boundary", data=data, showfliers=False, showmeans=True, meanprops={"marker":"o", "markerfacecolor":"red", "markeredgecolor":"black"})

    # Add labels and title
    plt.xlabel("Iterations")
    plt.ylabel("")
    plt.title("Iterations by Boundary handling\nShowing Ranges and Mean Values")
    plt.grid(axis='y', linestyle='--', alpha=0.7)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Iterations by Boundary handling")

def Plot_ColumnHeight_SolverIterations(data):
    # Extract static values for title
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    timeStep = data["timeStep"].iloc[0]

    # Calculate height based on particles
    data["height"] = data["particles"] / 12

    # Calculate the mean density error for each stiffness value
    mean_density_error_per_stiffness = data.groupby("height")["iterations"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.values, mean_density_error_per_stiffness.index, label="Iterations", marker="o")
    plt.xlabel("Solver Iterations")
    plt.ylabel("Column Height")
    plt.title(f"Column Height vs Solver Iterations\nSolver: {solver}, Boundary: {boundary}, Time Step: {timeStep}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Column Height vs Solver Iterations.png")

def Plot_TimeStep_SolverIterations(data):
    # Extract static values for title
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]

    # Calculate height based on particles
    data["height"] = data["particles"] / 12

    # Calculate the mean density error for each stiffness value
    mean_density_error_per_stiffness = data.groupby("timeStep")["iterations"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.values, mean_density_error_per_stiffness.index, label="Time Step", marker="o")
    plt.ylabel("Time Step")
    plt.xlabel("Solver Iterations")
    plt.title(f"Time Step vs Solver Iterations\nSolver: {solver}, Boundary: {boundary}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Time Step vs Solver Iterations.png")

def Plot_DensityError_Stiffness(data):
    # Extract static values for title
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    timeStep = data["timeStep"].iloc[0]

    # Calculate the mean density error for each stiffness value
    mean_density_error_per_stiffness = data.groupby("stiffness")["densityError"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.index, mean_density_error_per_stiffness.values, label="Density Error", marker="o")
    plt.xlabel("Stiffness")
    plt.ylabel("Density Error")
    plt.title(f"Stiffness vs Density Error\nSolver: {solver}, Boundary: {boundary}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Stiffness vs Density Error.png")

data = pd.read_csv("experiments\\stiffnesData\\simulation.csv")
data1 = pd.read_csv("experiments\\20250127_202349\\simulation.csv")
data2 = pd.read_csv("experiments\\columnHeightData\\simulation.csv")
data3 = pd.read_csv("experiments\\timestepData\\simulation.csv")

Plot_DensityError_Stiffness(data)
Plot_Boundary_Handling(data1)
Plot_ColumnHeight_SolverIterations(data2)
Plot_TimeStep_SolverIterations(data3)