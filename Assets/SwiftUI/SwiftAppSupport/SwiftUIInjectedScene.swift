// Any swift file whose name ends in "InjectedScene" is expected to contain
// a computed static "scene" property like the one below. It will be injected to the top
// level App's Scene. The name of the class/struct must match the name of the file.

import Foundation
import SwiftUI

struct SwiftUIInjectedScene {
    @SceneBuilder
    static var scene: some Scene {
        // You can create multiple WindowGroups here for different wnidows;
        // they need a distinct id. If you include multiple items,
        // the scene property must be decorated with "@SceneBuilder" as above.
        WindowGroup(id: "PDF", for: String.self) { $pdfUrl in
            PDFContentView(pdfUrl!)
        }
        .defaultSize(width: 500.0, height: 800.0)
        
        WindowGroup(id: "Video", for: String.self) { $videoUrl in
            VideoContentView(videoUrl!)
        }
        .defaultSize(width: 900.0, height: 700.0)

        
        WindowGroup(id: "SimpleText") {
            Text("PDF Window")
        }
    }
}
