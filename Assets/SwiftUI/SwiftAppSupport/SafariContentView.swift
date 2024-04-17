//
// This custom View is referenced by SwiftUIInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework
import SafariServices

struct SafariContentView: View {
    let safariUrlString:String
    
    init(_ safariUrlString: String) {
        self.safariUrlString = safariUrlString
    }
    
    var safariURL: URL {
        return URL(string: safariUrlString)!
    }
    
    @State private var isSafariViewPresented = false

    var body: some View {
        Button("Open Link") {
            // Present SafariView asynchronously
            DispatchQueue.main.async {
                isSafariViewPresented = true
            }
        }
        .fullScreenCover(isPresented: $isSafariViewPresented) {
            // Display SafariView as a sheet
            SafariView(url: safariURL)
        }
    }
}

struct SafariView: UIViewControllerRepresentable {
    let url: URL

    func makeUIViewController(context: Context) -> SFSafariViewController {
        return SFSafariViewController(url: url)
    }

    func updateUIViewController(_ uiViewController: SFSafariViewController, context: Context) {
        // Update the view controller if needed
    }
}
