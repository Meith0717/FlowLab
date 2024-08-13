import numpy as np
import matplotlib.pyplot as plt

import math
from math import pi
from numpy import array
import numpy as np

def kernel_alpha(particle_diameter):
    return 5 / (14 * pi * particle_diameter**2)

def distance_over_h(pos1, pos2, H):
    return np.linalg.norm(pos1 - pos2) / H

def kernel(position1, position2, particle_diameter):
    alpha = kernel_alpha(particle_diameter)
    distance_over_h_value = distance_over_h(position1, position2, particle_diameter)
    t1 = max(1 - distance_over_h_value, 0)
    t2 = max(2 - distance_over_h_value, 0)
    t3 = (t2**3) - 4 * (t1**3)
    return alpha * t3

def kernel_derivative(position1, position2, particle_diameter):
    position_difference = position1 - position2
    distance_over_h_value = distance_over_h(position1, position2, particle_diameter)
    if distance_over_h_value == 0:
        return np.array([0.0, 0.0])
    t1 = max(1 - distance_over_h_value, 0)
    t2 = max(2 - distance_over_h_value, 0)
    t3 = (-3 * t2**2) + (12 * t1**2)
    return kernel_alpha(particle_diameter) * (position_difference / (np.linalg.norm(position_difference) * particle_diameter)) * t3


# Function to plot W(q) from x = -2 to 2
def plot_W_from_x():
    plt.figure(figsize=(10, 5.5))
    for h in range(1, 4):
        x_values = np.linspace(-5.2, 5.2, 400)  # x from -2 to 2
        W_values = [kernel_derivative(x_values[i], 0, h) for i in range(0, 400) ]
        plt.plot(x_values, W_values, label=f'h={h}')
    plt.xlabel('xj - xi')
    plt.ylabel('∇W(xj-xi, h)') #('∇W(xj-xi, h)')
    plt.legend()
    plt.grid(True)
    plt.savefig('report\graphics\KernelDerivPlot.png', dpi=100)

# Example usage
plot_W_from_x()