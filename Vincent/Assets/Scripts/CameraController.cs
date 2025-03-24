using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public float targetOrthographicSize = 3f;
    public float zoomDuration = 2.5f;
    
    public IEnumerator ZoomIn() {
        float startSize = Camera.main.orthographicSize;
        float timer = 0f;
        while (timer < zoomDuration) {
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetOrthographicSize, timer / zoomDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        Camera.main.orthographicSize = targetOrthographicSize;
    }
}
