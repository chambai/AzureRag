from fastapi import FastAPI, HTTPException, Depends
from pydantic import BaseModel
import uvicorn
import numpy as np

app = FastAPI(title="Local Embedding Service")


class TextRequest(BaseModel):
    texts: list[str]


def get_embedder():
    return embed_text


@app.post("/embed")
def get_embeddings(
    req: TextRequest,
    embedder=Depends(get_embedder)
):
    try:
        vectors = embedder(req.texts)
        return {"vectors": vectors.tolist()}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    from embeddings import embed_text
    uvicorn.run(app, host="0.0.0.0", port=9000)
else:
    # (for unit testing)
    from .embeddings import embed_text
