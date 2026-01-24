from sentence_transformers import SentenceTransformer

# Load model on GPU
model = SentenceTransformer("all-mpnet-base-v2", device="cuda")  # A6000

def embed_text(texts):
    """
    texts: list of strings
    returns: list of vectors (numpy arrays)
    """
    embeddings = model.encode(texts, show_progress_bar=False, convert_to_numpy=True)
    return embeddings
