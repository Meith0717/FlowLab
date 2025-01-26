import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Load the CSV file
data = pd.read_csv("boundarieData\\simulation.csv")

# Extract the required columns
boundaries = data["boundary"].unique()
iterations = data["iterations"]

# Create a boxplot to show the range and mean of iterations for each boundary
plt.figure(figsize=(12, 8))
sns.boxplot(x="boundary", y="iterations", data=data, showmeans=True, meanprops={"marker":"o", "markerfacecolor":"red", "markeredgecolor":"black"})

# Add labels and title
plt.xlabel("Boundary")
plt.ylabel("Iterations")
plt.title("Iterations by Boundary\nShowing Ranges and Mean Values")
plt.grid(axis='y', linestyle='--', alpha=0.7)

# Show the plot
plt.tight_layout()
plt.show()
