import numpy as np
from fastapi.testclient import TestClient
from GpuLocal import main


def fake_embedder(texts):
    return np.array([[42.0, 43.0, 44.0]])


def test_embed_endpoint_success():
    main.app.dependency_overrides[main.get_embedder] = lambda: fake_embedder

    client = TestClient(main.app)

    response = client.post(
        "/embed",
        json={"texts": ["hello"]}
    )

    assert response.status_code == 200
    assert response.json() == {
        "vectors": [[42.0, 43.0, 44.0]]
    }

    main.app.dependency_overrides.clear()

def failing_embedder(texts):
    raise RuntimeError("kaboom")


def test_embed_endpoint_error():
    main.app.dependency_overrides[main.get_embedder] = lambda: failing_embedder

    client = TestClient(main.app)

    response = client.post(
        "/embed",
        json={"texts": ["hello"]}
    )

    assert response.status_code == 500
    assert "kaboom" in response.json()["detail"]

    main.app.dependency_overrides.clear()