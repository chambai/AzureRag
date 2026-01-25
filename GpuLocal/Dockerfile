FROM pytorch/pytorch:2.10.0-cuda12.6-cudnn9-runtime

WORKDIR /app

## Install Python 3 + pip + other basics
#RUN apt-get update && \
#apt-get install -y python3 python3-pip && \
#rm -rf /var/lib/apt/lists/*
#
## Need a specific wheel for the torch CUDA version & it doesn't run from inside requirements.txt. Installing separately
#RUN pip3 install torch --index-url https://download.pytorch.org/whl/cu126
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt

COPY main.py .

EXPOSE 8000

CMD ["python", "main.py"]
