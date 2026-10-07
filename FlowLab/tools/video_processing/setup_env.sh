#!/bin/bash
# FlowLab Video Processing Environment Setup
# For Ubuntu 24.04.5 LTS

set -e

echo "🎬 Setting up FlowLab Video Processing Environment..."

# Check if python3 is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Python3 not found. Installing..."
    sudo apt update
    sudo apt install -y python3 python3-pip python3-venv
fi

# Check if pip is installed
if ! command -v pip3 &> /dev/null; then
    echo "❌ pip3 not found. Installing..."
    sudo apt install -y python3-pip
fi

# Check if venv is available
if ! python3 -m venv --help &> /dev/null; then
    echo "❌ python3-venv not found. Installing..."
    sudo apt install -y python3-venv
fi

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ENV_DIR="${SCRIPT_DIR}/venv"

echo "📁 Environment directory: ${ENV_DIR}"

# Create virtual environment if it doesn't exist
if [ ! -d "${ENV_DIR}" ]; then
    echo "🆕 Creating virtual environment..."
    python3 -m venv "${ENV_DIR}"
else
    echo "✅ Virtual environment already exists"
fi

# Activate and install dependencies
echo "📦 Installing dependencies..."
source "${ENV_DIR}/bin/activate"
pip install --upgrade pip
pip install -r "${SCRIPT_DIR}/requirements.txt"

echo ""
echo "✅ Environment setup complete!"
echo ""
echo "To use this environment:"
echo "  1. Navigate to: ${SCRIPT_DIR}"
echo "  2. Activate: source venv/bin/activate"
echo "  3. Run script: python create_pip_video.py /path/to/recording/ -o output.mp4"
echo ""
echo "To deactivate: deactivate"
