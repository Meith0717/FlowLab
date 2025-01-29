import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import seaborn as sns
import statistics 
from mpl_toolkits.mplot3d import Axes3D

def remove_outliers(df, column):
    Q1 = df[column].quantile(0.25)
    Q3 = df[column].quantile(0.75)
    IQR = Q3 - Q1
    return df[(df[column] >= (Q1 - 1.5 * IQR)) & (df[column] <= (Q3 + 1.5 * IQR))]


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
    plt.savefig("Time Step vs Solver Iterations.png")

def Plot_DensityError_Stiffness(data):
    # Extract static values for title
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    timeStep = data["timeStep"].iloc[0]

    # Calculate the mean density error for each stiffness value
    mean_density_error_per_stiffness = data.groupby("stiffness")["absoluteError"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.index, mean_density_error_per_stiffness.values, label="Absolute Error", marker="o")
    plt.xlabel("Stiffness")
    plt.ylabel("Absolute Error")
    plt.title(f"Stiffness vs Density Error\nSolver: {solver}, Boundary: {boundary}")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()
    plt.savefig("experiments\\Stiffness vs Density Error.png")

def Plot_DensityError_Iterations(data):

    # Calculate the mean density error for each stiffness value
    data = data[(data["absoluteError"] < 0.1)]
    mean_density_error_per_stiffness = data.groupby("iterations")["absoluteError"].mean()

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(mean_density_error_per_stiffness.index, mean_density_error_per_stiffness.values, label="Density Error", marker="o")
    plt.xlabel("Iterations")
    plt.ylabel("Density Error")
    plt.legend()
    plt.grid(True)

    # Show the plot
    plt.tight_layout()

def Plot_3D_Gamma_SolverIterations(data):
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    particles = data["particles"].iloc[0]

    fig = plt.figure(figsize=(10, 6))
    ax = fig.add_subplot(111, projection='3d')

    data = data[(data["absoluteError"] < 0.01)]
    data = data[(data["cfl"] < 1)]
    grouped_data = data.groupby(["gamma1", "gamma2", "gamma3"]).agg({"iterations": "mean"}).reset_index()

    # Extract values
    gamma1_vals = grouped_data["gamma1"]
    gamma2_vals = grouped_data["gamma2"]
    gamma3_vals = grouped_data["gamma3"]
    iterations_vals = grouped_data["iterations"]

    # Find the minimum iteration value
    min_index = iterations_vals.idxmin()
    min_gamma1 = gamma1_vals.iloc[min_index]
    min_gamma2 = gamma2_vals.iloc[min_index]
    min_gamma3 = gamma3_vals.iloc[min_index]
    min_iterations = iterations_vals.iloc[min_index]

    # Scatter plot with color-coded iterations
    scatter = ax.scatter(gamma1_vals, gamma2_vals, gamma3_vals, c=iterations_vals, cmap='viridis', marker='o', alpha=0.8)

    # Add colorbar (legend for colors)
    cbar = fig.colorbar(scatter, ax=ax, shrink=0.6, aspect=10)
    cbar.set_label('Iterations', rotation=270, labelpad=15)

    # Annotate the minimum iteration point at its sample location
    ax.scatter(min_gamma1, min_gamma2, min_gamma3, color='red', s=100, edgecolor='black', label='Min Iteration')
    ax.text(min_gamma1, min_gamma2, min_gamma3, 
            f"Min: {min_iterations:.1f}\n(G1: {min_gamma1}, G2: {min_gamma2}, G3: {min_gamma3})", 
            fontsize=12, color='red', fontweight='bold', verticalalignment='bottom')

    # Labels
    ax.set_xlabel("Gamma 1")
    ax.set_ylabel("Gamma 2")
    ax.set_zlabel("Gamma 3")
    ax.set_title(f"{data["solver"].iloc[0]} for {particles} Particles with {data["boundary"].iloc[0]} | Time Step: {data ["timeStep"].iloc[0]} ")
    plt.savefig(f"{solver}_{boundary}Gamma.png")

def Plot_TimeStep_SearchPercentage(data):
    solver = data["solver"].iloc[0]
    boundary = data["boundary"].iloc[0]
    data["search_percentage"] = (data["neighborSearchTime"] / data["simulationStepsTime"]) * 100
    data = remove_outliers(data, "search_percentage")
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

Plot_3D_Gamma_SolverIterations(pd.read_csv("extrapolationGamma.csv"))
Plot_3D_Gamma_SolverIterations(pd.read_csv("mirroringGamma.csv"))