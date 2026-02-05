from sentence_transformers import SentenceTransformer
from typing import List
import numpy as np

_model = None


def get_model():
    global _model
    if _model is None:
        _model = SentenceTransformer(
            "all-mpnet-base-v2",
            device="cuda"
        )
    return _model


def embed_text(texts: List[str]) -> np.ndarray:
    model = get_model()
    return model.encode(
        texts,
        show_progress_bar=False,
        convert_to_numpy=True
    )