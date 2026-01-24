from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from embeddings import embed_text
import uvicorn

app = FastAPI(title="Local Embedding Service")

class TextRequest(BaseModel):
    texts: list[str]

@app.post("/embed")
def get_embeddings(req: TextRequest):
    try:
        vectors = embed_text(req.texts)
        return {"vectors": vectors.tolist()}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    # Keep server running
    uvicorn.run(app, host="0.0.0.0", port=8000)
