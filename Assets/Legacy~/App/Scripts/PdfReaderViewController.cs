using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Paroxe.PdfRenderer;
using UniRx;
using TMPro;

public class PdfReaderViewController : MonoBehaviour
{
    [SerializeField]
    private GameObject pdfReader;
    Action disposeVoice;

    public TextMeshProUGUI noPdfText;

    void Awake()
    {
        ProtocolState.procedureStream.Subscribe(_ =>
        {
            if(ProtocolState.procedureDef.Value != null && ProtocolState.procedureDef.Value.pdfPath != null)
            {
                LoadPdf(ProtocolState.procedureDef.Value.pdfPath);
            }
            else
            {
                Destroy(pdfReader);
                Debug.Log("No pdf path provided for the procedure");
                noPdfText.gameObject.SetActive(true);
            }
        });
    }

    void OnDisable()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;
    }

    public void LoadPdf(string pdfPath)
    {
        if(!string.IsNullOrEmpty(pdfPath))
        {
            string folderName = "Procedure" + "/" + ProtocolState.procedureDef.Value.title;
            string fileName = Path.GetFileName(pdfPath);

            Debug.Log("Folder Name: " + folderName);
            Debug.Log("File Name: " + fileName);

            PDFViewer viewer = pdfReader.GetComponent<PDFViewer>();

            viewer.FileSource = PDFViewer.FileSourceType.Resources;
            viewer.FileName = fileName;
            viewer.Folder = folderName;
        }
    }

    void SetupVoiceCommands()
    {
        disposeVoice?.Invoke();
        disposeVoice = null;

        disposeVoice = ServiceRegistry.GetService<IVoiceController>()?.Listen(new Dictionary<string, Action>()
        {
            {"hide pdf", () =>
                {
                    pdfReader.SetActive(false);
                }
            },
            {"show pdf", () =>
                {
                    pdfReader.SetActive(true);
                }
            }
        });
    }
}
