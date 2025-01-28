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

def Plot__Gamma_SolverIterations(gamma1, gamma2, gamma3):
    # Extract static values for title
    solver1 = gamma1["solver"].iloc[0]
    solver2 = gamma2["solver"].iloc[0]
    solver3 = gamma3["solver"].iloc[0]
    boundary1 = gamma1["boundary"].iloc[0]
    boundary2 = gamma2["boundary"].iloc[0]
    boundary3 = gamma3["boundary"].iloc[0]

    assert(solver1 == solver2 and solver2 == solver3)
    assert(boundary1 == boundary2 and boundary2 == boundary3)

    # Calculate the mean density error for each stiffness value
    gamma1_grouped = gamma1.groupby("gamma1")["iterations"].mean()
    gamma2_grouped = gamma2.groupby("gamma2")["iterations"].mean()
    gamma3_grouped = gamma3.groupby("gamma3")["iterations"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(gamma1_grouped.index, gamma1_grouped.values, label="Gamma 1", marker="o")
    plt.plot(gamma2_grouped.index, gamma2_grouped.values, label="Gamma 2", marker="o")
    plt.plot(gamma3_grouped.index, gamma3_grouped.values, label="Gamma 3", marker="o")
    plt.xlabel("Gamma")
    plt.ylabel("Solver Iterations")
    plt.title(f"Gamma vs Solver Iterations\nSolver: {solver1}, Boundary: {boundary1}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig(f"{solver1}_{boundary1}Gamma.png")

def Plot_Gamma2_SolverIterations(data):
    # Extract static values for title
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    timeStep = data["timeStep"].iloc[0]

    # Calculate the mean density error for each stiffness value
    mean_density_error_per_stiffness = data.groupby("gamma2")["iterations"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.index, mean_density_error_per_stiffness.values, label="Iterations", marker="o")
    plt.xlabel("Gamma 2")
    plt.ylabel("Solver Iterations")
    plt.title(f"Gamma 2 vs Solver Iterations\nSolver: {solver}, Boundary: {boundary}, Time Step: {timeStep}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Gamma 2 vs Solver Iterations.png")

def Plot_TimeStep_SearchPercentage(data):

    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    data["search_percentage"] = (data["neighborSearchTime"] / data["simulationStepsTime"]) * 100
    mean_search_percentage = data.groupby("timeStep")["search_percentage"].mean()
    plt.figure(figsize=(10, 6))
    plt.plot(mean_search_percentage.index, mean_search_percentage.values, label="Neighbor Search %", marker="o")
    plt.xlabel("TimeStep")
    plt.ylabel("Neighbor Search Time (%)")
    plt.title(f"TimeStep vs Neighbor search part\nSolver: {solver}, Boundary: {boundary}")
    plt.legend()
    plt.grid(True)
    plt.tight_layout()
    plt.savefig(f"{solver}TimeStepIterations.png")

data = pd.read_csv("IISPHTimeStepIterations.csv")
Plot_TimeStep_SearchPercentage(data)

data = pd.read_csv("extrapolationGamma1.csv")
data1 = pd.read_csv("extrapolationGamma2.csv")
data2 = pd.read_csv("extrapolationGamma3.csv")
Plot__Gamma_SolverIterations(data, data1, data2)