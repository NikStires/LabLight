//
// This custom View is referenced by SwiftUIInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework
import PDFKit

struct PDFContentView: View {
    let pdfUrlString: String
    
    init(_ pdfUrlString: String) {
        self.pdfUrlString = pdfUrlString
    }

    var pdfURL: URL {
        if pdfUrlString.hasPrefix("http") {
            return URL(string: pdfUrlString)!
        } else {
            return Bundle.main.url(forResource: "Data/Raw/pdf/" + pdfUrlString, withExtension: "pdf")!
        }
    }
    
    var body: some View {

        
        PDFKitView(url: pdfURL)
        .onAppear {
            // Call the public function that was defined in SwiftUISamplePlugin
            // inside UnityFramework
            CallCSharpCallback("appeared")
        }
    }
}

struct PDFKitView: UIViewRepresentable {
    
    let url: URL
    
    func makeUIView(context: Context) -> PDFView {
        let pdfView = PDFView()
        pdfView.document = PDFDocument(url: self.url)
        pdfView.autoScales = true
        return pdfView
    }
    
    func updateUIView(_ pdfView: PDFView, context: Context) {
        // Update pdf if needed
    }
}

#Preview(windowStyle: .automatic) {
    PDFContentView("")
}
