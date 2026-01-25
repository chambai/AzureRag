#FROM ubuntu:latest
#LABEL authors="loz"
#
#ENTRYPOINT ["top", "-b"]

FROM nvidia/cuda:12.6.0-runtime-ubuntu22.04

WORKDIR /app

COPY requirements.txt .
# Need a specific wheel for the torch CUDA version & it doesn't run from inside requirements.txt. Installing separately
RUN pip install torch --index-url https://download.pytorch.org/whl/cu126
RUN pip install --no-cache-dir -r requirements.txt

COPY main.py .

EXPOSE 8000

CMD ["python", "main.py"]
