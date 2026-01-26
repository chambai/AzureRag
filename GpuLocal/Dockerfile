FROM pytorch/pytorch:2.10.0-cuda12.6-cudnn9-runtime

# Install system dependencies for venv + pip
RUN apt-get update && \
apt-get install -y python3-venv build-essential && \
rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Create venv
RUN python -m venv /venv
ENV PATH="/venv/bin:$PATH"

# Install libraries in venv
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8000

CMD ["python", "main.py"]
