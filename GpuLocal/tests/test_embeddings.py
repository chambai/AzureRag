import numpy as np
from GpuLocal import embeddings


def test_embed_text_uses_model(mocker):
    fake_model = mocker.Mock()
    fake_model.encode.return_value = np.zeros((2, 5))

    mocker.patch(
        "GpuLocal.embeddings.get_model",
        return_value=fake_model
    )

    texts = ["a", "b"]
    vecs = embeddings.embed_text(texts)

    fake_model.encode.assert_called_once()
    assert vecs.shape == (2, 5)