//
// This custom View is referenced by SwiftUIInjectedScene
// to provide the body of a WindowGroup. It's part of the Unity-VisionOS
// target because it lives inside a "SwiftAppSupport" directory (and Unity
// will move it to that target).
//

import Foundation
import SwiftUI
import UnityFramework
import AVKit

struct VideoContentView: View {
    let videoUrlString: String
    
    init(_ videoUrlString: String) {
        self.videoUrlString = videoUrlString
    }
    
    var videoURL: URL {
        return Bundle.main.url(forResource: "Data/Raw/videos/" + videoUrlString, withExtension: "MOV")!
    }
    
    @State var isPlaying: Bool = false
    
    var body: some View {
        var player = AVPlayer(url: videoURL)
        VStack {
            VideoPlayer(player: player)
                .frame(width: 1600, height: 900, alignment: .center)
        }
    }
}

#Preview(windowStyle: .automatic) {
    VideoContentView("")
}
