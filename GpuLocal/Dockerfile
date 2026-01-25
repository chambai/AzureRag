#FROM ubuntu:latest
#LABEL authors="loz"
#
#ENTRYPOINT ["top", "-b"]

FROM nvidia/cuda:12.6.0-runtime-ubuntu22.04

WORKDIR /app

# Install Python 3 + pip + other basics
RUN apt-get update && \
apt-get install -y python3 python3-pip && \
rm -rf /var/lib/apt/lists/*

# Need a specific wheel for the torch CUDA version & it doesn't run from inside requirements.txt. Installing separately
#RUN pip3 install torch --index-url https://download.pytorch.org/whl/cu126
COPY requirements.txt .
# RUN pip3 install --no-cache-dir -r requirements.txt
RUN pip3 install fastapi
RUN pip3 install uvicorn
RUN pip3 install sentence-transformers
RUN pip3 install numpy

COPY main.py .

EXPOSE 8000

CMD ["python", "main.py"]
