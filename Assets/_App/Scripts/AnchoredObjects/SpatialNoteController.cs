using TMPro;
using UnityEngine;

/// <summary>
/// Controller for a spatialnote embedde in a anchored object
/// </summary>
public class SpatialNoteController : AnchorPayloadController
{
    [SerializeField]
    private TextMeshProUGUI text;

    protected override void UpdateView()
    {
        base.UpdateView();

        var note = payload as SpatialNotePayload;
        if (note != null)
        {
            text.text = note.text;
        }
    }

    // Spatial note specific
    public void Record()
    {
        Debug.Log("Record text");

        SpeechRecognizer.Instance.RecognizedTextHandler = HandleRecognizedText;
    }

    public void HandleRecognizedText(string recognizedText)
    {
        Debug.Log(recognizedText);
        text.text = recognizedText;

        var note = payload as SpatialNotePayload;
        if (note != null)
        {
            note.text = recognizedText;
            AnchoredObjectController.SaveAnchorData();
        }

        SpeechRecognizer.Instance.RecognizedTextHandler = null;
    }
}
