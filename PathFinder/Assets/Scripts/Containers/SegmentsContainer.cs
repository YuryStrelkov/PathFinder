using UnityEngine;

public class SegmentsContainer : MonoBehaviour
{
    private static SegmentsContainer _instance;
    public static SegmentsContainer Instance => _instance;
    void Awake() => _instance = this;
}
