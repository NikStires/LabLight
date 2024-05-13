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
        return Bundle.main.url(forResource: videoUrlString, withExtension: "m4v")!
    }
    
    @State var isPlaying: Bool = false
    
    var body: some View {
        var player = AVPlayer(url: videoURL)
        VStack {
            VideoPlayer(player: player)
                .frame(width: 320, height: 180, alignment: .center)
            
            
            Button {
                isPlaying ? player.pause() : player.play()
                isPlaying.toggle()
                player.seek(to: .zero)
            } label: {
                Image(systemName: isPlaying ? "stop" : "play")
                    .padding()
            }
        }
    }
}

#Preview(windowStyle: .automatic) {
    VideoContentView("")
}
